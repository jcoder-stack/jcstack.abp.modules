using JcStack.Abp.Keycloak.Identity.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace JcStack.Abp.Keycloak.Identity;

public abstract class IdentityController : AbpControllerBase
{
    protected IdentityController()
    {
        LocalizationResource = typeof(JcStackAbpKeycloakIdentityResource);
    }
}
