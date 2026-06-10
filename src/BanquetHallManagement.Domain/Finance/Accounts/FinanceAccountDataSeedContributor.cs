using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Uow;

namespace BanquetHallManagement.Finance.Accounts;

public class FinanceAccountDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Account, Guid> _accountRepository;
    private readonly IGuidGenerator _guidGenerator;

    public FinanceAccountDataSeedContributor(
        IRepository<Account, Guid> accountRepository,
        IGuidGenerator guidGenerator)
    {
        _accountRepository = accountRepository;
        _guidGenerator = guidGenerator;
    }

    [UnitOfWork]
    public async Task SeedAsync(DataSeedContext context)
    {
        var existingCodes = (await _accountRepository.GetListAsync())
            .Select(account => account.Code)
            .ToHashSet(StringComparer.Ordinal);

        foreach (var definition in GetAccountDefinitions())
        {
            if (existingCodes.Contains(definition.Code))
            {
                continue;
            }

            await _accountRepository.InsertAsync(
                new Account(
                    _guidGenerator.Create(),
                    definition.Code,
                    definition.Name,
                    definition.Type),
                autoSave: true);
        }
    }

    private static IEnumerable<(string Code, string Name, AccountType Type)> GetAccountDefinitions()
    {
        yield return (FinanceAccountCodes.Cash, "Cash", AccountType.Asset);
        yield return (
            FinanceAccountCodes.CustomerRefundLiabilities,
            "Customer Refund Liabilities",
            AccountType.Liability);
        yield return (FinanceAccountCodes.DeferredRevenue, "Deferred Revenue", AccountType.Liability);
        yield return (FinanceAccountCodes.HallRevenue, "Hall Revenue", AccountType.Revenue);
        yield return (FinanceAccountCodes.ServiceRevenue, "Service Revenue", AccountType.Revenue);
        yield return (
            FinanceAccountCodes.NonRefundableDepositRevenue,
            "Non Refundable Deposit Revenue",
            AccountType.Revenue);
    }
}
