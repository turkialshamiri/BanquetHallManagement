using System;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Finance.JournalEntries;

[Authorize(BanquetHallManagementPermissions.Finance.ViewJournalEntries)]
public class JournalEntryAppService : BanquetHallManagementAppService, IJournalEntryAppService
{
    private readonly IJournalEntryRepository _journalEntryRepository;
    private readonly IRepository<Account, Guid> _accountRepository;

    public JournalEntryAppService(
        IJournalEntryRepository journalEntryRepository,
        IRepository<Account, Guid> accountRepository)
    {
        _journalEntryRepository = journalEntryRepository;
        _accountRepository = accountRepository;
    }

    public async Task<PagedResultDto<JournalEntryDto>> GetListAsync(
        PagedAndSortedResultRequestDto input)
    {
        var query = await _journalEntryRepository.WithDetailsAsync();

        var totalCount = await AsyncExecuter.CountAsync(query);

        var entries = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(entry => entry.EntryDate)
                .ThenByDescending(entry => entry.EntryNumber)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        var accounts = await _accountRepository.GetListAsync();
        var accountMap = accounts.ToDictionary(account => account.Id);

        var items = entries
            .Select(entry => MapToDto(entry, accountMap))
            .ToList();

        return new PagedResultDto<JournalEntryDto>(totalCount, items);
    }

    public async Task<JournalEntryDto> GetAsync(Guid id)
    {
        var entry = await _journalEntryRepository.GetAsync(id, includeDetails: true);
        var accounts = await _accountRepository.GetListAsync();
        var accountMap = accounts.ToDictionary(account => account.Id);

        return MapToDto(entry, accountMap);
    }

    private static JournalEntryDto MapToDto(
        JournalEntry entry,
        System.Collections.Generic.Dictionary<Guid, Account> accountMap)
    {
        return new JournalEntryDto
        {
            Id = entry.Id,
            EntryNumber = entry.EntryNumber,
            EntryDate = entry.EntryDate,
            SourceType = entry.SourceType.ToString(),
            Description = entry.Description,
            ReservationId = entry.ReservationId,
            PaymentId = entry.PaymentId,
            ReservationNumber = entry.ReservationNumber,
            CustomerName = entry.CustomerName,
            HallName = entry.HallName,
            EmployeeName = entry.EmployeeName,
            IsPosted = entry.IsPosted,
            PostedTime = entry.PostedTime,
            TotalDebit = entry.GetTotalDebit(),
            TotalCredit = entry.GetTotalCredit(),
            Lines = entry.Lines.Select(line =>
            {
                accountMap.TryGetValue(line.AccountId, out var account);

                return new JournalEntryLineDto
                {
                    Id = line.Id,
                    AccountId = line.AccountId,
                    AccountCode = account?.Code ?? string.Empty,
                    AccountName = account?.Name ?? string.Empty,
                    Debit = line.Debit,
                    Credit = line.Credit,
                    Description = line.Description,
                };
            }).ToList(),
        };
    }
}
