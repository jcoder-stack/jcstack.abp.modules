using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.ObjectExtending;

namespace JcStack.Abp.Identity.OrganizationUnits;

[Serializable]
public class UpdateOrganizationUnitInput : ExtensibleObject
{
    [Required]
    [StringLength(128)]
    public string DisplayName { get; set; } = null!;

    public string? ConcurrencyStamp { get; set; }
}
