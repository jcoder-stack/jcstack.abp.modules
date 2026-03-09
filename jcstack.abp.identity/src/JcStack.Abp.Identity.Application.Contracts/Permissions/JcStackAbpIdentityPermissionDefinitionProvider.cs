using JcStack.Abp.Identity.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace JcStack.Abp.Identity.Permissions;

public class JcStackAbpIdentityPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(
            JcStackAbpIdentityPermissions.GroupName,
            L("Permission:OrganizationUnits"));

        var organizationUnits = group.AddPermission(
            JcStackAbpIdentityPermissions.OrganizationUnits.Default,
            L("Permission:OrganizationUnits.Default"));

        organizationUnits.AddChild(
            JcStackAbpIdentityPermissions.OrganizationUnits.Create,
            L("Permission:OrganizationUnits.Create"));

        organizationUnits.AddChild(
            JcStackAbpIdentityPermissions.OrganizationUnits.Update,
            L("Permission:OrganizationUnits.Update"));

        organizationUnits.AddChild(
            JcStackAbpIdentityPermissions.OrganizationUnits.Delete,
            L("Permission:OrganizationUnits.Delete"));

        organizationUnits.AddChild(
            JcStackAbpIdentityPermissions.OrganizationUnits.Move,
            L("Permission:OrganizationUnits.Move"));

        organizationUnits.AddChild(
            JcStackAbpIdentityPermissions.OrganizationUnits.ManageMembers,
            L("Permission:OrganizationUnits.ManageMembers"));

        organizationUnits.AddChild(
            JcStackAbpIdentityPermissions.OrganizationUnits.ManageRoles,
            L("Permission:OrganizationUnits.ManageRoles"));

        // Lookup 权限（独立权限，不是 Default 的子权限）
        group.AddPermission(
            JcStackAbpIdentityPermissions.OrganizationUnits.Lookup,
            L("Permission:OrganizationUnits.Lookup"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<JcStackAbpIdentityResource>(name);
    }
}
