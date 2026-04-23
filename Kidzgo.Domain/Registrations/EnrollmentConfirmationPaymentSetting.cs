using Kidzgo.Domain.Common;
using Kidzgo.Domain.Schools;

namespace Kidzgo.Domain.Registrations;

public class EnrollmentConfirmationPaymentSetting : Entity
{
    public Guid Id { get; set; }
    public Guid? BranchId { get; set; }
    public string ScopeKey { get; set; } = null!;
    public string PaymentMethod { get; set; } = "Tiền mặt / Chuyển khoản";
    public string AccountName { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string BankName { get; set; } = null!;
    public string? BankCode { get; set; }
    public string? BankBin { get; set; }
    public string VietQrTemplate { get; set; } = "compact2";
    public string? LogoUrl { get; set; }
    public string? NewStudentPolicyText { get; set; }
    public string? ReservationPolicyText { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }

    public Branch? Branch { get; set; }

    public static string BuildScopeKey(Guid? branchId)
        => branchId.HasValue ? branchId.Value.ToString("N") : "global";
}
