using System;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Halls;
using BanquetHallManagement.Permissions;
using BanquetHallManagement.Reservations;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Domain.Repositories;

namespace BanquetHallManagement.Finance.HallAccessCards;

[Authorize(BanquetHallManagementPermissions.Finance.HallAccessCardsView)]
public class HallAccessCardAppService : BanquetHallManagementAppService, IHallAccessCardAppService
{
    private readonly IHallAccessCardRepository _hallAccessCardRepository;
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Hall, Guid> _hallRepository;

    public HallAccessCardAppService(
        IHallAccessCardRepository hallAccessCardRepository,
        IRepository<Reservation, Guid> reservationRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Hall, Guid> hallRepository)
    {
        _hallAccessCardRepository = hallAccessCardRepository;
        _reservationRepository = reservationRepository;
        _customerRepository = customerRepository;
        _hallRepository = hallRepository;
    }

    public async Task<HallAccessCardDto> GetAsync(Guid id)
    {
        var card = await _hallAccessCardRepository.GetAsync(id);
        return ObjectMapper.Map<HallAccessCard, HallAccessCardDto>(card);
    }

    public async Task<HallAccessCardDto> GetByReservationAsync(Guid reservationId)
    {
        var card = await _hallAccessCardRepository.FindByReservationIdAsync(reservationId);

        if (card == null)
        {
            throw new Volo.Abp.BusinessException(
                BanquetHallManagementDomainErrorCodes.HallAccessCardNotFound)
                .WithData("ReservationId", reservationId);
        }

        return ObjectMapper.Map<HallAccessCard, HallAccessCardDto>(card);
    }

    [Authorize(BanquetHallManagementPermissions.Finance.HallAccessCardsPrint)]
    public async Task<HallAccessCardPrintDataDto> GetPrintDataAsync(Guid id)
    {
        var card = await _hallAccessCardRepository.GetAsync(id);
        var reservation = await _reservationRepository.GetAsync(card.ReservationId);
        var customer = await _customerRepository.GetAsync(reservation.CustomerId);
        var hall = await _hallRepository.GetAsync(reservation.HallId);

        return new HallAccessCardPrintDataDto
        {
            HallAccessCardId = card.Id,
            CardNumber = card.CardNumber,
            IssuedAt = card.IssuedAt,
            ReservationId = reservation.Id,
            EventDate = card.EventDate,
            EntryTime = card.EntryTime,
            ExitTime = card.ExitTime,
            GuestsCount = reservation.GuestsCount,
            CustomerName = customer.Name,
            CustomerPhone = customer.Phone,
            CustomerCompany = customer.Company,
            HallName = hall.Name,
            HallLocation = hall.Location,
        };
    }
}
