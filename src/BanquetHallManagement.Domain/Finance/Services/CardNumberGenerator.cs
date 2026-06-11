using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Finance.Services;

public class CardNumberGenerator : DomainService, ICardNumberGenerator
{
    private const string Prefix = "HAC";

    private readonly FinanceNumberSequenceManager _sequenceManager;

    public CardNumberGenerator(FinanceNumberSequenceManager sequenceManager)
    {
        _sequenceManager = sequenceManager;
    }

    public Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        return _sequenceManager.GenerateNextAsync(Prefix, cancellationToken);
    }
}
