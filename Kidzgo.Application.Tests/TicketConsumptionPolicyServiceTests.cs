using Kidzgo.Application.Services;
using Kidzgo.Domain.Sessions;
using Xunit;

namespace Kidzgo.Application.Tests;

public sealed class TicketConsumptionPolicyServiceTests
{
    private readonly TicketConsumptionPolicyService _service = new();

    [Fact]
    public void Evaluate_consumes_ticket_for_main_present_normal_session()
    {
        var result = _service.Evaluate(
            AttendanceStatus.Present,
            null,
            ParticipationType.Main,
            SectionType.Normal);

        Assert.True(result.ShouldConsumeTicket);
        Assert.Equal(1, result.Quantity);
        Assert.True(result.AdvanceLessonProgression);
    }

    [Fact]
    public void Evaluate_does_not_consume_ticket_for_free_present_normal_session()
    {
        var result = _service.Evaluate(
            AttendanceStatus.Present,
            null,
            ParticipationType.Free,
            SectionType.Normal);

        Assert.False(result.ShouldConsumeTicket);
        Assert.Equal(0, result.Quantity);
        Assert.True(result.AdvanceLessonProgression);
    }

    [Fact]
    public void Evaluate_consumes_ticket_for_main_absent_without_notice()
    {
        var result = _service.Evaluate(
            AttendanceStatus.Absent,
            AbsenceType.NoNotice,
            ParticipationType.Main,
            SectionType.Normal);

        Assert.True(result.ShouldConsumeTicket);
        Assert.Equal(1, result.Quantity);
        Assert.False(result.AdvanceLessonProgression);
    }

    [Fact]
    public void Evaluate_does_not_consume_ticket_for_main_absent_with_notice()
    {
        var result = _service.Evaluate(
            AttendanceStatus.Absent,
            AbsenceType.WithNotice24H,
            ParticipationType.Main,
            SectionType.Normal);

        Assert.False(result.ShouldConsumeTicket);
        Assert.Equal(0, result.Quantity);
        Assert.False(result.AdvanceLessonProgression);
    }

    [Fact]
    public void Evaluate_does_not_consume_ticket_for_makeup_session()
    {
        var result = _service.Evaluate(
            AttendanceStatus.Makeup,
            null,
            ParticipationType.Main,
            SectionType.Makeup);

        Assert.False(result.ShouldConsumeTicket);
        Assert.Equal(0, result.Quantity);
        Assert.False(result.AdvanceLessonProgression);
    }

    [Fact]
    public void Selectable_participation_types_only_expose_main_and_free()
    {
        Assert.Equal(
            [ParticipationType.Main, ParticipationType.Free],
            ParticipationTypeRules.SelectableValues);
    }
}
