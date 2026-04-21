using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Domain.Common;
using Kidzgo.Domain.Gamification;
using Microsoft.EntityFrameworkCore;

namespace Kidzgo.Application.Gamification.BatchDeliverRewardRedemptions;

/// <summary>
/// Handler để deliver hàng loạt các reward redemptions đã approved
/// Thường được chạy vào cuối tháng để deliver tất cả redemption đã approved trong tháng
/// </summary>
public sealed class BatchDeliverRewardRedemptionsCommandHandler(IDbContext context)
    : ICommandHandler<BatchDeliverRewardRedemptionsCommand, BatchDeliverRewardRedemptionsResponse>
{
    public async Task<Result<BatchDeliverRewardRedemptionsResponse>> Handle(
        BatchDeliverRewardRedemptionsCommand command,
        CancellationToken cancellationToken)
    {
        // Validate month nếu có
        if (command.Month.HasValue && (command.Month.Value < 1 || command.Month.Value > 12))
        {
            return Result.Failure<BatchDeliverRewardRedemptionsResponse>(
                Domain.Common.Error.Validation("Month", "Month must be between 1 and 12"));
        }

        // Validate year nếu có
        if (command.Year.HasValue && (command.Year.Value < 2000 || command.Year.Value > 2100))
        {
            return Result.Failure<BatchDeliverRewardRedemptionsResponse>(
                Domain.Common.Error.Validation("Year", "Year must be between 2000 and 2100"));
        }

        // Nếu có month thì phải có year
        if (command.Month.HasValue && !command.Year.HasValue)
        {
            return Result.Failure<BatchDeliverRewardRedemptionsResponse>(
                Domain.Common.Error.Validation("Year", "Year is required when Month is specified"));
        }

        var query = context.RewardRedemptions
            .Where(r => r.Status == RedemptionStatus.Approved)
            .AsQueryable();

        // Filter theo tháng/năm nếu được chỉ định
        if (command.Year.HasValue && command.Month.HasValue)
        {
            // Tạo DateTime với Kind = UTC để tương thích với PostgreSQL
            var startDate = VietnamTime.TreatAsVietnamLocal(
                new DateTime(command.Year.Value, command.Month.Value, 1, 0, 0, 0));
            var endDate = startDate.AddMonths(1);

            // Lọc các redemption được approve trong tháng đó
            query = query.Where(r =>
                r.HandledAt.HasValue &&
                r.HandledAt.Value >= startDate &&
                r.HandledAt.Value < endDate);
        }
        else if (command.Year.HasValue)
        {
            // Tạo DateTime với Kind = UTC để tương thích với PostgreSQL
            var startDate = VietnamTime.TreatAsVietnamLocal(
                new DateTime(command.Year.Value, 1, 1, 0, 0, 0));
            var endDate = startDate.AddYears(1);

            query = query.Where(r =>
                r.HandledAt.HasValue &&
                r.HandledAt.Value >= startDate &&
                r.HandledAt.Value < endDate);
        }

        var redemptions = await query.ToListAsync(cancellationToken);

        if (!redemptions.Any())
        {
            return Result.Success(new BatchDeliverRewardRedemptionsResponse
            {
                DeliveredCount = 0,
                DeliveredRedemptionIds = new List<Guid>(),
                DeliveredAt = VietnamTime.UtcNow()
            });
        }

        var now = VietnamTime.UtcNow();
        var deliveredIds = new List<Guid>();

        // Update tất cả redemption thành Delivered
        foreach (var redemption in redemptions)
        {
            redemption.Status = RedemptionStatus.Delivered;
            redemption.DeliveredAt = now;
            deliveredIds.Add(redemption.Id);
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success(new BatchDeliverRewardRedemptionsResponse
        {
            DeliveredCount = redemptions.Count,
            DeliveredRedemptionIds = deliveredIds,
            DeliveredAt = now
        });
    }
}

