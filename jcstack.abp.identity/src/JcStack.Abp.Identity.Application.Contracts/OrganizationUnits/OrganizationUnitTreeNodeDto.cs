using System;
using System.Collections.Generic;

namespace JcStack.Abp.Identity.OrganizationUnits;

[Serializable]
public class OrganizationUnitTreeNodeDto
{
    public Guid Id { get; set; }

    public Guid? ParentId { get; set; }

    public string Code { get; set; } = null!;

    public string DisplayName { get; set; } = null!;

    public int MemberCount { get; set; }

    public int RoleCount { get; set; }

    public List<OrganizationUnitTreeNodeDto> Children { get; set; } = [];
}
