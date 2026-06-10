using System;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance;
using BanquetHallManagement.Finance.Services;
using BanquetHallManagement.Reservations;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Finance.Payments;

public class PaymentManager : DomainService
{
    private readonly IRepository<Payment, Guid> _paymentRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IReceiptNumberGenerator _receiptNumberGenerator;
    private readonly DepositConfirmationService _depositConfirmationService;

    public PaymentManager(
        IRepository<Payment, Guid> paymentRepository,
        IReservationRepository reservationRepository,
        IReceiptNumberGenerator receiptNumberGenerator,
        DepositConfirmationService depositConfirmationService)
    {
        _paymentRepository = paymentRepository;
        _reservationRepository = reservationRepository;
        _receiptNumberGenerator = receiptNumberGenerator;
        _depositConfirmationService = depositConfirmationService;
    }

    public async Task<Payment> RecordDepositAsync(
        Guid reservationId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetAsync(
            reservationId,
            cancellationToken: cancellationToken);

        ValidateDepositRequest(reservation, amount);

        var receiptNumber = await _receiptNumberGenerator.GenerateAsync(cancellationToken);

        await _depositConfirmationService.ConfirmWithDepositAsync(
            reservation,
            amount,
            cancellationToken);

        var payment = new Payment(
            GuidGenerator.Create(),
            reservationId,
            amount,
            Clock.Now,
            PaymentType.Deposit,
            receiptNumber);

        await _paymentRepository.InsertAsync(payment, autoSave: false, cancellationToken);

        return payment;
    }

    private static void ValidateDepositRequest(Reservation reservation, decimal amount)
    {
        if (reservation.Status != ReservationStatus.Pending)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.PaymentInvalidReservationStatus);
        }

        if (amount <= 0)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.PaymentAmountInvalid);
        }

        var minimumDeposit = FinancePaymentRules.CalculateMinimumDeposit(reservation.TotalPrice);
        if (amount < minimumDeposit)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.PaymentDepositBelowMinimum);
        }
    }
}
