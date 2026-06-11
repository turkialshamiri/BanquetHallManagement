using System;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Payments.Events;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EventBus;

namespace BanquetHallManagement.EventHandlers.Finance;

public class PaymentJournalEntryHandler :
    ILocalEventHandler<PaymentReceivedDomainEvent>,
    ITransientDependency
{
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IJournalPostingService _journalPostingService;

    public PaymentJournalEntryHandler(
        IRepository<Payment, Guid> paymentRepository,
        IJournalPostingService journalPostingService)
    {
        _paymentRepository = paymentRepository;
        _journalPostingService = journalPostingService;
    }

    public async Task HandleEventAsync(PaymentReceivedDomainEvent eventData)
    {
        var payment = await FindPaymentAsync(eventData.PaymentId);

        switch (eventData.PaymentType)
        {
            case PaymentType.Deposit:
                await _journalPostingService.PostDepositRevenueAsync(payment);
                break;

            case PaymentType.Installment:
                await _journalPostingService.PostDeferredRevenueAsync(payment);
                break;

            default:
                throw new BusinessException(
                    BanquetHallManagementDomainErrorCodes.PaymentJournalPostingNotSupported)
                    .WithData("PaymentType", eventData.PaymentType.ToString());
        }

        await _paymentRepository.UpdateAsync(payment, autoSave: false);
    }

    private Task<Payment> FindPaymentAsync(Guid paymentId)
    {
        return _paymentRepository.GetAsync(paymentId);
    }
}
