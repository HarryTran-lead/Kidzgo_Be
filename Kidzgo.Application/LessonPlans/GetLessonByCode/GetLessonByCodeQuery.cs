using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.LessonPlans.GetLessonByCode;

public sealed class GetLessonByCodeQuery : IQuery<GetLessonByCodeResponse>
{
    public string LessonCode { get; init; } = null!;
}
