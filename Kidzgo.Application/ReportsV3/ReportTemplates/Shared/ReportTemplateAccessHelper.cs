using Kidzgo.Domain.Users;

namespace Kidzgo.Application.ReportsV3.ReportTemplates.Shared;

internal static class ReportTemplateAccessHelper
{
    public static bool CanView(UserRole role)
    {
        return role is UserRole.Admin or UserRole.ManagementStaff;
    }

    public static bool CanManage(UserRole role)
    {
        return role == UserRole.Admin;
    }
}
