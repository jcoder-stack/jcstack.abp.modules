using System;

namespace JcStack.Abp.Keycloak.Identity.BackgroundJobs.Roles;

/// <summary>
/// 角色更新同步作业参数
/// </summary>
public class RoleUpdatedSyncJobArgs
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}
