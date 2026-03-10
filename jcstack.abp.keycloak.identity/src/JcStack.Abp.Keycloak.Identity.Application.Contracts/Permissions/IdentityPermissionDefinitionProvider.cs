using JcStack.Abp.Keycloak.Identity.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace JcStack.Abp.Keycloak.Identity.Permissions;

public class IdentityPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(IdentityPermissions.GroupName, L("Permission:Identity"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<JcStackAbpKeycloakIdentityResource>(name);
    }
}
