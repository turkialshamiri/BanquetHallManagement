using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Values;

namespace BanquetHallManagement.Reservations;

public class Money : ValueObject
{
    public decimal Amount { get; private set; }

    public static Money Zero { get; } = new(0m);

    public Money(decimal amount)
    {
        if (amount < 0)
        {
            throw new BusinessException(BanquetHallManagementDomainErrorCodes.PaymentAmountInvalid);
        }

        Amount = amount;
    }

    private Money()
    {
    }

    public static implicit operator decimal(Money money) => money.Amount;

    public static implicit operator Money(decimal amount) => new(amount);

    public static Money operator +(Money left, decimal right) => new(left.Amount + right);

    public static Money operator +(Money left, Money right) => new(left.Amount + right.Amount);

    public static bool operator <(Money left, Money right) => left.Amount < right.Amount;

    public static bool operator >(Money left, Money right) => left.Amount > right.Amount;

    public static bool operator <=(Money left, Money right) => left.Amount <= right.Amount;

    public static bool operator >=(Money left, Money right) => left.Amount >= right.Amount;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
    }
}
