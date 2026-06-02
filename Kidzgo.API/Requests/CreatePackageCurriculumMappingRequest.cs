namespace Kidzgo.API.Requests;

public sealed class CreatePackageCurriculumMappingRequest
{
    public Guid PackageId { get; init; }
    public Guid SyllabusId { get; init; }
}
