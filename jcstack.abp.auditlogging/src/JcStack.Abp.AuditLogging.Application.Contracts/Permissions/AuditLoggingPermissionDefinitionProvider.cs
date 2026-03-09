using JcStack.Abp.AuditLogging.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace JcStack.Abp.AuditLogging.Permissions;

public class AuditLoggingPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(
            AuditLoggingPermissions.GroupName,
            L("Permission:AuditLogging"));

        var auditLogs = group.AddPermission(
            AuditLoggingPermissions.AuditLogs.Default,
            L("Permission:AuditLogs"));

        auditLogs.AddChild(
            AuditLoggingPermissions.AuditLogs.EntityChanges,
            L("Permission:AuditLogs.EntityChanges"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<AuditLoggingResource>(name);
    }
}
