using System;
using Volo.Abp.Application.Dtos;

namespace JcStack.Abp.Identity.OrganizationUnits;

[Serializable]
public class GetOrganizationUnitMembersInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
