using System;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Enums;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Finance.JournalEntries;

public interface IJournalEntryRepository : IRepository<JournalEntry, Guid>
{
    Task<JournalEntry?> FindByPaymentIdAsync(
        Guid paymentId,
        CancellationToken cancellationToken = default);

    Task<JournalEntry?> FindByReservationAndSourceTypeAsync(
        Guid reservationId,
        JournalEntrySourceType sourceType,
        CancellationToken cancellationToken = default);
}
