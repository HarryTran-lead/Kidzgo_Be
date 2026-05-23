using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Sessions.GetSessionLessonPlanDocument;

public sealed class GetSessionLessonPlanDocumentQuery : IQuery<GetSessionLessonPlanDocumentResponse>
{
    public Guid SessionId { get; init; }
}
