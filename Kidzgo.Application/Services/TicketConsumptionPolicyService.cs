using Kidzgo.Domain.Sessions;

namespace Kidzgo.Application.Services;

public sealed record TicketConsumptionDecision(
    bool ShouldConsumeTicket,
    int Quantity,
    bool AdvanceLessonProgression,
    string Reason);

public sealed class TicketConsumptionPolicyService
{
    public TicketConsumptionDecision Evaluate(
        AttendanceStatus? attendanceStatus,
        AbsenceType? absenceType,
        SectionType sectionType)
    {
        var shouldConsume = attendanceStatus switch
        {
            AttendanceStatus.Present => true,
            AttendanceStatus.Absent when absenceType == AbsenceType.NoNotice => true,
            _ => false
        };

        var shouldAdvanceLesson = sectionType == SectionType.Normal &&
                                  attendanceStatus == AttendanceStatus.Present;

        var reason = shouldConsume
            ? $"Attendance {attendanceStatus} in {sectionType} section"
            : $"No ticket consumption for attendance {attendanceStatus}";

        return new TicketConsumptionDecision(
            shouldConsume,
            shouldConsume ? 1 : 0,
            shouldAdvanceLesson,
            reason);
    }
}
