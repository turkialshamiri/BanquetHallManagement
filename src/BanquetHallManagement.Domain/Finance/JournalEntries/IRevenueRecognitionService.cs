using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Reservations;

namespace BanquetHallManagement.Finance.JournalEntries;

public interface IRevenueRecognitionService
{
    Task<JournalEntry?> RecognizeRevenueAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default);
}
