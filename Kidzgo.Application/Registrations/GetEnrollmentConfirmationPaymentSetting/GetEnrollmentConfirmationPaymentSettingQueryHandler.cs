using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Registrations.Shared;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Registrations;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Registrations.GetEnrollmentConfirmationPaymentSetting;

public sealed class GetEnrollmentConfirmationPaymentSettingQueryHandler(
    IDbContext context
) : IQueryHandler<GetEnrollmentConfirmationPaymentSettingQuery, GetEnrollmentConfirmationPaymentSettingResponse>
{
    public async Task<Result<GetEnrollmentConfirmationPaymentSettingResponse>> Handle(
        GetEnrollmentConfirmationPaymentSettingQuery query,
        CancellationToken cancellationToken)
    {
        EnrollmentConfirmationPaymentSetting? setting = null;
        var isFallback = false;

        if (query.BranchId.HasValue)
        {
            var branchScopeKey = EnrollmentConfirmationPaymentSetting.BuildScopeKey(query.BranchId);
            setting = await context.EnrollmentConfirmationPaymentSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ScopeKey == branchScopeKey, cancellationToken);
        }

        if (setting is null)
        {
            setting = await context.EnrollmentConfirmationPaymentSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.ScopeKey == EnrollmentConfirmationPaymentSetting.BuildScopeKey(null),
                    cancellationToken);
            isFallback = query.BranchId.HasValue && setting is not null;
        }

        return Result.Success(ToResponse(setting, isFallback));
    }

    private static GetEnrollmentConfirmationPaymentSettingResponse ToResponse(
        EnrollmentConfirmationPaymentSetting? setting,
        bool isFallback)
    {
        if (setting is null)
        {
            return new GetEnrollmentConfirmationPaymentSettingResponse
            {
                NewStudentPolicyLines = EnrollmentConfirmationPolicyContent.GetNewStudentPolicyLines(null),
                ReservationPolicyLines = EnrollmentConfirmationPolicyContent.GetReservationPolicyLines(null),
                IsActive = false
            };
        }

        return new GetEnrollmentConfirmationPaymentSettingResponse
        {
            Id = setting.Id,
            BranchId = setting.BranchId,
            IsFallbackToGlobal = isFallback,
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
    }
}
