using System;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance;
using BanquetHallManagement.Finance.HallAccessCards;
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
    private readonly HallAccessCardManager _hallAccessCardManager;

    public PaymentManager(
        IRepository<Payment, Guid> paymentRepository,
        IReservationRepository reservationRepository,
        IReceiptNumberGenerator receiptNumberGenerator,
        DepositConfirmationService depositConfirmationService,
        HallAccessCardManager hallAccessCardManager)
    {
        _paymentRepository = paymentRepository;
        _reservationRepository = reservationRepository;
        _receiptNumberGenerator = receiptNumberGenerator;
        _depositConfirmationService = depositConfirmationService;
        _hallAccessCardManager = hallAccessCardManager;
    }

    public async Task<DepositPaymentResult> RecordDepositAsync(
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

        HallAccessCard? hallAccessCard = null;
        var isFullyPaid = false;

        if (FinancePaymentRules.IsFullPayment(amount, reservation.TotalPrice))
        {
            isFullyPaid = reservation.TryMarkFullyPaid();
            if (isFullyPaid)
            {
                hallAccessCard = await _hallAccessCardManager.CreateForReservationAsync(
                    reservation,
                    cancellationToken);
            }

            await _reservationRepository.UpdateAsync(reservation, autoSave: false, cancellationToken);
        }

        await _paymentRepository.InsertAsync(payment, autoSave: false, cancellationToken);

        return new DepositPaymentResult(
            payment,
            isFullyPaid,
            hallAccessCard?.Id);
    }

    public async Task<InstallmentPaymentResult> RecordInstallmentAsync(
        Guid reservationId,
        decimal amount,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetAsync(
            reservationId,
            cancellationToken: cancellationToken);

        ValidateInstallmentRequest(reservation, amount);

        var remainingBeforePayment = reservation.GetRemainingAmount();
        var paymentType = amount == remainingBeforePayment && reservation.PaidAmount + amount >= reservation.TotalPrice
            ? PaymentType.Final
            : PaymentType.Installment;

        reservation.ApplyInstallment(amount);

        var receiptNumber = await _receiptNumberGenerator.GenerateAsync(cancellationToken);

        var payment = new Payment(
            GuidGenerator.Create(),
            reservationId,
            amount,
            Clock.Now,
            paymentType,
            receiptNumber);

        var isFullyPaid = reservation.TryMarkFullyPaid();

        HallAccessCard? hallAccessCard = null;
        if (isFullyPaid)
        {
            hallAccessCard = await _hallAccessCardManager.CreateForReservationAsync(
                reservation,
                cancellationToken);
        }

        await _reservationRepository.UpdateAsync(reservation, autoSave: false, cancellationToken);
        await _paymentRepository.InsertAsync(payment, autoSave: false, cancellationToken);

        return new InstallmentPaymentResult(
            payment,
            reservation.GetRemainingAmount(),
            isFullyPaid,
            hallAccessCard?.Id);
    }

    private static void ValidateInstallmentRequest(Reservation reservation, decimal amount)
    {
        if (reservation.Status != ReservationStatus.Confirmed)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.PaymentInvalidReservationStatus);
        }

        if (amount <= 0)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.PaymentAmountInvalid);
        }

        if (FinancePaymentRules.WouldExceedTotalPrice(
                reservation.PaidAmount,
                amount,
                reservation.TotalPrice))
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.PaymentAmountExceedsRemaining)
                .WithData("RemainingAmount", reservation.GetRemainingAmount());
        }

        var remaining = reservation.GetRemainingAmount();
        var minimumInstallment = FinancePaymentRules.CalculateMinimumInstallment(reservation.TotalPrice);
        if (amount < minimumInstallment && amount < remaining)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.PaymentInstallmentBelowMinimum);
        }
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

        if (FinancePaymentRules.WouldExceedTotalPrice(
                reservation.PaidAmount,
                amount,
                reservation.TotalPrice))
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.PaymentAmountExceedsRemaining)
                .WithData("RemainingAmount", reservation.GetRemainingAmount());
        }

        var minimumDeposit = FinancePaymentRules.CalculateMinimumDeposit(reservation.TotalPrice);
        if (amount < minimumDeposit)
        {
            throw new BusinessException(
                BanquetHallManagementDomainErrorCodes.PaymentDepositBelowMinimum);
        }
    }
}
