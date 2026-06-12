using System;
using System.Linq;
using System.Threading.Tasks;
using BanquetHallManagement.Customers;
using BanquetHallManagement.Entities.BanquetHallManagement.Entities;
using BanquetHallManagement.Enums;
using BanquetHallManagement.Halls;
using BanquetHallManagement.Permissions;
using BanquetHallManagement.Reservations;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace BanquetHallManagement.Finance.HallAccessCards;

[Authorize(BanquetHallManagementPermissions.Finance.HallAccessCardsView)]
public class HallAccessCardAppService : BanquetHallManagementAppService, IHallAccessCardAppService
{
    private readonly IHallAccessCardRepository _hallAccessCardRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Hall, Guid> _hallRepository;
    private readonly IIdentityUserRepository _identityUserRepository;

    public HallAccessCardAppService(
        IHallAccessCardRepository hallAccessCardRepository,
        IReservationRepository reservationRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Hall, Guid> hallRepository,
        IIdentityUserRepository identityUserRepository)
    {
        _hallAccessCardRepository = hallAccessCardRepository;
        _reservationRepository = reservationRepository;
        _customerRepository = customerRepository;
        _hallRepository = hallRepository;
        _identityUserRepository = identityUserRepository;
    }

    public async Task<PagedResultDto<HallAccessCardListItemDto>> GetListAsync(
        HallAccessCardGetListInput input)
    {
        var cardQuery = await _hallAccessCardRepository.GetQueryableAsync();
        var reservationQuery = await _reservationRepository.GetQueryableAsync();
        var customerQuery = await _customerRepository.GetQueryableAsync();
        var hallQuery = await _hallRepository.GetQueryableAsync();

        var query =
            from card in cardQuery
            join reservation in reservationQuery on card.ReservationId equals reservation.Id
            join customer in customerQuery on reservation.CustomerId equals customer.Id
            join hall in hallQuery on reservation.HallId equals hall.Id
            select new { card, reservation, customer, hall };

        if (!string.IsNullOrWhiteSpace(input.ReservationNumber))
        {
            query = query.Where(item =>
                item.reservation.ReservationNumber == input.ReservationNumber);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var items = await AsyncExecuter.ToListAsync(
            query
                .OrderByDescending(item => item.card.IssuedAt)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        var dtos = new System.Collections.Generic.List<HallAccessCardListItemDto>(items.Count);

        foreach (var item in items)
        {
            dtos.Add(new HallAccessCardListItemDto
            {
                Id = item.card.Id,
                ReservationId = item.reservation.Id,
                ReservationNumber = item.reservation.ReservationNumber,
                CardNumber = item.card.CardNumber,
                IssuedAt = item.card.IssuedAt,
                EventDate = item.card.EventDate,
                EntryTime = item.card.EntryTime,
                ExitTime = item.card.ExitTime,
                CustomerName = item.customer.Name,
                HallName = item.hall.Name,
                EmployeeName = await ResolveEmployeeNameAsync(item.card.CreatorId),
                ReservationStatus = item.reservation.Status.ToString(),
                PaymentStatus = ResolvePaymentStatus(item.reservation),
                IsUsed = item.card.IsUsed,
            });
        }

        return new PagedResultDto<HallAccessCardListItemDto>(totalCount, dtos);
    }

    public async Task<HallAccessCardDto> GetAsync(Guid id)
    {
        var card = await _hallAccessCardRepository.GetAsync(id);
        var reservation = await _reservationRepository.GetAsync(card.ReservationId);

        var dto = ObjectMapper.Map<HallAccessCard, HallAccessCardDto>(card);
        dto.ReservationNumber = reservation.ReservationNumber;

        return dto;
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

        var reservation = await _reservationRepository.GetAsync(reservationId);
        var dto = ObjectMapper.Map<HallAccessCard, HallAccessCardDto>(card);
        dto.ReservationNumber = reservation.ReservationNumber;

        return dto;
    }

    public async Task<HallAccessCardDto> GetByReservationNumberAsync(string reservationNumber)
    {
        var reservation = await _reservationRepository.FindByReservationNumberAsync(reservationNumber);
        if (reservation == null)
        {
            throw new Volo.Abp.BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationNotFound)
                .WithData("ReservationNumber", reservationNumber);
        }

        return await GetByReservationAsync(reservation.Id);
    }

    [Authorize(BanquetHallManagementPermissions.Reservations.ConfirmHallEntry)]
    public async Task<HallAccessCardEntryPreviewDto> GetEntryPreviewByReservationNumberAsync(
        string reservationNumber)
    {
        var reservation = await _reservationRepository.FindByReservationNumberAsync(reservationNumber);
        if (reservation == null)
        {
            throw new Volo.Abp.BusinessException(
                BanquetHallManagementDomainErrorCodes.ReservationNotFound)
                .WithData("ReservationNumber", reservationNumber);
        }

        var card = await _hallAccessCardRepository.FindByReservationIdAsync(reservation.Id);
        if (card == null)
        {
            throw new Volo.Abp.BusinessException(
                BanquetHallManagementDomainErrorCodes.HallAccessCardNotFound)
                .WithData("ReservationNumber", reservationNumber);
        }

        var customer = await _customerRepository.GetAsync(reservation.CustomerId);
        var hall = await _hallRepository.GetAsync(reservation.HallId);

        return new HallAccessCardEntryPreviewDto
        {
            ReservationId = reservation.Id,
            ReservationNumber = reservation.ReservationNumber,
            ReservationStatus = reservation.Status.ToString(),
            PaymentStatus = ResolvePaymentStatus(reservation),
            TotalPrice = reservation.TotalPrice,
            PaidAmount = reservation.PaidAmount,
            CustomerName = customer.Name,
            HallName = hall.Name,
            HallAccessCardId = card.Id,
            CardNumber = card.CardNumber,
            EventDate = card.EventDate,
            EntryTime = card.EntryTime,
            ExitTime = card.ExitTime,
            EmployeeName = await ResolveEmployeeNameAsync(card.CreatorId),
            CanConfirmEntry = reservation.Status == ReservationStatus.FullyPaid,
        };
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
            ReservationNumber = reservation.ReservationNumber,
            EventDate = card.EventDate,
            EntryTime = card.EntryTime,
            ExitTime = card.ExitTime,
            GuestsCount = reservation.GuestsCount,
            CustomerName = customer.Name,
            CustomerPhone = customer.Phone,
            CustomerCompany = customer.Company,
            HallName = hall.Name,
            HallLocation = hall.Location,
            EmployeeName = await ResolveEmployeeNameAsync(card.CreatorId),
        };
    }

    private static string ResolvePaymentStatus(Reservation reservation)
    {
        if (reservation.PaidAmount >= reservation.TotalPrice)
        {
            return nameof(ReservationStatus.FullyPaid);
        }

        if (reservation.PaidAmount > 0)
        {
            return "PartiallyPaid";
        }

        return "Unpaid";
    }

    private async Task<string> ResolveEmployeeNameAsync(Guid? userId)
    {
        if (!userId.HasValue)
        {
            return string.Empty;
        }

        var user = await _identityUserRepository.FindAsync(userId.Value);
        if (user == null)
        {
            return string.Empty;
        }

        return string.IsNullOrWhiteSpace(user.Name) ? user.UserName ?? string.Empty : user.Name;
    }
}
