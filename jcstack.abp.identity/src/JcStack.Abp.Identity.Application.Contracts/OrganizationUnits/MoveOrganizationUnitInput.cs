using System;

namespace JcStack.Abp.Identity.OrganizationUnits;

[Serializable]
public class MoveOrganizationUnitInput
{
    public Guid? NewParentId { get; set; }
}
