using Kidzgo.API.Extensions;
using Kidzgo.Application;
using Kidzgo.Infrastructure;

namespace Kidzgo.API;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Configuration
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
            .AddJsonFile(
                $"appsettings.{builder.Environment.EnvironmentName}.local.json",
                optional: true,
                reloadOnChange: true)
            .AddEnvironmentVariables();

        builder.ConfigurePresentationHost();
        builder.Host.UseWindowsService();

        builder.Services
            .AddApplication()
            .AddPresentation(builder.Configuration)
            .AddInfrastructure(builder.Configuration);

        var app = builder.Build();

        app.UsePresentation();

        app.Run();
    }
}
