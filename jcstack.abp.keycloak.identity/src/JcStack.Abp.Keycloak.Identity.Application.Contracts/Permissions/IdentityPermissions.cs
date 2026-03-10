using Volo.Abp.Reflection;

namespace JcStack.Abp.Keycloak.Identity.Permissions;

public class IdentityPermissions
{
    public const string GroupName = "Identity";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(IdentityPermissions));
    }
}
