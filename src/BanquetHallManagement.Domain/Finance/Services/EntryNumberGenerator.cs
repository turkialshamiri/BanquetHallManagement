using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Finance.Services;

public class EntryNumberGenerator : DomainService, IEntryNumberGenerator
{
    private const string Prefix = "JE";

    private readonly FinanceNumberSequenceManager _sequenceManager;

    public EntryNumberGenerator(FinanceNumberSequenceManager sequenceManager)
    {
        _sequenceManager = sequenceManager;
    }

    public Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        return _sequenceManager.GenerateNextAsync(Prefix, cancellationToken);
    }
}
