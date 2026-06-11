using System.Threading.Tasks;
using BanquetHallManagement.Finance.Invoices.Events;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;

namespace BanquetHallManagement.EventHandlers.Finance;

public class ConsoleInvoiceHandler :
    ILocalEventHandler<DepositInvoiceCreatedEvent>,
    ITransientDependency
{
    private readonly ILogger<ConsoleInvoiceHandler> _logger;

    public ConsoleInvoiceHandler(ILogger<ConsoleInvoiceHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleEventAsync(DepositInvoiceCreatedEvent eventData)
    {
        _logger.LogInformation(
            "Deposit invoice created | Invoice: {InvoiceNumber} ({InvoiceId}) | Reservation: {ReservationId} | Payment: {PaymentId} | Amount: {Amount} | Type: {InvoiceType} | IssuedAt: {IssuedAt}",
            eventData.InvoiceNumber,
            eventData.InvoiceId,
            eventData.ReservationId,
            eventData.PaymentId,
            eventData.Amount,
            eventData.InvoiceType,
            eventData.IssuedAt);

        return Task.CompletedTask;
    }
}
