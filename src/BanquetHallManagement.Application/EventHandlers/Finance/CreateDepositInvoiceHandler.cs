using System;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Invoices;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Payments.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;

namespace BanquetHallManagement.EventHandlers.Finance;

public class CreateDepositInvoiceHandler :
    ILocalEventHandler<PaymentReceivedDomainEvent>,
    ITransientDependency
{
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IInvoiceManager _invoiceManager;

    public CreateDepositInvoiceHandler(
        IRepository<Payment, Guid> paymentRepository,
        IInvoiceManager invoiceManager)
    {
        _paymentRepository = paymentRepository;
        _invoiceManager = invoiceManager;
    }

    public async Task HandleEventAsync(PaymentReceivedDomainEvent eventData)
    {
        if (eventData.PaymentType != PaymentType.Deposit)
        {
            return;
        }

        var payment = await _paymentRepository.GetAsync(eventData.PaymentId);

        await _invoiceManager.CreateDepositInvoiceAsync(payment);

        await _paymentRepository.UpdateAsync(payment, autoSave: false);
    }
}
