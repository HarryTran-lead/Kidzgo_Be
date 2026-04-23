using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Domain.Homework;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.QuestionBank.Shared;

public sealed class QuestionBankMatrixSelectionRequest
{
    public Guid ProgramId { get; init; }
    public HomeworkQuestionType QuestionType { get; init; }
    public string? Skill { get; init; }
    public string? Topic { get; init; }
    public bool ShuffleQuestions { get; init; } = true;
    public IReadOnlyCollection<QuestionBankMatrixLevelCount> Distribution { get; init; } = Array.Empty<QuestionBankMatrixLevelCount>();
}

public sealed class QuestionBankMatrixLevelCount
{
    public QuestionLevel Level { get; init; }
    public int Count { get; init; }
}

public sealed class QuestionBankMatrixSelectionResult
{
    public List<QuestionBankMatrixLevelCount> NormalizedDistribution { get; init; } = new();
    public List<QuestionBankItem> SelectedItems { get; init; } = new();
    public int RequestedQuestionCount { get; init; }
    public bool HasInvalidDistribution { get; init; }
    public QuestionLevel? InsufficientLevel { get; init; }
    public int RequiredCount { get; init; }
    public int AvailableCount { get; init; }
}

internal static class QuestionBankMatrixSelector
{
    public static async Task<QuestionBankMatrixSelectionResult> SelectAsync(
        IDbContext context,
        QuestionBankMatrixSelectionRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedDistribution = request.Distribution
            .Where(x => x.Count > 0)
            .GroupBy(x => x.Level)
            .Select(g => new QuestionBankMatrixLevelCount
            {
                Level = g.Key,
                Count = g.Sum(x => x.Count)
            })
            .ToList();

        if (normalizedDistribution.Count == 0)
        {
            return new QuestionBankMatrixSelectionResult
            {
                HasInvalidDistribution = true
            };
        }

        var levels = normalizedDistribution.Select(d => d.Level).ToList();
        var bankItemsQuery = context.QuestionBankItems
            .AsNoTracking()
            .Where(q =>
                q.ProgramId == request.ProgramId &&
                !q.IsDeleted &&
                q.QuestionType == request.QuestionType &&
                levels.Contains(q.Level));

        if (!string.IsNullOrWhiteSpace(request.Skill))
        {
            var skill = request.Skill.Trim().ToLowerInvariant();
            bankItemsQuery = bankItemsQuery.Where(q => q.Skill != null && q.Skill.ToLower().Contains(skill));
        }

        if (!string.IsNullOrWhiteSpace(request.Topic))
        {
            var topic = request.Topic.Trim().ToLowerInvariant();
            bankItemsQuery = bankItemsQuery.Where(q => q.Topic != null && q.Topic.ToLower().Contains(topic));
        }

        var bankItems = await bankItemsQuery.ToListAsync(cancellationToken);
        var groupedItems = bankItems
            .GroupBy(q => q.Level)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var level in normalizedDistribution)
        {
            groupedItems.TryGetValue(level.Level, out var itemsForLevel);
            var availableCount = itemsForLevel?.Count ?? 0;
            if (availableCount < level.Count)
            {
                return new QuestionBankMatrixSelectionResult
                {
                    NormalizedDistribution = normalizedDistribution,
                    RequestedQuestionCount = normalizedDistribution.Sum(x => x.Count),
                    InsufficientLevel = level.Level,
                    RequiredCount = level.Count,
                    AvailableCount = availableCount
                };
            }
        }

        var random = new Random();
        var selectedItems = new List<QuestionBankItem>();

        foreach (var level in normalizedDistribution)
        {
            var pool = groupedItems[level.Level];
            if (request.ShuffleQuestions)
            {
                Shuffle(pool, random);
            }
            else
            {
                pool = pool
                    .OrderBy(x => x.CreatedAt)
                    .ThenBy(x => x.QuestionText)
                    .ToList();
            }

            selectedItems.AddRange(pool.Take(level.Count));
        }

        if (request.ShuffleQuestions)
        {
            Shuffle(selectedItems, random);
        }
        else
        {
            selectedItems = selectedItems
                .OrderBy(x => x.Level)
                .ThenBy(x => x.CreatedAt)
                .ThenBy(x => x.QuestionText)
                .ToList();
        }

        return new QuestionBankMatrixSelectionResult
        {
            NormalizedDistribution = normalizedDistribution,
            SelectedItems = selectedItems,
            RequestedQuestionCount = normalizedDistribution.Sum(x => x.Count)
        };
    }

    private static void Shuffle<T>(IList<T> list, Random random)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
