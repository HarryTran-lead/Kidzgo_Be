using Kidzgo.API.Extensions;
using Kidzgo.API.Requests;
using Kidzgo.Application.Registrations.AssignClass;
using Kidzgo.Application.Registrations.CancelRegistration;
using Kidzgo.Application.Registrations.CreateRegistration;
using Kidzgo.Application.Registrations.GenerateEnrollmentConfirmationPdf;
using Kidzgo.Application.Registrations.GetEnrollmentConfirmationPdfHistory;
using Kidzgo.Application.Registrations.GetEnrollmentConfirmationPdfPreview;
using Kidzgo.Application.Registrations.GetEnrollmentConfirmationPaymentSetting;
using Kidzgo.Application.Registrations.GetRegistrationById;
using Kidzgo.Application.Registrations.GetRegistrations;
using Kidzgo.Application.Registrations.GetWaitingList;
using Kidzgo.Application.Registrations.ImportActiveRegistration;
using Kidzgo.Application.Registrations.SuggestClasses;
using Kidzgo.Application.Registrations.TransferClass;
using Kidzgo.Application.Registrations.UpdateEnrollmentConfirmationPaymentSetting;
using Kidzgo.Application.Registrations.UpdateRegistration;
using Kidzgo.Application.Registrations.UpgradeTuitionPlan;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kidzgo.API.Controllers;

[Route("api/registrations")]
[ApiController]
public class RegistrationController : ControllerBase
{
    private readonly ISender _mediator;

