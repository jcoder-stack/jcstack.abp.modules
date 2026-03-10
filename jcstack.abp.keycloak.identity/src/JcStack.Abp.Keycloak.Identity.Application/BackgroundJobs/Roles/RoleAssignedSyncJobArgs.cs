using System;

namespace JcStack.Abp.Keycloak.Identity.BackgroundJobs.Roles;

public class RoleAssignedSyncJobArgs
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}
