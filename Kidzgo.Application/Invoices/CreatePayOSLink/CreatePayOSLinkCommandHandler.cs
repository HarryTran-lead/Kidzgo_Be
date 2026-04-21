using Kidzgo.Application.Abstraction.Data;
using Kidzgo.Application.Abstraction.Messaging;
using Kidzgo.Application.Abstraction.Payments;
using Kidzgo.Domain.Finance;
using Kidzgo.Domain.Finance.Errors;
using Kidzgo.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Kidzgo.Application.Invoices.CreatePayOSLink;

public sealed class CreatePayOSLinkCommandHandler(
    IDbContext context,
    IPayOSService payOSService,
    IOptions<PayOSOptions> payOSOptions
) : ICommandHandler<CreatePayOSLinkCommand, CreatePayOSLinkResponse>
{
    public async Task<Result<CreatePayOSLinkResponse>> Handle(CreatePayOSLinkCommand command, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices
            .Include(i => i.StudentProfile)
            .Include(i => i.InvoiceLines)
            .FirstOrDefaultAsync(i => i.Id == command.InvoiceId, cancellationToken);

        if (invoice is null)
        {
            return Result.Failure<CreatePayOSLinkResponse>(
                InvoiceErrors.InvoiceNotFound);
        }

        // Cannot create payment link for paid or cancelled invoices
        if (invoice.Status == InvoiceStatus.Paid || invoice.Status == InvoiceStatus.Cancelled)
        {
            return Result.Failure<CreatePayOSLinkResponse>(
                InvoiceErrors.InvoiceAlreadyPaid);
        }

        // Generate order code from invoice ID (use last 8 digits of GUID as long)
        // PayOS requires orderCode to be unique and between 1000 and 999999999999
        var orderCode = GenerateOrderCode(invoice.Id);

        // Convert amount to VND (đồng) - PayOS uses integer amount
        var amountInVnd = (int)(invoice.Amount * 1000); // Assuming Amount is in thousands of VND

        // Build items list from invoice lines
        var items = invoice.InvoiceLines.Select(il => new PayOSItem
        {
            Name = il.Description ?? $"Invoice Line {il.ItemType}",
            Quantity = il.Quantity,
            Price = (int)(il.UnitPrice * 1000) // Convert to VND
        }).ToList();

        // If no invoice lines, create a single item
        if (!items.Any())
        {
            items = new List<PayOSItem>
            {
                new PayOSItem
                {
                    Name = invoice.Description ?? $"Invoice {invoice.Type}",
                    Quantity = 1,
                    Price = amountInVnd
                }
            };
        }

        var options = payOSOptions.Value;
        var request = new PayOSCreateLinkRequest
        {
            OrderCode = orderCode,
            Amount = amountInVnd,
            Description = invoice.Description ?? $"Invoice {invoice.Type} for {invoice.StudentProfile.DisplayName}",
            Items = items,
            ReturnUrl = options.ReturnUrl.Replace("{invoiceId}", invoice.Id.ToString()),
            CancelUrl = options.CancelUrl.Replace("{invoiceId}", invoice.Id.ToString()),
            ExpiredAt = invoice.DueDate.HasValue
                ? (int)(VietnamTime.TreatAsVietnamLocal(invoice.DueDate.Value.ToDateTime(TimeOnly.MinValue))
                    .Subtract(DateTime.UnixEpoch).TotalSeconds)
                : null
        };

        var payOSResponse = await payOSService.CreatePaymentLinkAsync(request, cancellationToken);

        if (payOSResponse.Error != 0 || payOSResponse.Data == null)
        {
            return Result.Failure<CreatePayOSLinkResponse>(
                InvoiceErrors.PayOSCreateLinkFailed(payOSResponse.Message));
        }

        // Save PayOS data to invoice
        invoice.PayosPaymentLink = payOSResponse.Data.CheckoutUrl;
        invoice.PayosQr = payOSResponse.Data.QrCode;
        invoice.PayosOrderCode = orderCode;
        await context.SaveChangesAsync(cancellationToken);

        return new CreatePayOSLinkResponse
        {
            InvoiceId = invoice.Id,
            CheckoutUrl = payOSResponse.Data.CheckoutUrl,
            QrCodeData = payOSResponse.Data.QrCode
        };
    }

    private static long GenerateOrderCode(Guid invoiceId)
    {
        // Use last 8 bytes of GUID to generate a unique order code
        // Convert to positive long (PayOS requires 1000-999999999999)
        var bytes = invoiceId.ToByteArray();
        var value = BitConverter.ToInt64(bytes, 8);
        var orderCode = Math.Abs(value % 999999999000) + 1000; // Ensure between 1000 and 999999999999
        return orderCode;
    }
}
