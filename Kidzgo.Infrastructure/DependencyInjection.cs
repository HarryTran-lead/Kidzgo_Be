using System.Text;
using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Homework;
using Kidzgo.Application.Abstraction.Payments;
using Kidzgo.Application.Abstraction.Services;
using Kidzgo.Application.Abstraction.Reports;
using Kidzgo.Application.Abstraction.Storage;
using Kidzgo.Infrastructure.AI;
using Kidzgo.Infrastructure.Authentication;
using Kidzgo.Infrastructure.BackgroundJobs;
using Kidzgo.Infrastructure.Database;
using Kidzgo.Infrastructure.Payments;
using Kidzgo.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using What2Gift.Infrastructure.Shared;

namespace Kidzgo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        => services
            .AddDatabase(configuration)
            .AddHealthChecks(configuration)
            .AddClientUrl(configuration)            
            .AddMailService(configuration)
            .AddPushNotificationService(configuration)
            .AddPayOSService(configuration)
            .AddZaloService(configuration)
            .AddAuthenticationInternal(configuration)
            .AddBackgroundJobs(configuration);

    private static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddQuartz(q =>
        {
            // Register SyncPlannedToActualSessionsJob
            var syncJobKey = new JobKey(nameof(SyncPlannedToActualSessionsJob));
            var syncCron = configuration["Quartz:Schedules:SyncPlannedToActualSessionsJob"];

            q.AddJob<SyncPlannedToActualSessionsJob>(opts => opts.WithIdentity(syncJobKey));

            q.AddTrigger(opts =>
            {
                opts.ForJob(syncJobKey)
                    .WithIdentity($"{nameof(SyncPlannedToActualSessionsJob)}.trigger");

                if (!string.IsNullOrWhiteSpace(syncCron))
                {
                    opts.WithCronSchedule(syncCron, x => x.WithMisfireHandlingInstructionDoNothing());
                }
                else
                {
                    // Fallback: mỗi 1 phút
                    opts.WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(1)
                        .RepeatForever());
                }
            });

            var maintainRollingSessionWindowJobKey = new JobKey(nameof(MaintainRollingSessionWindowJob));
            var maintainRollingSessionWindowCron = configuration["Quartz:Schedules:MaintainRollingSessionWindowJob"];

            q.AddJob<MaintainRollingSessionWindowJob>(opts => opts.WithIdentity(maintainRollingSessionWindowJobKey));

            q.AddTrigger(opts =>
            {
                opts.ForJob(maintainRollingSessionWindowJobKey)
                    .WithIdentity($"{nameof(MaintainRollingSessionWindowJob)}.trigger");

                if (!string.IsNullOrWhiteSpace(maintainRollingSessionWindowCron))
                {
                    opts.WithCronSchedule(
                        maintainRollingSessionWindowCron,
                        x => x.WithMisfireHandlingInstructionDoNothing());
                }
                else
                {
                    opts.WithCronSchedule("0 0 18 * * ?", x => x.WithMisfireHandlingInstructionDoNothing());
                }
            });

            // Register AutoConfirmRewardRedemptionJob
            var autoConfirmJobKey = new JobKey(nameof(AutoConfirmRewardRedemptionJob));
            var autoConfirmCron = configuration["Quartz:Schedules:AutoConfirmRewardRedemptionJob"];

            q.AddJob<AutoConfirmRewardRedemptionJob>(opts => opts.WithIdentity(autoConfirmJobKey));

            q.AddTrigger(opts =>
            {
                opts.ForJob(autoConfirmJobKey)
                    .WithIdentity($"{nameof(AutoConfirmRewardRedemptionJob)}.trigger");

                if (!string.IsNullOrWhiteSpace(autoConfirmCron))
                {
                    opts.WithCronSchedule(autoConfirmCron, x => x.WithMisfireHandlingInstructionDoNothing());
                }
                else
                {
                    // Fallback: mỗi ngày lúc 2:00 AM UTC
                    opts.WithCronSchedule("0 0 2 * * ?", x => x.WithMisfireHandlingInstructionDoNothing());
                }
            });

            // Register MarkOverdueHomeworkSubmissionsJob (UC-132)
            var markOverdueHomeworkJobKey = new JobKey(nameof(MarkOverdueHomeworkSubmissionsJob));
            var markOverdueHomeworkCron = configuration["Quartz:Schedules:MarkOverdueHomeworkSubmissionsJob"];

            q.AddJob<MarkOverdueHomeworkSubmissionsJob>(opts => opts.WithIdentity(markOverdueHomeworkJobKey));

            q.AddTrigger(opts =>
            {
                opts.ForJob(markOverdueHomeworkJobKey)
                    .WithIdentity($"{nameof(MarkOverdueHomeworkSubmissionsJob)}.trigger");

                if (!string.IsNullOrWhiteSpace(markOverdueHomeworkCron))
                {
                    opts.WithCronSchedule(markOverdueHomeworkCron, x => x.WithMisfireHandlingInstructionDoNothing());
                }
                else
                {
                    // Fallback: mỗi 5 phút
                    opts.WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(5)
                        .RepeatForever());
                }
            });

            // Register ResetMissedDailyCheckInMissionProgressJob
            var resetDailyCheckInMissionJobKey = new JobKey(nameof(ResetMissedDailyCheckInMissionProgressJob));
            var resetDailyCheckInMissionCron = configuration["Quartz:Schedules:ResetMissedDailyCheckInMissionProgressJob"];

            q.AddJob<ResetMissedDailyCheckInMissionProgressJob>(opts => opts.WithIdentity(resetDailyCheckInMissionJobKey));

            q.AddTrigger(opts =>
            {
                opts.ForJob(resetDailyCheckInMissionJobKey)
                    .WithIdentity($"{nameof(ResetMissedDailyCheckInMissionProgressJob)}.trigger");

                if (!string.IsNullOrWhiteSpace(resetDailyCheckInMissionCron))
                {
                    opts.WithCronSchedule(resetDailyCheckInMissionCron, x => x.WithMisfireHandlingInstructionDoNothing());
                }
                else
                {
                    opts.WithSimpleSchedule(x => x
                        .WithIntervalInHours(1)
                        .RepeatForever());
                }
            });

            // Register SendNotificationRemindersJob (UC-331-336)
            var notificationRemindersJobKey = new JobKey(nameof(SendNotificationRemindersJob));
            var notificationRemindersCron = configuration["Quartz:Schedules:SendNotificationRemindersJob"];

            q.AddJob<SendNotificationRemindersJob>(opts => opts.WithIdentity(notificationRemindersJobKey));

            q.AddTrigger(opts =>
            {
                opts.ForJob(notificationRemindersJobKey)
                    .WithIdentity($"{nameof(SendNotificationRemindersJob)}.trigger");

                if (!string.IsNullOrWhiteSpace(notificationRemindersCron))
                {
                    opts.WithCronSchedule(notificationRemindersCron, x => x.WithMisfireHandlingInstructionDoNothing());
                }
                else
                {
                    // Fallback: mỗi giờ
                    opts.WithSimpleSchedule(x => x
                        .WithIntervalInHours(1)
                        .RepeatForever());
                }
            });

            // Register ResumePausedEnrollmentsJob
            var resumePausedEnrollmentsJobKey = new JobKey(nameof(ResumePausedEnrollmentsJob));
            var resumePausedEnrollmentsCron = configuration["Quartz:Schedules:ResumePausedEnrollmentsJob"];

            q.AddJob<ResumePausedEnrollmentsJob>(opts => opts.WithIdentity(resumePausedEnrollmentsJobKey));

            q.AddTrigger(opts =>
            {
                opts.ForJob(resumePausedEnrollmentsJobKey)
                    .WithIdentity($"{nameof(ResumePausedEnrollmentsJob)}.trigger");

                if (!string.IsNullOrWhiteSpace(resumePausedEnrollmentsCron))
                {
                    opts.WithCronSchedule(resumePausedEnrollmentsCron, x => x.WithMisfireHandlingInstructionDoNothing());
                }
                else
                {
                    opts.WithSimpleSchedule(x => x
                        .WithIntervalInMinutes(5)
                        .RepeatForever());
                }
            });

            var backfillStudentSessionAssignmentsCron =
                configuration["Quartz:Schedules:BackfillStudentSessionAssignmentsJob"];

            if (!string.IsNullOrWhiteSpace(backfillStudentSessionAssignmentsCron))
            {
                var backfillJobKey = new JobKey(nameof(BackfillStudentSessionAssignmentsJob));

                q.AddJob<BackfillStudentSessionAssignmentsJob>(opts => opts.WithIdentity(backfillJobKey));

                q.AddTrigger(opts =>
                {
                    opts.ForJob(backfillJobKey)
                        .WithIdentity($"{nameof(BackfillStudentSessionAssignmentsJob)}.trigger")
                        .WithCronSchedule(
                            backfillStudentSessionAssignmentsCron,
                            x => x.WithMisfireHandlingInstructionDoNothing());
                });
            }
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });
        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Default));
            
            // Suppress PendingModelChangesWarning để không bị throw exception khi Migrate()
            // Warning này có thể xuất hiện khi model và snapshot không khớp hoàn toàn
            options.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<TemplateRuntimeSeeder>();

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks().AddNpgSql(configuration.GetConnectionString("Database")!);
        return services;
    }
    
    private static IServiceCollection AddMailService(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));
        services.AddTransient<IMailService, MailService>();
        return services;
    }

    private static IServiceCollection AddClientUrl(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ClientSettings>(configuration.GetSection(nameof(ClientSettings)));
        services.AddSingleton<Application.Abstraction.Services.IClientUrlProvider, ClientUrlProvider>();
        return services;
    }

    private static IServiceCollection AddPayOSService(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PayOSOptions>(configuration.GetSection(PayOSOptions.SectionName));
        
        services.AddHttpClient<IPayOSService, PayOSService>(client =>
        {
            var options = configuration.GetSection(PayOSOptions.SectionName).Get<PayOSOptions>();
            if (options != null)
            {
                client.BaseAddress = new Uri(options.BaseUrl);
            }
        });

        return services;
    }
    
    private static IServiceCollection AddPushNotificationService(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IPushNotificationService, PushNotificationService>();
        return services;
    }

    private static IServiceCollection AddZaloService(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<Shared.ZaloSettings>(configuration.GetSection("Zalo"));
        services.AddHttpClient<Application.Abstraction.Services.IZaloService, Services.ZaloService>();
        return services;
    }

    private static IServiceCollection AddAuthenticationInternal(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true, 
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Secret"]!)
                    ),
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.Zero
                };
            });
    
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();
        services.AddSingleton<ITokenProvider, TokenProvider>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IMailService, MailService>();
        services.AddSingleton<IPayload, Payload>();
        services.AddScoped<ITemplateRenderer, TemplateRenderer>();
        services.AddScoped<IImageUploader, ImageUploader>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<ITeachingMaterialStorageService, TeachingMaterialStorageService>();
        services.AddScoped<ITeachingMaterialPreviewService, TeachingMaterialPreviewService>();
        
        // Register FileStorageService - Local hoặc Cloudinary dựa trên config
        var fileStorageProvider = configuration["FileStorage:Provider"] ?? "Cloudinary";
        if (fileStorageProvider.Equals("Local", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<Application.Abstraction.Storage.IFileStorageService, LocalFileStorageService>();
        }
        // else
        // {
        //     services.AddScoped<Application.Abstraction.Storage.IFileStorageService, CloudinaryFileStorageService>();
        // }
        
        services.AddScoped<Kidzgo.Application.Services.SessionGenerationService>();
        services.AddScoped<Application.Abstraction.Reports.IMonthlyReportDataAggregator, MonthlyReportDataAggregator>();
        
        // Register AI Report Generator
        services.AddHttpClient<IAiReportGenerator, HttpAiReportGenerator>(client =>
        {
            var baseUrl = configuration["AiService:BaseUrl"] ?? "http://localhost:8000";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromMinutes(5);
        });

        // Register AI Feedback Enhancer (UC-174)
        services.AddHttpClient<IAiFeedbackEnhancer, HttpAiFeedbackEnhancer>(client =>
        {
            var baseUrl = configuration["AiService:BaseUrl"] ?? "http://localhost:8000";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        services.AddHttpClient<IAiHomeworkAssistant, HttpAiHomeworkAssistant>(client =>
        {
            var baseUrl = configuration["AiService:BaseUrl"] ?? "http://localhost:8000";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        
        // Register PDF Report Generator
        services.AddScoped<Application.Abstraction.Reports.IPdfReportGenerator, Reports.QuestPdfReportGenerator>();
        services.AddScoped<IEnrollmentConfirmationPdfGenerator, Reports.EnrollmentConfirmationPdfGenerator>();
        
        return services;
    }

    
}
