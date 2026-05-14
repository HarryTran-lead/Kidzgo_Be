using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Payroll;
using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Lookups.GetLookups;

public sealed class GetLookupsQueryHandler : IQueryHandler<GetLookupsQuery, GetLookupsResponse>
{
    public Task<Result<GetLookupsResponse>> Handle(GetLookupsQuery query, CancellationToken cancellationToken)
    {
        var lookups = new Dictionary<string, List<LookupItemDto>>();

        // AttendanceStatus
        lookups["attendanceStatus"] = Enum.GetValues<AttendanceStatus>()
            .Select(e => new LookupItemDto { Value = e.ToString(), DisplayName = e.ToString() })
            .ToList();

        // SessionType (ParticipationType)
        lookups["sessionType"] = Enum.GetValues<ParticipationType>()
            .Select(e => new LookupItemDto { Value = e.ToString(), DisplayName = e.ToString() })
            .ToList();

        // SectionType
        lookups["sectionType"] = Enum.GetValues<SectionType>()
            .Select(e => new LookupItemDto { Value = e.ToString(), DisplayName = e.ToString() })
            .ToList();

        // SessionStatus
        lookups["sessionStatus"] = Enum.GetValues<SessionStatus>()
            .Select(e => new LookupItemDto { Value = e.ToString(), DisplayName = e.ToString() })
            .ToList();

        // ClassStatus
        lookups["classStatus"] = Enum.GetValues<ClassStatus>()
            .Select(e => new LookupItemDto { Value = e.ToString(), DisplayName = e.ToString() })
            .ToList();

        // EnrollmentStatus
        lookups["enrollmentStatus"] = Enum.GetValues<EnrollmentStatus>()
            .Select(e => new LookupItemDto { Value = e.ToString(), DisplayName = e.ToString() })
            .ToList();

        // InvoiceStatus
        lookups["invoiceStatus"] = Enum.GetValues<InvoiceStatus>()
            .Select(e => new LookupItemDto { Value = e.ToString(), DisplayName = e.ToString() })
            .ToList();

        // HomeworkStatus
        lookups["homeworkStatus"] = Enum.GetValues<HomeworkStatus>()
            .Select(e => new LookupItemDto { Value = e.ToString(), DisplayName = e.ToString() })
            .ToList();

        // SessionRoleType
        lookups["sessionRoleType"] = Enum.GetValues<SessionRoleType>()
            .Select(e => new LookupItemDto { Value = e.ToString(), DisplayName = e.ToString() })
            .ToList();

        return Task.FromResult<Result<GetLookupsResponse>>(
            new GetLookupsResponse
            {
                Lookups = lookups
            });
    }
}

