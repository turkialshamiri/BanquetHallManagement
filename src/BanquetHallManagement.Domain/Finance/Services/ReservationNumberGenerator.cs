using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;

namespace BanquetHallManagement.Finance.Services;

public class ReservationNumberGenerator : DomainService, IReservationNumberGenerator
{
    private const string Prefix = "RES";

    private readonly FinanceNumberSequenceManager _sequenceManager;

    public ReservationNumberGenerator(FinanceNumberSequenceManager sequenceManager)
    {
        _sequenceManager = sequenceManager;
    }

    public Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        return _sequenceManager.GenerateNextAsync(Prefix, cancellationToken);
    }
}
