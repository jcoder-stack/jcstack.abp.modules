using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace JcStack.Abp.Identity.OrganizationUnits;

public interface IOrganizationUnitAppService : IApplicationService
{
    Task<OrganizationUnitDto> GetAsync(Guid id);

    Task<PagedResultDto<OrganizationUnitDto>> GetListAsync(PagedAndSortedResultRequestDto input);

    Task<OrganizationUnitDto> CreateAsync(CreateOrganizationUnitInput input);

    Task<OrganizationUnitDto> UpdateAsync(Guid id, UpdateOrganizationUnitInput input);

    Task DeleteAsync(Guid id);

    Task<List<OrganizationUnitTreeNodeDto>> GetTreeAsync();

    Task<List<OrganizationUnitDto>> GetChildrenAsync(Guid? parentId);

    Task<List<OrganizationUnitDto>> GetAllChildrenAsync(Guid id);

    Task MoveAsync(Guid id, MoveOrganizationUnitInput input);

    Task<PagedResultDto<IdentityUserDto>> GetMembersAsync(Guid id, GetOrganizationUnitMembersInput input);

    Task AddMemberAsync(Guid id, Guid userId);

    Task RemoveMemberAsync(Guid id, Guid userId);

    Task<PagedResultDto<IdentityRoleDto>> GetRolesAsync(Guid id, PagedAndSortedResultRequestDto input);

    Task AddRoleAsync(Guid id, Guid roleId);

    Task RemoveRoleAsync(Guid id, Guid roleId);

    Task<List<OrganizationUnitLookupDto>> GetLookupAsync();
}
