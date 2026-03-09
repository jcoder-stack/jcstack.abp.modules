using JcStack.Abp.Identity.Localization;
using Volo.Abp.Application.Services;

namespace JcStack.Abp.Identity;

public abstract class JcStackAbpIdentityAppServiceBase : ApplicationService
{
    protected JcStackAbpIdentityAppServiceBase()
    {
        LocalizationResource = typeof(JcStackAbpIdentityResource);
        ObjectMapperContext = typeof(JcStackAbpIdentityApplicationModule);
    }
}
