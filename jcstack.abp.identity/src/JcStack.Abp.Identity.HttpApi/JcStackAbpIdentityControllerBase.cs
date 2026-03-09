using JcStack.Abp.Identity.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace JcStack.Abp.Identity;

public abstract class JcStackAbpIdentityControllerBase : AbpControllerBase
{
    protected JcStackAbpIdentityControllerBase()
    {
        LocalizationResource = typeof(JcStackAbpIdentityResource);
    }
}
