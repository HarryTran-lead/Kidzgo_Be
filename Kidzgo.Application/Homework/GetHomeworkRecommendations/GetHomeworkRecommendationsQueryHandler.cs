using System.Text.Json;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Homework;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Homework.Shared;
using Kidzgo.Application.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.Homework.Errors;
using Kidzgo.Domain.Users.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Homework.GetHomeworkRecommendations;

public sealed class GetHomeworkRecommendationsQueryHandler(
    IDbContext context,
    IUserContext userContext,
    IAiHomeworkAssistant aiHomeworkAssistant
) : IQueryHandler<GetHomeworkRecommendationsQuery, GetHomeworkRecommendationsResponse>
{
    public async Task<Result<GetHomeworkRecommendationsResponse>> Handle(
        GetHomeworkRecommendationsQuery query,
        CancellationToken cancellationToken)
    {
        var studentId = userContext.StudentId;

        if (!studentId.HasValue)
        {
            return Result.Failure<GetHomeworkRecommendationsResponse>(ProfileErrors.StudentNotFound);
        }

        var homeworkStudent = await context.HomeworkStudents
            .Include(hs => hs.Assignment)
                .ThenInclude(a => a.Class)
            .FirstOrDefaultAsync(hs => hs.Id == query.HomeworkStudentId, cancellationToken);

        if (homeworkStudent is null)
        {
            return Result.Failure<GetHomeworkRecommendationsResponse>(
                HomeworkErrors.SubmissionNotFound(query.HomeworkStudentId));
        }

        if (homeworkStudent.StudentProfileId != studentId.Value)
        {
            return Result.Failure<GetHomeworkRecommendationsResponse>(
                HomeworkErrors.SubmissionUnauthorized);
        }

        if (!homeworkStudent.Assignment.AiRecommendEnabled)
        {
            return Result.Failure<GetHomeworkRecommendationsResponse>(
                HomeworkErrors.AiRecommendNotEnabled);
        }

        var aiResult = await aiHomeworkAssistant.GetRecommendationsAsync(
            new AiHomeworkRecommendationRequest
            {
                Context = HomeworkAiContextMapper.BuildContext(homeworkStudent.Assignment, studentId.Value),
                LatestScore = homeworkStudent.Score.HasValue ? (float)homeworkStudent.Score.Value : null,
                MaxScore = homeworkStudent.Assignment.MaxScore.HasValue ? (float)homeworkStudent.Assignment.MaxScore.Value : null,
                TeacherFeedback = homeworkStudent.TeacherFeedback,
                AiFeedback = homeworkStudent.AiFeedback,
                StudentAnswerText = !string.IsNullOrWhiteSpace(query.CurrentAnswerText)
                    ? query.CurrentAnswerText
                    : homeworkStudent.TextAnswer,
                Language = string.IsNullOrWhiteSpace(query.Language) ? "vi" : query.Language
            },
            cancellationToken);

        var maxItems = Math.Clamp(query.MaxItems, 1, 10);
        var preferredQuestionType = HomeworkAiContextMapper.GetPreferredQuestionType(homeworkStudent.Assignment);
        var candidatesQuery = context.QuestionBankItems
            .Where(q => q.ProgramId == homeworkStudent.Assignment.Class.ProgramId && !q.IsDeleted);

        if (preferredQuestionType.HasValue)
        {
            candidatesQuery = candidatesQuery.Where(q => q.QuestionType == preferredQuestionType.Value);
        }

        var candidates = await candidatesQuery
            .OrderByDescending(q => q.CreatedAt)
            .Take(200)
            .ToListAsync(cancellationToken);

        var items = candidates
            .Select(item => RankItem(item, aiResult))
            .OrderByDescending(x => x.Score)
            .ThenByDescending(x => x.Item.CreatedAt)
            .Take(maxItems)
            .Select(x => new RecommendedPracticeItemDto
            {
                QuestionBankItemId = x.Item.Id,
                QuestionText = x.Item.QuestionText,
                QuestionType = x.Item.QuestionType.ToString(),
                Options = x.Item.Options == null
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(x.Item.Options) ?? new List<string>(),
                Topic = x.Item.Topic,
                Skill = x.Item.Skill,
                GrammarTags = StringListJson.Deserialize(x.Item.GrammarTags),
                VocabularyTags = StringListJson.Deserialize(x.Item.VocabularyTags),
                Level = x.Item.Level.ToString(),
                Points = x.Item.Points,
                Reason = x.Reason
            })
            .ToList();

        return new GetHomeworkRecommendationsResponse
        {
            AiUsed = aiResult.AiUsed,
            Summary = aiResult.Result.Summary,
            FocusSkill = aiResult.Result.FocusSkill,
            Topics = aiResult.Result.Topics,
            GrammarTags = aiResult.Result.GrammarTags,
            VocabularyTags = aiResult.Result.VocabularyTags,
            RecommendedLevels = aiResult.Result.RecommendedLevels,
            PracticeTypes = aiResult.Result.PracticeTypes,
            Warnings = aiResult.Result.Warnings,
            Items = items
        };
    }

    private static RankedQuestionBankItem RankItem(
        QuestionBankItem item,
        AiHomeworkRecommendationResult aiResult)
    {
        var score = 0;
        var matched = new List<string>();

        var topics = Normalize(aiResult.Result.Topics);
        var grammarTags = Normalize(aiResult.Result.GrammarTags);
        var vocabularyTags = Normalize(aiResult.Result.VocabularyTags);
        var levels = Normalize(aiResult.Result.RecommendedLevels);
        var focusSkill = (aiResult.Result.FocusSkill ?? string.Empty).Trim().ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(item.Topic) && topics.Contains(item.Topic.Trim().ToLowerInvariant()))
        {
            score += 6;
            matched.Add("cung topic");
        }

        if (!string.IsNullOrWhiteSpace(item.Skill) && item.Skill.Trim().ToLowerInvariant() == focusSkill)
        {
            score += 4;
            matched.Add("dung skill can luyen");
        }

        var itemGrammar = Normalize(StringListJson.Deserialize(item.GrammarTags));
        var itemVocabulary = Normalize(StringListJson.Deserialize(item.VocabularyTags));
        var grammarOverlap = itemGrammar.Intersect(grammarTags).Count();
        var vocabularyOverlap = itemVocabulary.Intersect(vocabularyTags).Count();

        if (grammarOverlap > 0)
        {
            score += grammarOverlap * 3;
            matched.Add("trung grammar tags");
        }

        if (vocabularyOverlap > 0)
        {
            score += vocabularyOverlap * 2;
            matched.Add("trung vocabulary tags");
        }

        if (levels.Contains(item.Level.ToString().ToLowerInvariant()))
        {
            score += 3;
            matched.Add("do kho phu hop");
        }

        if (score == 0)
        {
            matched.Add("goi y cung chuong trinh");
        }

        return new RankedQuestionBankItem(item, score, string.Join(", ", matched));
    }

    private static HashSet<string> Normalize(IEnumerable<string> values)
        => values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLowerInvariant())
            .ToHashSet();

    private sealed record RankedQuestionBankItem(
        QuestionBankItem Item,
        int Score,
        string Reason);
}
