using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.ObjectExtending;

namespace JcStack.Abp.Identity.OrganizationUnits;

[Serializable]
public class CreateOrganizationUnitInput : ExtensibleObject
{
    public Guid? ParentId { get; set; }

    [Required]
    [StringLength(128)]
    public string DisplayName { get; set; } = null!;
}
