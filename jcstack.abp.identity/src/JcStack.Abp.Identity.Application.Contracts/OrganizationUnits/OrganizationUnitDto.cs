using System;
using Volo.Abp.Application.Dtos;

namespace JcStack.Abp.Identity.OrganizationUnits;

[Serializable]
public class OrganizationUnitDto : ExtensibleAuditedEntityDto<Guid>
{
    public Guid? ParentId { get; set; }

    public string Code { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public int MemberCount { get; set; }

    public int RoleCount { get; set; }
}
