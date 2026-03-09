using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JcStack.Abp.Identity.OrganizationUnits;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;

namespace JcStack.Abp.Identity;

[RemoteService(Name = JcStackAbpIdentityRemoteServiceConsts.RemoteServiceName)]
[Area(JcStackAbpIdentityRemoteServiceConsts.ModuleName)]
[Route("api/identity/organization-units")]
public class OrganizationUnitController : JcStackAbpIdentityControllerBase, IOrganizationUnitAppService
{
    protected IOrganizationUnitAppService OrganizationUnitAppService { get; }

    public OrganizationUnitController(IOrganizationUnitAppService organizationUnitAppService)
    {
        OrganizationUnitAppService = organizationUnitAppService;
    }

    [HttpGet]
    [Route("{id}")]
    public virtual Task<OrganizationUnitDto> GetAsync(Guid id)
    {
        return OrganizationUnitAppService.GetAsync(id);
    }

    [HttpGet]
    public virtual Task<PagedResultDto<OrganizationUnitDto>> GetListAsync([FromQuery] PagedAndSortedResultRequestDto input)
    {
        return OrganizationUnitAppService.GetListAsync(input);
    }

    [HttpPost]
    public virtual Task<OrganizationUnitDto> CreateAsync(CreateOrganizationUnitInput input)
    {
        return OrganizationUnitAppService.CreateAsync(input);
    }

    [HttpPut]
    [Route("{id}")]
    public virtual Task<OrganizationUnitDto> UpdateAsync(Guid id, UpdateOrganizationUnitInput input)
    {
        return OrganizationUnitAppService.UpdateAsync(id, input);
    }

    [HttpDelete]
    [Route("{id}")]
    public virtual Task DeleteAsync(Guid id)
    {
        return OrganizationUnitAppService.DeleteAsync(id);
    }

    [HttpGet]
    [Route("tree")]
    public virtual Task<List<OrganizationUnitTreeNodeDto>> GetTreeAsync()
    {
        return OrganizationUnitAppService.GetTreeAsync();
    }

    [HttpGet]
    [Route("children")]
    public virtual Task<List<OrganizationUnitDto>> GetChildrenAsync([FromQuery] Guid? parentId)
    {
        return OrganizationUnitAppService.GetChildrenAsync(parentId);
    }

    [HttpGet]
    [Route("{id}/all-children")]
    public virtual Task<List<OrganizationUnitDto>> GetAllChildrenAsync(Guid id)
    {
        return OrganizationUnitAppService.GetAllChildrenAsync(id);
    }

    [HttpPut]
    [Route("{id}/move")]
    public virtual Task MoveAsync(Guid id, MoveOrganizationUnitInput input)
    {
        return OrganizationUnitAppService.MoveAsync(id, input);
    }

    [HttpGet]
    [Route("{id}/members")]
    public virtual Task<PagedResultDto<IdentityUserDto>> GetMembersAsync(Guid id, [FromQuery] GetOrganizationUnitMembersInput input)
    {
        return OrganizationUnitAppService.GetMembersAsync(id, input);
    }

    [HttpPut]
    [Route("{id}/members/{userId}")]
    public virtual Task AddMemberAsync(Guid id, Guid userId)
    {
        return OrganizationUnitAppService.AddMemberAsync(id, userId);
    }

    [HttpDelete]
    [Route("{id}/members/{userId}")]
    public virtual Task RemoveMemberAsync(Guid id, Guid userId)
    {
        return OrganizationUnitAppService.RemoveMemberAsync(id, userId);
    }

    [HttpGet]
    [Route("{id}/roles")]
    public virtual Task<PagedResultDto<IdentityRoleDto>> GetRolesAsync(Guid id, [FromQuery] PagedAndSortedResultRequestDto input)
    {
        return OrganizationUnitAppService.GetRolesAsync(id, input);
    }

    [HttpPut]
    [Route("{id}/roles/{roleId}")]
    public virtual Task AddRoleAsync(Guid id, Guid roleId)
    {
        return OrganizationUnitAppService.AddRoleAsync(id, roleId);
    }

    [HttpDelete]
    [Route("{id}/roles/{roleId}")]
    public virtual Task RemoveRoleAsync(Guid id, Guid roleId)
    {
        return OrganizationUnitAppService.RemoveRoleAsync(id, roleId);
    }

    [HttpGet]
    [Route("lookup")]
    public virtual Task<List<OrganizationUnitLookupDto>> GetLookupAsync()
    {
        return OrganizationUnitAppService.GetLookupAsync();
    }
}
