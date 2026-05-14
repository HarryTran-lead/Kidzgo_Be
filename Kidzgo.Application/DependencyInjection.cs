using FluentValidation;
using Kidzgo.Application.Abstraction.Behaviors;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.ProgramProgressions.Shared;
using Kidzgo.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Kidzgo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            config.AddOpenBehavior(typeof(LoggingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        services.AddScoped<ISchedulePatternParser, RRuleSchedulePatternParser>();
        services.AddScoped<SessionConflictChecker>();
        services.AddScoped<StudentSessionAssignmentService>();
        services.AddScoped<SessionParticipantService>();
        services.AddScoped<StudentEnrollmentScheduleConflictService>();
        services.AddScoped<PauseEnrollmentEligibleClassResolver>();
        services.AddScoped<RegistrationSessionConsumptionService>();
        services.AddScoped<TicketConsumptionPolicyService>();
        services.AddScoped<TicketGrantService>();
        services.AddScoped<ClassLifecycleService>();
        services.AddScoped<ApprovedLeaveAttendanceService>();
        services.AddScoped<PauseEnrollmentReactivationService>();
        services.AddScoped<ProgramProgressionEvaluationService>();
        services.AddScoped<ProgramProgressionApprovalService>();
        services.AddScoped<ProgramProgressionScheduleNotificationService>();
        services.AddScoped<ILevelCalculationService, LevelCalculationService>();
        services.AddScoped<IGamificationService, GamificationService>();
        return services;
    }
}