    public RegistrationController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// Tạo mới đăng ký học
    [HttpPost]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CreateRegistration(
        [FromBody] CreateRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateRegistrationCommand
        {
            StudentProfileId = request.StudentProfileId,
            BranchId = request.BranchId,
            ProgramId = request.ProgramId,
            TuitionPlanId = request.TuitionPlanId,
            SecondaryProgramId = request.SecondaryProgramId,
            SecondaryProgramSkillFocus = request.SecondaryProgramSkillFocus,
            ExpectedStartDate = request.ExpectedStartDate,
            PreferredSchedule = request.PreferredSchedule,
            Note = request.Note
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/registrations/{r.Id}");
    }

    /// Import hoc sinh dang hoc giua chung voi so buoi da hoc/con lai tu he thong cu.
    /// Sau khi import, FE tiep tuc goi assign-class de bat dau theo doi tu buoi sap toi.
    [HttpPost("import-active")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> ImportActiveRegistration(
        [FromBody] ImportActiveRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ImportActiveRegistrationCommand
        {
            StudentProfileId = request.StudentProfileId,
            BranchId = request.BranchId,
            ProgramId = request.ProgramId,
            TuitionPlanId = request.TuitionPlanId,
            ExpectedStartDate = request.ExpectedStartDate,
            ActualStartDate = request.ActualStartDate,
            PreferredSchedule = request.PreferredSchedule,
            Note = request.Note,
            UsedSessions = request.UsedSessions,
            RemainingSessions = request.RemainingSessions
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(r => $"/api/registrations/{r.Id}");
    }

    /// Xem danh sách đăng ký
    [HttpGet]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetRegistrations(
        [FromQuery] Guid? studentProfileId,
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? programId,
        [FromQuery] string? status,
        [FromQuery] Guid? classId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetRegistrationsQuery
        {
            StudentProfileId = studentProfileId,
            BranchId = branchId,
            ProgramId = programId,
            Status = status,
            ClassId = classId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Xem chi tiết đăng ký
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetRegistrationById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetRegistrationByIdQuery { Id = id };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Cập nhật đăng ký
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpdateRegistration(
        Guid id,
        [FromBody] UpdateRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateRegistrationCommand
        {
            Id = id,
            ExpectedStartDate = request.ExpectedStartDate,
            PreferredSchedule = request.PreferredSchedule,
            Note = request.Note,
            TuitionPlanId = request.TuitionPlanId,
            SecondaryProgramId = request.SecondaryProgramId,
            SecondaryProgramSkillFocus = request.SecondaryProgramSkillFocus,
            RemoveSecondaryProgram = request.RemoveSecondaryProgram
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// Hủy đăng ký
    [HttpPatch("{id:guid}/cancel")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> CancelRegistration(
        Guid id,
        [FromQuery] string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var command = new CancelRegistrationCommand
        {
            Id = id,
            Reason = reason
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// Gợi ý lớp phù hợp cho đăng ký
    [HttpGet("{id:guid}/suggest-classes")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> SuggestClasses(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new SuggestClassesQuery { RegistrationId = id };
        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Xếp lớp cho học viên
    [HttpPost("{id:guid}/assign-class")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> AssignClass(
        Guid id,
        [FromBody] AssignClassRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignClassCommand
        {
            RegistrationId = id,
            ClassId = request.ClassId,
            EntryType = request.EntryType,
            Track = request.Track,
            FirstStudyDate = request.FirstStudyDate,
            SessionSelectionPattern = request.SessionSelectionPattern
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// Xuat phieu xac nhan nhap hoc PDF sau khi hoc vien duoc xep lop
    [HttpGet("{id:guid}/enrollment-confirmation-pdf")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetEnrollmentConfirmationPdfPreview(
        Guid id,
        [FromQuery] string track = "primary",
        [FromQuery] string formType = "auto",
        CancellationToken cancellationToken = default)
    {
        var query = new GetEnrollmentConfirmationPdfPreviewQuery
        {
            RegistrationId = id,
            Track = track,
            FormType = formType
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Xem lich su cac file PDF xac nhan nhap hoc da generate
    [HttpGet("{id:guid}/enrollment-confirmation-pdf/history")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetEnrollmentConfirmationPdfHistory(
        Guid id,
        [FromQuery] string? track,
        [FromQuery] string? formType,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetEnrollmentConfirmationPdfHistoryQuery
        {
            RegistrationId = id,
            Track = track,
            FormType = formType,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Xuat phieu xac nhan nhap hoc PDF sau khi hoc vien duoc xep lop
    [HttpPost("{id:guid}/enrollment-confirmation-pdf")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GenerateEnrollmentConfirmationPdf(
        Guid id,
        [FromQuery] string track = "primary",
        [FromQuery] bool regenerate = false,
        [FromQuery] string formType = "auto",
        CancellationToken cancellationToken = default)
    {
        var command = new GenerateEnrollmentConfirmationPdfCommand(id, track, regenerate, formType);
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// Xem cau hinh thong tin thanh toan dung tren phieu xac nhan nhap hoc
    [HttpGet("enrollment-confirmation-payment-setting")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetEnrollmentConfirmationPaymentSetting(
        [FromQuery] Guid? branchId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetEnrollmentConfirmationPaymentSettingQuery
        {
            BranchId = branchId
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Admin cap nhat thong tin thanh toan va logo dung tren phieu xac nhan nhap hoc
    [HttpPut("enrollment-confirmation-payment-setting")]
    [Authorize(Roles = "Admin")]
    public async Task<IResult> UpdateEnrollmentConfirmationPaymentSetting(
        [FromBody] EnrollmentConfirmationPaymentSettingRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateEnrollmentConfirmationPaymentSettingCommand
        {
            BranchId = request.BranchId,
            PaymentMethod = request.PaymentMethod,
            AccountName = request.AccountName,
            AccountNumber = request.AccountNumber,
            BankName = request.BankName,
            BankCode = request.BankCode,
            BankBin = request.BankBin,
            VietQrTemplate = request.VietQrTemplate,
            LogoUrl = request.LogoUrl,
            IsActive = request.IsActive
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// Danh sách chờ xếp lớp
    [HttpGet("waiting-list")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> GetWaitingList(
        [FromQuery] Guid? branchId,
        [FromQuery] Guid? programId,
        [FromQuery] string? track,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetWaitingListQuery
        {
            BranchId = branchId,
            ProgramId = programId,
            Track = track,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// Chuyển lớp
    [HttpPost("{id:guid}/transfer-class")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> TransferClass(
        Guid id,
        [FromQuery] Guid newClassId,
        [FromQuery] string track = "primary",
        [FromQuery] string? sessionSelectionPattern = null,
        [FromQuery] DateTime? effectiveDate = null,
        CancellationToken cancellationToken = default)
    {
        var command = new TransferClassCommand
        {
            RegistrationId = id,
            NewClassId = newClassId,
            EffectiveDate = effectiveDate ?? VietnamTime.UtcNow(),
            Track = track,
            SessionSelectionPattern = sessionSelectionPattern
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// Nâng gói học
    [HttpPost("{id:guid}/upgrade")]
    [Authorize(Roles = "Admin,ManagementStaff")]
    public async Task<IResult> UpgradeTuitionPlan(
        Guid id,
        [FromQuery] Guid newTuitionPlanId,
        CancellationToken cancellationToken)
    {
        var command = new UpgradeTuitionPlanCommand
        {
            RegistrationId = id,
            NewTuitionPlanId = newTuitionPlanId
        };

        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
