using System;
using System.Threading;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Halls;
using BanquetHallManagement.Reservations;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Identity;
using Volo.Abp.Users;

namespace BanquetHallManagement.Finance.JournalEntries;

public class JournalEntryContextProvider : DomainService, IJournalEntryContextProvider
{
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Hall, Guid> _hallRepository;
    private readonly IIdentityUserRepository _identityUserRepository;

    public JournalEntryContextProvider(
        IRepository<Customer, Guid> customerRepository,
        IRepository<Hall, Guid> hallRepository,
        IIdentityUserRepository identityUserRepository)
    {
        _customerRepository = customerRepository;
        _hallRepository = hallRepository;
        _identityUserRepository = identityUserRepository;
    }

    public async Task<JournalEntryBusinessMetadata> ResolveForReservationAsync(
        Reservation reservation,
        CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetAsync(
            reservation.CustomerId,
            cancellationToken: cancellationToken);
        var hall = await _hallRepository.GetAsync(
            reservation.HallId,
            cancellationToken: cancellationToken);

        return new JournalEntryBusinessMetadata
        {
            ReservationNumber = reservation.ReservationNumber,
            CustomerName = customer.Name,
            HallName = hall.Name,
            EmployeeName = await ResolveEmployeeNameAsync(cancellationToken),
        };
    }

    private async Task<string> ResolveEmployeeNameAsync(CancellationToken cancellationToken)
    {
        var currentUser = LazyServiceProvider.LazyGetService<ICurrentUser>();
        if (currentUser?.Id == null)
        {
            return string.Empty;
        }

        var user = await _identityUserRepository.FindAsync(
            currentUser.Id.Value,
            cancellationToken: cancellationToken);

        if (user == null)
        {
            return string.Empty;
        }

        return string.IsNullOrWhiteSpace(user.Name)
            ? user.UserName ?? string.Empty
            : user.Name;
    }
}
