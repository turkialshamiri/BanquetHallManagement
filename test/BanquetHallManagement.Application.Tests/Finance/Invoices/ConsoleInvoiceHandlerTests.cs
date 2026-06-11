using System;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.EventHandlers.Finance;
using BanquetHallManagement.Finance.Invoices.Events;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace BanquetHallManagement.Finance.Invoices;

public class ConsoleInvoiceHandlerTests
{
    [Fact]
    public async Task HandleEventAsync_Should_Log_Invoice_Details()
    {
        var logger = Substitute.For<ILogger<ConsoleInvoiceHandler>>();
        var handler = new ConsoleInvoiceHandler(logger);

        var eventData = new DepositInvoiceCreatedEvent(
            Guid.NewGuid(),
            "INV-2026-00001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            30_000m,
            InvoiceType.Deposit,
            new DateTime(2026, 6, 11, 12, 0, 0));

        await handler.HandleEventAsync(eventData);

        logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(state => state.ToString()!.Contains("INV-2026-00001")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
