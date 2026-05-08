using Microsoft.Extensions.Localization;
using BanquetHallManagement.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace BanquetHallManagement;

[Dependency(ReplaceServices = true)]
public class BanquetHallManagementBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<BanquetHallManagementResource> _localizer;

    public BanquetHallManagementBrandingProvider(IStringLocalizer<BanquetHallManagementResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
