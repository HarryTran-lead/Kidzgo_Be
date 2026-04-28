using Kidzgo.Domain.Common;

namespace Kidzgo.Domain.Registrations.Errors;

public static class RegistrationDiscountCampaignErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "RegistrationDiscountCampaign.NotFound",
        $"Registration discount campaign with Id = '{id}' was not found");

    public static Error BranchNotFound(Guid id) => Error.NotFound(
        "RegistrationDiscountCampaign.BranchNotFound",
        $"Branch with Id = '{id}' was not found");

    public static Error ProgramNotFound(Guid id) => Error.NotFound(
        "RegistrationDiscountCampaign.ProgramNotFound",
        $"Program with Id = '{id}' was not found");

    public static Error TuitionPlanNotFound(Guid id) => Error.NotFound(
        "RegistrationDiscountCampaign.TuitionPlanNotFound",
        $"Tuition plan with Id = '{id}' was not found");

    public static readonly Error InvalidDateRange = Error.Validation(
        "RegistrationDiscountCampaign.InvalidDateRange",
        "EndDate must be greater than or equal to StartDate");

    public static readonly Error InvalidDiscountValue = Error.Validation(
        "RegistrationDiscountCampaign.InvalidDiscountValue",
        "DiscountValue must be greater than 0");

    public static readonly Error InvalidPercentageDiscountValue = Error.Validation(
        "RegistrationDiscountCampaign.InvalidPercentageDiscountValue",
        "Percentage discount value must be between 0 and 100");

    public static readonly Error MissingApplicability = Error.Validation(
        "RegistrationDiscountCampaign.MissingApplicability",
        "Campaign must apply to at least one of initial registration, renewal, or upgrade");

    public static readonly Error TuitionPlanProgramMismatch = Error.Validation(
        "RegistrationDiscountCampaign.TuitionPlanProgramMismatch",
        "Selected tuition plan must belong to the selected program");

    public static readonly Error TuitionPlanBranchMismatch = Error.Validation(
        "RegistrationDiscountCampaign.TuitionPlanBranchMismatch",
        "Selected tuition plan must belong to the selected branch when branch scope is provided");

    public static Error FixedAmountExceedsTuitionPlanAmount(decimal discountValue, decimal tuitionAmount) => Error.Validation(
        "RegistrationDiscountCampaign.FixedAmountExceedsTuitionPlanAmount",
        $"Fixed amount discount '{discountValue}' must not exceed tuition amount '{tuitionAmount}'.");
}
