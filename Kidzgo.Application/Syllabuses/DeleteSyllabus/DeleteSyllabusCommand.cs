using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Syllabuses.DeleteSyllabus;

public sealed class DeleteSyllabusCommand : ICommand<DeleteSyllabusResponse>
{
    public Guid Id { get; init; }
}
