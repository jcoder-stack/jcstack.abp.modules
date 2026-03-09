using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JcStack.Abp.Identity.OrganizationUnits;
using JcStack.Abp.Identity.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Identity;

namespace JcStack.Abp.Identity.Lookup;

/// <summary>
/// 组织单元 Lookup 服务
/// 提供独立的查找权限，用于其他模块引用组织单元
/// </summary>
[Authorize(JcStackAbpIdentityPermissions.OrganizationUnits.Lookup)]
public class OrganizationUnitLookupAppService : JcStackAbpIdentityAppServiceBase, IOrganizationUnitLookupAppService
{
    private readonly IOrganizationUnitRepository _organizationUnitRepository;

    public OrganizationUnitLookupAppService(IOrganizationUnitRepository organizationUnitRepository)
    {
        _organizationUnitRepository = organizationUnitRepository;
    }

    public async Task<List<OrganizationUnitLookupDto>> GetLookupAsync()
    {
        var organizationUnits = await _organizationUnitRepository.GetListAsync(includeDetails: false);

        return organizationUnits.Select(ou => new OrganizationUnitLookupDto
        {
            Id = ou.Id,
            DisplayName = ou.DisplayName
        }).ToList();
    }
}
