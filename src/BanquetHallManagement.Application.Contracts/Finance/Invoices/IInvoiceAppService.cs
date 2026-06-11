using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Finance.Invoices;

public interface IInvoiceAppService : IApplicationService
{
    Task<InvoiceDto> GetAsync(Guid id);

    Task<ListResultDto<InvoiceDto>> GetByReservationAsync(Guid reservationId);

    Task<InvoicePrintDataDto> GetPrintDataAsync(Guid id);
}
