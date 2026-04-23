using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Kidzgo.Domain.Registrations.Errors;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.UpdateEnrollmentConfirmationPaymentSetting;

public sealed class UpdateEnrollmentConfirmationPaymentSettingCommandHandler(
    IDbContext context,
    IUserContext userContext
) : ICommandHandler<UpdateEnrollmentConfirmationPaymentSettingCommand, UpdateEnrollmentConfirmationPaymentSettingResponse>
{
    public async Task<Result<UpdateEnrollmentConfirmationPaymentSettingResponse>> Handle(
        UpdateEnrollmentConfirmationPaymentSettingCommand command,
        CancellationToken cancellationToken)
    {
        var validation = Validate(command);
        if (validation is not null)
        {
            return Result.Failure<UpdateEnrollmentConfirmationPaymentSettingResponse>(validation);
        }

        if (command.BranchId.HasValue)
        {
            var branchExists = await context.Branches
                .AnyAsync(b => b.Id == command.BranchId.Value, cancellationToken);

            if (!branchExists)
            {
                return Result.Failure<UpdateEnrollmentConfirmationPaymentSettingResponse>(
                    RegistrationErrors.BranchNotFound(command.BranchId.Value));
            }
        }

        var scopeKey = EnrollmentConfirmationPaymentSetting.BuildScopeKey(command.BranchId);
        var now = VietnamTime.UtcNow();

        var setting = await context.EnrollmentConfirmationPaymentSettings
            .FirstOrDefaultAsync(x => x.ScopeKey == scopeKey, cancellationToken);

        if (setting is null)
        {
            setting = new EnrollmentConfirmationPaymentSetting
            {
                Id = Guid.NewGuid(),
                BranchId = command.BranchId,
                ScopeKey = scopeKey,
                CreatedAt = now
            };

            context.EnrollmentConfirmationPaymentSettings.Add(setting);
        }

        setting.PaymentMethod = Normalize(command.PaymentMethod) ?? "Tiền mặt / Chuyển khoản";
        setting.AccountName = Normalize(command.AccountName)!;
        setting.AccountNumber = Normalize(command.AccountNumber)!;
        setting.BankName = Normalize(command.BankName)!;
        setting.BankCode = Normalize(command.BankCode);
        setting.BankBin = Normalize(command.BankBin);
        setting.VietQrTemplate = Normalize(command.VietQrTemplate) ?? "compact2";
        setting.LogoUrl = Normalize(command.LogoUrl);
        setting.NewStudentPolicyText = EnrollmentConfirmationPolicyContent.SerializeLines(command.NewStudentPolicyLines);
        setting.ReservationPolicyText = EnrollmentConfirmationPolicyContent.SerializeLines(command.ReservationPolicyLines);
        setting.IsActive = command.IsActive;
        setting.UpdatedAt = now;
        setting.UpdatedBy = userContext.UserId;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(ToResponse(setting));
    }

    private static Error? Validate(UpdateEnrollmentConfirmationPaymentSettingCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.AccountName))
        {
            return Error.Validation(
                "Registration.PaymentAccountNameRequired",
                "Payment account name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.AccountNumber))
        {
            return Error.Validation(
                "Registration.PaymentAccountNumberRequired",
                "Payment account number is required.");
        }

        if (string.IsNullOrWhiteSpace(command.BankName))
        {
            return Error.Validation(
                "Registration.PaymentBankNameRequired",
                "Payment bank name is required.");
        }

        if (string.IsNullOrWhiteSpace(command.BankCode) && string.IsNullOrWhiteSpace(command.BankBin))
        {
            return Error.Validation(
                "Registration.PaymentBankIdentifierRequired",
                "Bank code or bank BIN is required to generate VietQR.");
        }

        if (!string.IsNullOrWhiteSpace(command.LogoUrl) && !IsSupportedImageUrl(command.LogoUrl))
        {
            return Error.Validation(
                "Registration.PaymentLogoUrlInvalid",
                "Logo URL must be an absolute http(s) URL or a data:image URL.");
        }

        return null;
    }

    private static bool IsSupportedImageUrl(string value)
    {
        var trimmed = value.Trim();
        if (trimmed.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    private static UpdateEnrollmentConfirmationPaymentSettingResponse ToResponse(
        EnrollmentConfirmationPaymentSetting setting)
        => new()
        {
            Id = setting.Id,
            BranchId = setting.BranchId,
            PaymentMethod = setting.PaymentMethod,
            AccountName = setting.AccountName,
            AccountNumber = setting.AccountNumber,
            BankName = setting.BankName,
            BankCode = setting.BankCode,
            BankBin = setting.BankBin,
            VietQrTemplate = setting.VietQrTemplate,
            LogoUrl = setting.LogoUrl,
            NewStudentPolicyLines = EnrollmentConfirmationPolicyContent.GetNewStudentPolicyLines(setting.NewStudentPolicyText),
            ReservationPolicyLines = EnrollmentConfirmationPolicyContent.GetReservationPolicyLines(setting.ReservationPolicyText),
            QrPreviewUrl = EnrollmentConfirmationPaymentQrBuilder.BuildVietQrUrl(
                setting.BankBin,
                setting.BankCode,
                setting.AccountNumber,
                setting.AccountName,
                null,
                null,
                setting.VietQrTemplate),
            IsActive = setting.IsActive,
            CreatedAt = setting.CreatedAt,
            UpdatedAt = setting.UpdatedAt,
            UpdatedBy = setting.UpdatedBy
        };

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
