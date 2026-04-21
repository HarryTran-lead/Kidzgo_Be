using Kidzgo.Application.Abstraction.Authentication;
using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Payroll;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Users.GetAccountantStaffOverview;

public sealed class GetAccountantStaffOverviewQueryHandler(
    IDbContext context,
    IUserContext userContext)
    : IQueryHandler<GetAccountantStaffOverviewQuery, AccountantStaffOverviewResponse>
{
    public async Task<Result<AccountantStaffOverviewResponse>> Handle(
        GetAccountantStaffOverviewQuery query,
        CancellationToken cancellationToken)
    {
        var userId = userContext.UserId;
        var now = VietnamTime.UtcNow();
        var todayInVietnam = VietnamTime.ToVietnamDateOnly(now);
        var fromDate = query.FromDate ?? now.AddMonths(-1);
        var toDate = query.ToDate ?? now.AddMonths(1);

        // Get staff's branch
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null || !user.BranchId.HasValue)
        {
            return Result.Failure<AccountantStaffOverviewResponse>(
                Domain.Common.Error.NotFound("User", "not found"));
        }

        var branchId = user.BranchId.Value;

        // Base queries filtered by branch (Accountant Staff - Finance)
        var invoicesQuery = context.Invoices
            .AsNoTracking()
            .Where(i => i.BranchId == branchId);
        var paymentsQuery = context.Payments
            .AsNoTracking()
            .Where(p => p.Invoice.BranchId == branchId);
        var payrollRunsQuery = context.PayrollRuns
            .AsNoTracking()
            .Where(pr => pr.BranchId == branchId);
        var cashbookQuery = context.CashbookEntries
            .AsNoTracking()
            .Where(cb => cb.BranchId == branchId);

        // Apply entity filters
        if (query.StudentProfileId.HasValue)
        {
            invoicesQuery = invoicesQuery.Where(i => i.StudentProfileId == query.StudentProfileId.Value);
            paymentsQuery = paymentsQuery.Where(p => p.Invoice.StudentProfileId == query.StudentProfileId.Value);
        }

        if (query.InvoiceId.HasValue)
        {
            invoicesQuery = invoicesQuery.Where(i => i.Id == query.InvoiceId.Value);
            paymentsQuery = paymentsQuery.Where(p => p.InvoiceId == query.InvoiceId.Value);
        }

        if (query.PaymentId.HasValue)
        {
            paymentsQuery = paymentsQuery.Where(p => p.Id == query.PaymentId.Value);
        }

        // Statistics
        var statistics = new DashboardStatistics
        {
            TotalRevenue = await paymentsQuery
                .Where(p => p.PaidAt >= fromDate && p.PaidAt <= toDate)
                .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0,
            PendingPayments = await invoicesQuery
                .Where(i => i.Status == InvoiceStatus.Pending)
                .SumAsync(i => (decimal?)i.Amount, cancellationToken) ?? 0,
            OverdueAmount = await invoicesQuery
                .Where(i => i.Status == InvoiceStatus.Overdue)
                .SumAsync(i => (decimal?)i.Amount, cancellationToken) ?? 0,
            PendingInvoices = await invoicesQuery
                .CountAsync(i => i.Status == InvoiceStatus.Pending, cancellationToken),
            OverdueInvoices = await invoicesQuery
                .CountAsync(i => i.Status == InvoiceStatus.Overdue, cancellationToken),
            PendingPayrolls = await payrollRunsQuery
                .CountAsync(pr => pr.Status == PayrollRunStatus.Draft || pr.Status == PayrollRunStatus.Approved, cancellationToken),
            CashBalance = await cashbookQuery
                .Where(cb => cb.EntryDate <= VietnamTime.ToVietnamDateOnly(now))
                .SumAsync(cb => cb.Type == CashbookEntryType.CashIn ? (decimal?)cb.Amount : -(decimal?)cb.Amount, cancellationToken) ?? 0
        };

        // Pending Invoices
        var pendingInvoices = await invoicesQuery
            .Where(i => i.Status == InvoiceStatus.Pending)
            .OrderBy(i => i.DueDate)
            .Take(20)
            .Select(i => new InvoiceSummaryDto
            {
                Id = i.Id,
                InvoiceNumber = i.Id.ToString(),
                StudentName = i.StudentProfile.DisplayName,
                Amount = i.Amount,
                PaymentStatus = i.Status.ToString(),
                DueDate = i.DueDate.HasValue
                    ? VietnamTime.TreatAsVietnamLocal(i.DueDate.Value.ToDateTime(TimeOnly.MinValue))
                    : null,
                IssuedAt = i.IssuedAt
            })
            .ToListAsync(cancellationToken);

        // Overdue Invoices
        var overdueInvoices = await invoicesQuery
            .Where(i => i.Status == InvoiceStatus.Overdue)
            .OrderBy(i => i.DueDate)
            .Take(20)
            .Select(i => new InvoiceSummaryDto
            {
                Id = i.Id,
                InvoiceNumber = i.Id.ToString(),
                StudentName = i.StudentProfile.DisplayName,
                Amount = i.Amount,
                PaymentStatus = i.Status.ToString(),
                DueDate = i.DueDate.HasValue
                    ? VietnamTime.TreatAsVietnamLocal(i.DueDate.Value.ToDateTime(TimeOnly.MinValue))
                    : null,
                IssuedAt = i.IssuedAt
            })
            .ToListAsync(cancellationToken);

        // Recent Payments
        var recentPayments = await paymentsQuery
            .Where(p => p.PaidAt >= fromDate && p.PaidAt <= toDate)
            .OrderByDescending(p => p.PaidAt)
            .Take(20)
            .Select(p => new PaymentSummaryDto
            {
                Id = p.Id,
                InvoiceNumber = p.Invoice.Id.ToString(),
                StudentName = p.Invoice.StudentProfile.DisplayName,
                Amount = p.Amount,
                PaymentMethod = p.Method.ToString(),
                PaidAt = p.PaidAt ?? VietnamTime.UtcNow()
            })
            .ToListAsync(cancellationToken);

        // Debt Summary
        var debtSummary = await invoicesQuery
            .Where(i => i.Status == InvoiceStatus.Pending || i.Status == InvoiceStatus.Overdue)
            .GroupBy(i => new { i.StudentProfileId, i.StudentProfile.DisplayName })
            .Select(g => new DebtSummaryDto
            {
                StudentProfileId = g.Key.StudentProfileId,
                StudentName = g.Key.DisplayName,
                TotalDebt = g.Sum(i => i.Amount),
                OverdueDays = g.Where(i => i.Status == InvoiceStatus.Overdue && i.DueDate.HasValue)
                    .Select(i => (int?)(todayInVietnam.DayNumber - i.DueDate!.Value.DayNumber))
                    .Max() ?? 0,
                InvoiceCount = g.Count()
            })
            .OrderByDescending(d => d.TotalDebt)
            .Take(20)
            .ToListAsync(cancellationToken);

        // Pending Payrolls
        var pendingPayrolls = await payrollRunsQuery
            .Where(pr => pr.Status == PayrollRunStatus.Draft || pr.Status == PayrollRunStatus.Approved)
            .OrderByDescending(pr => pr.CreatedAt)
            .Take(10)
            .Select(pr => new PayrollSummaryDto
            {
                Id = pr.Id,
                PayrollPeriod = $"{pr.PeriodStart:yyyy-MM-dd} to {pr.PeriodEnd:yyyy-MM-dd}",
                TotalAmount = pr.PayrollLines.Sum(pl => pl.Amount),
                Status = pr.Status.ToString(),
                CreatedAt = pr.CreatedAt
            })
            .ToListAsync(cancellationToken);

        // Recent Cashbook Entries
        var recentCashbookEntries = await cashbookQuery
            .Where(cb => cb.EntryDate >= VietnamTime.ToVietnamDateOnly(fromDate) && 
                        cb.EntryDate <= VietnamTime.ToVietnamDateOnly(toDate))
            .OrderByDescending(cb => cb.EntryDate)
            .ThenByDescending(cb => cb.CreatedAt)
            .Take(20)
            .Select(cb => new CashbookSummaryDto
            {
                Id = cb.Id,
                Type = cb.Type.ToString(),
                Amount = cb.Amount,
                Description = cb.Description ?? "",
                TransactionDate = VietnamTime.TreatAsVietnamLocal(cb.EntryDate.ToDateTime(TimeOnly.MinValue))
            })
            .ToListAsync(cancellationToken);

        return new AccountantStaffOverviewResponse
        {
            Statistics = statistics,
            PendingInvoices = pendingInvoices,
            OverdueInvoices = overdueInvoices,
            RecentPayments = recentPayments,
            DebtSummary = debtSummary,
            PendingPayrolls = pendingPayrolls,
            RecentCashbookEntries = recentCashbookEntries
        };
    }
}

