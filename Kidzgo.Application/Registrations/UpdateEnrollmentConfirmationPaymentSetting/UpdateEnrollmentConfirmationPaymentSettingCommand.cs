using Kidzgo.Application.Abstraction.Messaging;

namespace Kidzgo.Application.Registrations.UpdateEnrollmentConfirmationPaymentSetting;

public sealed class UpdateEnrollmentConfirmationPaymentSettingCommand
    : ICommand<UpdateEnrollmentConfirmationPaymentSettingResponse>
{
    public Guid? BranchId { get; init; }
    public string? PaymentMethod { get; init; }
    public string AccountName { get; init; } = null!;
    public string AccountNumber { get; init; } = null!;
    public string BankName { get; init; } = null!;
    public string? BankCode { get; init; }
    public string? BankBin { get; init; }
    public string? VietQrTemplate { get; init; }
    public string? LogoUrl { get; init; }
    public List<string>? NewStudentPolicyLines { get; init; }
    public List<string>? ReservationPolicyLines { get; init; }
    public bool IsActive { get; init; } = true;
}
