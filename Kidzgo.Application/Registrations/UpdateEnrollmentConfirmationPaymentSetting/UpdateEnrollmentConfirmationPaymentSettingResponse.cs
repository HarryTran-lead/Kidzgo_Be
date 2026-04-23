namespace Kidzgo.Application.Registrations.UpdateEnrollmentConfirmationPaymentSetting;

public sealed class UpdateEnrollmentConfirmationPaymentSettingResponse
{
    public Guid Id { get; init; }
    public Guid? BranchId { get; init; }
    public string PaymentMethod { get; init; } = "Tiền mặt / Chuyển khoản";
    public string AccountName { get; init; } = null!;
    public string AccountNumber { get; init; } = null!;
    public string BankName { get; init; } = null!;
    public string? BankCode { get; init; }
    public string? BankBin { get; init; }
    public string VietQrTemplate { get; init; } = "compact2";
    public string? LogoUrl { get; init; }
    public List<string> NewStudentPolicyLines { get; init; } = new();
    public List<string> ReservationPolicyLines { get; init; } = new();
    public string? QrPreviewUrl { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public Guid? UpdatedBy { get; init; }
}
