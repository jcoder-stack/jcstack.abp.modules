using JcStack.Abp.Keycloak.Identity.Localization;
using Volo.Abp.Application.Services;

namespace JcStack.Abp.Keycloak.Identity;

public abstract class JcStackAbpKeycloakIdentityAppService : ApplicationService
{
    protected JcStackAbpKeycloakIdentityAppService()
    {
        LocalizationResource = typeof(JcStackAbpKeycloakIdentityResource);
        ObjectMapperContext = typeof(JcStackAbpKeycloakIdentityApplicationModule);
    }
}
