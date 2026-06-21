using BanquetHallManagement.Finance.Accounts;
using BanquetHallManagement.Finance.HallAccessCards;
using BanquetHallManagement.Finance.Invoices;
using BanquetHallManagement.Finance.JournalEntries;
using BanquetHallManagement.Finance.Payments;
using BanquetHallManagement.Finance.Sequences;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace BanquetHallManagement.EntityFrameworkCore.Finance;

[ConnectionStringName("Default")]
public interface IFinanceDbContext : IEfCoreDbContext
{
    DbSet<Account> FinanceAccounts { get; }

    DbSet<JournalEntry> JournalEntries { get; }

    DbSet<FinanceNumberSequence> FinanceNumberSequences { get; }

    DbSet<Payment> Payments { get; }

    DbSet<Invoice> Invoices { get; }

    DbSet<HallAccessCard> HallAccessCards { get; }
}
