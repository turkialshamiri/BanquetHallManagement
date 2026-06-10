using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Finance.Services;

public class InvoiceNumberGenerator : DomainService, IInvoiceNumberGenerator
{
    private const string Prefix = "INV";

    private readonly FinanceNumberSequenceManager _sequenceManager;

    public InvoiceNumberGenerator(FinanceNumberSequenceManager sequenceManager)
    {
        _sequenceManager = sequenceManager;
    }

    public Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        return _sequenceManager.GenerateNextAsync(Prefix, cancellationToken);
    }
}
