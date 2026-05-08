using BanquetHallManagement.Localization;
using Volo.Abp.Application.Services;

namespace BanquetHallManagement;

/* Inherit your application services from this class.
 */
public abstract class BanquetHallManagementAppService : ApplicationService
{
    protected BanquetHallManagementAppService()
    {
        LocalizationResource = typeof(BanquetHallManagementResource);
    }
}
