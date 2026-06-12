using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Reservations;

namespace BanquetHallManagement.Finance.JournalEntries;

public interface IJournalEntryContextProvider
{
    Task<JournalEntryBusinessMetadata> ResolveForReservationAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default);
}
