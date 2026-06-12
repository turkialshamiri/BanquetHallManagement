using System;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Payments.Events;
using BanquetHallManagement.Reservations;
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
    private readonly IReservationRepository _reservationRepository;
    private readonly IJournalPostingService _journalPostingService;

    public PaymentJournalEntryHandler(
        IRepository<Payment, Guid> paymentRepository,
        IReservationRepository reservationRepository,
        IJournalPostingService journalPostingService)
    {
        _paymentRepository = paymentRepository;
        _reservationRepository = reservationRepository;
        _journalPostingService = journalPostingService;
    }

    public async Task HandleEventAsync(PaymentReceivedDomainEvent eventData)
    {
        var payment = await _paymentRepository.GetAsync(eventData.PaymentId);
        var reservation = await _reservationRepository.GetAsync(eventData.ReservationId);

        switch (eventData.PaymentType)
        {
            case PaymentType.Deposit:
                if (FinancePaymentRules.IsFullPayment(payment.Amount, reservation.TotalPrice))
                {
                    await _journalPostingService.PostFullDepositPaymentAsync(payment, reservation);
                }
                else
                {
                    await _journalPostingService.PostDepositRevenueAsync(payment, reservation);
                }

                break;

            case PaymentType.Installment:
            case PaymentType.Final:
                await _journalPostingService.PostDeferredRevenueAsync(payment, reservation);
                break;

            default:
                throw new BusinessException(
                    BanquetHallManagementDomainErrorCodes.PaymentJournalPostingNotSupported)
                    .WithData("PaymentType", eventData.PaymentType.ToString());
        }

        await _paymentRepository.UpdateAsync(payment, autoSave: false);
    }
}
