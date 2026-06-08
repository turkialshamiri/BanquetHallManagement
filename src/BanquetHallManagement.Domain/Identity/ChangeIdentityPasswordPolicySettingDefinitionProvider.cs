using Volo.Abp.Identity.Settings;
using Volo.Abp.Settings;

namespace BanquetHallManagement.Identity;

public class ChangeIdentityPasswordPolicySettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        var requiredLength = context.GetOrNull(IdentitySettingNames.Password.RequiredLength);
        if (requiredLength != null)
        {
            requiredLength.DefaultValue = "6";
        }

        var requireNonAlphanumeric = context.GetOrNull(IdentitySettingNames.Password.RequireNonAlphanumeric);
        if (requireNonAlphanumeric != null)
        {
            requireNonAlphanumeric.DefaultValue = true.ToString();
        }

        var requireLowercase = context.GetOrNull(IdentitySettingNames.Password.RequireLowercase);
        if (requireLowercase != null)
        {
            requireLowercase.DefaultValue = true.ToString();
        }

        var requireUppercase = context.GetOrNull(IdentitySettingNames.Password.RequireUppercase);
        if (requireUppercase != null)
        {
            requireUppercase.DefaultValue = true.ToString();
        }

        var requireDigit = context.GetOrNull(IdentitySettingNames.Password.RequireDigit);
        if (requireDigit != null)
        {
            requireDigit.DefaultValue = true.ToString();
        }
    }
}
