using Microsoft.EntityFrameworkCore;

namespace BanquetHallManagement.EntityFrameworkCore.Finance.Configurations;

public static class FinanceDbContextModelCreatingExtensions
{
    public static void ConfigureFinance(this ModelBuilder builder)
    {
        builder.ApplyConfiguration(new AccountConfiguration());
        builder.ApplyConfiguration(new JournalEntryConfiguration());
        builder.ApplyConfiguration(new JournalEntryLineConfiguration());
        builder.ApplyConfiguration(new FinanceNumberSequenceConfiguration());
        builder.ApplyConfiguration(new PaymentConfiguration());
        builder.ApplyConfiguration(new InvoiceConfiguration());
        builder.ApplyConfiguration(new HallAccessCardConfiguration());
    }
}
