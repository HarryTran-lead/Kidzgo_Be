using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Classes;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Homework;
using Kidzgo.Domain.LessonPlans;
using Kidzgo.Domain.Payroll;
using Kidzgo.Domain.Sessions;
using Microsoft.EntityFrameworkCore;
using Kidzgo.Application.Abstraction.Data;

namespace Kidzgo.Application.Lookups.GetLookups;

public sealed class GetLookupsQueryHandler(IDbContext context) : IQueryHandler<GetLookupsQuery, GetLookupsResponse>
{
    public async Task<Result<GetLookupsResponse>> Handle(GetLookupsQuery query, CancellationToken cancellationToken)
    {
        var lookups = new Dictionary<string, List<LookupItemDto>>();

        // AttendanceStatus
        lookups["attendanceStatus"] = Enum.GetValues<AttendanceStatus>()
            .Select(e => new LookupItemDto { Value = e.ToString(), DisplayName = e.ToString() })
            .ToList();

        // ParticipationType
        lookups["participationType"] = ParticipationTypeRules.SelectableValues
            .Select(e => new LookupItemDto { Value = e.ToString(), DisplayName = e.ToString() })
            .ToList();

        // SectionType (also exposed as SessionType in Phase 1.5)
        lookups["sectionType"] = Enum.GetValues<SectionType>()
            .Select(e => new LookupItemDto { Value = e.ToString(), DisplayName = e.ToString() })
            .ToList();
        lookups["sessionType"] = lookups["sectionType"];

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

        lookups["slotType"] = await context.SlotTypes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new LookupItemDto
            {
                Value = x.Id.ToString(),
                DisplayName = x.Code
            })
            .ToListAsync(cancellationToken);

        lookups["learningTicketType"] = await context.LearningTicketTypes
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new LookupItemDto
            {
                Value = x.Id.ToString(),
                DisplayName = x.Code
            })
            .ToListAsync(cancellationToken);

        return new GetLookupsResponse
        {
            Lookups = lookups
        };
    }
}

