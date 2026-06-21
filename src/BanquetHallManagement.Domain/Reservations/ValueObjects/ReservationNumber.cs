using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace BanquetHallManagement.Reservations;

public class ReservationCode : ValueObject
{
    public const int MaxLength = 50;

    public string Value { get; private set; } = null!;

    public static ReservationCode Create(string value)
    {
        return new ReservationCode(
            Check.NotNullOrWhiteSpace(value, nameof(value), maxLength: MaxLength));
    }

    private ReservationCode(string value)
    {
        Value = value;
    }

    private ReservationCode()
    {
    }

    public static implicit operator string(ReservationCode number) => number.Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
