using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Finance.Services;

public class ReceiptNumberGenerator : DomainService, IReceiptNumberGenerator
{
    private const string Prefix = "RC";

    private readonly FinanceNumberSequenceManager _sequenceManager;

    public ReceiptNumberGenerator(FinanceNumberSequenceManager sequenceManager)
    {
        _sequenceManager = sequenceManager;
    }

    public Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        return _sequenceManager.GenerateNextAsync(Prefix, cancellationToken);
    }
}
