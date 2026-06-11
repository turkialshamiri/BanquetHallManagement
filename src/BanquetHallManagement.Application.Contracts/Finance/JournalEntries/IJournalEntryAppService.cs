using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement.Finance.JournalEntries;

public interface IJournalEntryAppService : IApplicationService
{
    Task<PagedResultDto<JournalEntryDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    Task<JournalEntryDto> GetAsync(Guid id);
}
