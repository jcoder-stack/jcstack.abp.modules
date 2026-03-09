using System;
using Volo.Abp.Application.Dtos;

namespace JcStack.Abp.Identity.OrganizationUnits;

[Serializable]
public class OrganizationUnitLookupDto : EntityDto<Guid>
{
    public string DisplayName { get; set; } = null!;
}
