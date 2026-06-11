using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Services;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Finance.JournalEntries;

public class JournalPostingService : DomainService, IJournalPostingService
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IRepository<Account, Guid> _accountRepository;
    private readonly IEntryNumberGenerator _entryNumberGenerator;

    public JournalPostingService(
        IJournalEntryRepository journalEntryRepository,
        IRepository<Account, Guid> accountRepository,
        IEntryNumberGenerator entryNumberGenerator)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
        _entryNumberGenerator = entryNumberGenerator;
    }

    public Task<JournalEntry> PostDepositRevenueAsync(
        Payment payment,
        CancellationToken cancellationToken = default)
    {
        return PostPaymentAsync(
            payment,
            JournalEntrySourceType.DepositRevenue,
            FinanceAccountCodes.NonRefundableDepositRevenue,
            cancellationToken);
    }

    public Task<JournalEntry> PostDeferredRevenueAsync(
        Payment payment,
        CancellationToken cancellationToken = default)
    {
        return PostPaymentAsync(
            payment,
            JournalEntrySourceType.DeferredRevenue,
            FinanceAccountCodes.DeferredRevenue,
            cancellationToken);
    }

    private async Task<JournalEntry> PostPaymentAsync(
        Payment payment,
        JournalEntrySourceType sourceType,
        string creditAccountCode,
        CancellationToken cancellationToken)
    {
        var existingEntry = await FindExistingEntryAsync(payment, cancellationToken);
        if (existingEntry != null)
        {
            payment.LinkJournalEntry(existingEntry.Id);
            return existingEntry;
        }

        var cashAccount = await GetRequiredAccountAsync(FinanceAccountCodes.Cash, cancellationToken);
        var creditAccount = await GetRequiredAccountAsync(creditAccountCode, cancellationToken);

        var entryNumber = await _entryNumberGenerator.GenerateAsync(cancellationToken);

        var entry = new JournalEntry(
            GuidGenerator.Create(),
            entryNumber,
            payment.PaymentDate,
            sourceType,
            BuildDescription(payment, sourceType),
            payment.ReservationId,
            payment.Id);

        entry.AddLine(
            GuidGenerator.Create(),
            cashAccount.Id,
            payment.Amount,
            0m,
            "Cash receipt");

        entry.AddLine(
            GuidGenerator.Create(),
            creditAccount.Id,
            0m,
            payment.Amount,
            BuildCreditLineDescription(sourceType));

        entry.Post(Clock.Now);

        await _journalEntryRepository.InsertAsync(entry, autoSave: false, cancellationToken);

        payment.LinkJournalEntry(entry.Id);

        return entry;
    }

    private async Task<JournalEntry?> FindExistingEntryAsync(
        Payment payment,
        CancellationToken cancellationToken)
    {
        if (payment.JournalEntryId.HasValue)
        {
            var linkedEntry = await _journalEntryRepository.FindAsync(
                payment.JournalEntryId.Value,
                cancellationToken: cancellationToken);

            if (linkedEntry != null)
            {
                return linkedEntry;
            }
        }

        return await _journalEntryRepository.FindByPaymentIdAsync(
            payment.Id,
            cancellationToken);
    }

    private async Task<Account> GetRequiredAccountAsync(
        string accountCode,
        CancellationToken cancellationToken)
    {
        var query = await _accountRepository.GetQueryableAsync();

        var account = await AsyncExecuter.FirstOrDefaultAsync(
            query.Where(candidate => candidate.Code == accountCode && candidate.IsActive),
            cancellationToken);

        if (account == null)
        {
            throw new BusinessException(BanquetHallManagementDomainErrorCodes.AccountNotFound)
                .WithData("AccountCode", accountCode);
        }

        return account;
    }

    private static string BuildDescription(Payment payment, JournalEntrySourceType sourceType)
    {
        return sourceType switch
        {
            JournalEntrySourceType.DepositRevenue =>
                $"Deposit payment {payment.ReceiptNumber}",
            JournalEntrySourceType.DeferredRevenue =>
                $"Installment payment {payment.ReceiptNumber}",
            _ => $"Payment {payment.ReceiptNumber}",
        };
    }

    private static string BuildCreditLineDescription(JournalEntrySourceType sourceType)
    {
        return sourceType switch
        {
            JournalEntrySourceType.DepositRevenue => "Non-refundable deposit revenue",
            JournalEntrySourceType.DeferredRevenue => "Deferred revenue",
            _ => "Payment revenue",
        };
    }
}
