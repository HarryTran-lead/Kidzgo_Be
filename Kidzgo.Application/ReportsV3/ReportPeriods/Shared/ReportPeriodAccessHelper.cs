using Kidzgo.Domain.Users;

namespace Kidzgo.Application.ReportsV3.ReportPeriods.Shared;

internal static class ReportPeriodAccessHelper
{
    public static bool CanManage(UserRole role)
    {
        return role is UserRole.Admin or UserRole.ManagementStaff or UserRole.Teacher;
    }
}
