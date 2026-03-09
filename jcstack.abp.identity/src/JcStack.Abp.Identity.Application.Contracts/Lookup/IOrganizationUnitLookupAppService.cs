using System.Collections.Generic;
using System.Threading.Tasks;
using JcStack.Abp.Identity.OrganizationUnits;
using Volo.Abp.Application.Services;

namespace JcStack.Abp.Identity.Lookup;

/// <summary>
/// 组织单元 Lookup 服务接口
/// 提供独立的查找权限，用于其他模块引用组织单元
/// </summary>
public interface IOrganizationUnitLookupAppService : IApplicationService
{
    /// <summary>
    /// 获取组织单元下拉列表
    /// </summary>
    Task<List<OrganizationUnitLookupDto>> GetLookupAsync();
}
