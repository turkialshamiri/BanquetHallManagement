using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace BanquetHallManagement.Finance.Sequences;

public class FinanceNumberSequence : Entity<Guid>
{
    public string Prefix { get; private set; } = null!;

    public int Year { get; private set; }

    public int LastNumber { get; private set; }

    protected FinanceNumberSequence()
    {
    }

    public FinanceNumberSequence(Guid id, string prefix, int year, int lastNumber = 0)
    {
        Id = id;
        Prefix = Check.NotNullOrWhiteSpace(prefix, nameof(prefix), maxLength: 10);
        Year = year;
        LastNumber = lastNumber;
    }

    public int GetNextNumber()
    {
        LastNumber++;
        return LastNumber;
    }

    public static string Format(string prefix, int year, int number)
    {
        return $"{prefix}-{year}-{number:D5}";
    }
}
