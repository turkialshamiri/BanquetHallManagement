using BanquetHallManagement.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace BanquetHallManagement.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class BanquetHallManagementController : AbpControllerBase
{
    protected BanquetHallManagementController()
    {
        LocalizationResource = typeof(BanquetHallManagementResource);
    }
}
