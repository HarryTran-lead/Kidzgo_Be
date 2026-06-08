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
        ParticipationType participationType,
        SectionType sectionType)
    {
        var canConsumeTicket = ParticipationTypeRules.ShouldConsumeTicket(participationType);
        var shouldConsume = canConsumeTicket && (attendanceStatus switch
        {
            AttendanceStatus.Present => true,
            AttendanceStatus.Absent when absenceType == AbsenceType.NoNotice => true,
            _ => false
        });

        var shouldAdvanceLesson = sectionType == SectionType.Normal &&
                                  attendanceStatus == AttendanceStatus.Present;

        var reason = !canConsumeTicket
            ? $"No ticket consumption for {participationType} participation"
            : shouldConsume
            ? $"Attendance {attendanceStatus} in {sectionType} section"
            : $"No ticket consumption for attendance {attendanceStatus}";

        return new TicketConsumptionDecision(
            shouldConsume,
            shouldConsume ? 1 : 0,
            shouldAdvanceLesson,
            reason);
    }
}
