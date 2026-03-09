using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JcStack.Abp.Identity.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;

namespace JcStack.Abp.Identity.OrganizationUnits;

[Authorize(JcStackAbpIdentityPermissions.OrganizationUnits.Default)]
public class OrganizationUnitAppService : JcStackAbpIdentityAppServiceBase, IOrganizationUnitAppService
{
    protected IOrganizationUnitRepository OrganizationUnitRepository { get; }
    protected OrganizationUnitManager OrganizationUnitManager { get; }
    protected IIdentityUserRepository UserRepository { get; }
    protected IIdentityRoleRepository RoleRepository { get; }

    public OrganizationUnitAppService(
        IOrganizationUnitRepository organizationUnitRepository,
        OrganizationUnitManager organizationUnitManager,
        IIdentityUserRepository userRepository,
        IIdentityRoleRepository roleRepository)
    {
        OrganizationUnitRepository = organizationUnitRepository;
        OrganizationUnitManager = organizationUnitManager;
        UserRepository = userRepository;
        RoleRepository = roleRepository;
    }

    public virtual async Task<OrganizationUnitDto> GetAsync(Guid id)
    {
        var organizationUnit = await OrganizationUnitRepository.GetAsync(id);
        return await ToOrganizationUnitDtoAsync(organizationUnit);
    }

    public virtual async Task<PagedResultDto<OrganizationUnitDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var totalCount = await OrganizationUnitRepository.GetCountAsync();
        var items = await OrganizationUnitRepository.GetListAsync(
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount);

        var dtos = new List<OrganizationUnitDto>();
        foreach (var item in items)
        {
            dtos.Add(await ToOrganizationUnitDtoAsync(item));
        }

        return new PagedResultDto<OrganizationUnitDto>(totalCount, dtos);
    }

    [Authorize(JcStackAbpIdentityPermissions.OrganizationUnits.Create)]
    public virtual async Task<OrganizationUnitDto> CreateAsync(CreateOrganizationUnitInput input)
    {
        var organizationUnit = new OrganizationUnit(
            GuidGenerator.Create(),
            input.DisplayName,
            input.ParentId,
            CurrentTenant.Id);

        input.MapExtraPropertiesTo(organizationUnit);

        await OrganizationUnitManager.CreateAsync(organizationUnit);

        return await ToOrganizationUnitDtoAsync(organizationUnit);
    }

    [Authorize(JcStackAbpIdentityPermissions.OrganizationUnits.Update)]
    public virtual async Task<OrganizationUnitDto> UpdateAsync(Guid id, UpdateOrganizationUnitInput input)
    {
        var organizationUnit = await OrganizationUnitRepository.GetAsync(id);

        organizationUnit.ConcurrencyStamp = input.ConcurrencyStamp;
        organizationUnit.DisplayName = input.DisplayName;

        input.MapExtraPropertiesTo(organizationUnit);

        await OrganizationUnitManager.UpdateAsync(organizationUnit);

        return await ToOrganizationUnitDtoAsync(organizationUnit);
    }

    [Authorize(JcStackAbpIdentityPermissions.OrganizationUnits.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        await OrganizationUnitManager.DeleteAsync(id);
    }

    public virtual async Task<List<OrganizationUnitTreeNodeDto>> GetTreeAsync()
    {
        var allOrganizationUnits = await OrganizationUnitRepository.GetListAsync(includeDetails: false);
        var memberCounts = await GetMemberCountsAsync(allOrganizationUnits);
        var roleCounts = await GetRoleCountsAsync(allOrganizationUnits);

        var lookup = allOrganizationUnits.ToDictionary(ou => ou.Id);
        var rootNodes = new List<OrganizationUnitTreeNodeDto>();

        foreach (var organizationUnit in allOrganizationUnits)
        {
            var node = ToTreeNodeDto(organizationUnit, memberCounts, roleCounts);

            if (organizationUnit.ParentId == null)
            {
                rootNodes.Add(node);
            }
            else if (lookup.TryGetValue(organizationUnit.ParentId.Value, out _))
            {
                // Will be added as a child later
            }
        }

        // Build tree structure
        foreach (var organizationUnit in allOrganizationUnits.Where(ou => ou.ParentId != null))
        {
            var node = ToTreeNodeDto(organizationUnit, memberCounts, roleCounts);
            var parent = FindNode(rootNodes, organizationUnit.ParentId!.Value);
            parent?.Children.Add(node);
        }

        return rootNodes;
    }

    public virtual async Task<List<OrganizationUnitDto>> GetChildrenAsync(Guid? parentId)
    {
        var children = await OrganizationUnitRepository.GetChildrenAsync(parentId, includeDetails: false);

        var dtos = new List<OrganizationUnitDto>();
        foreach (var child in children)
        {
            dtos.Add(await ToOrganizationUnitDtoAsync(child));
        }

        return dtos;
    }

    public virtual async Task<List<OrganizationUnitDto>> GetAllChildrenAsync(Guid id)
    {
        var organizationUnit = await OrganizationUnitRepository.GetAsync(id);
        var children = await OrganizationUnitRepository.GetAllChildrenWithParentCodeAsync(
            organizationUnit.Code,
            organizationUnit.Id,
            includeDetails: false);

        var dtos = new List<OrganizationUnitDto>();
        foreach (var child in children)
        {
            dtos.Add(await ToOrganizationUnitDtoAsync(child));
        }

        return dtos;
    }

    [Authorize(JcStackAbpIdentityPermissions.OrganizationUnits.Move)]
    public virtual async Task MoveAsync(Guid id, MoveOrganizationUnitInput input)
    {
        await OrganizationUnitManager.MoveAsync(id, input.NewParentId);
    }

    [Authorize(JcStackAbpIdentityPermissions.OrganizationUnits.ManageMembers)]
    public virtual async Task<PagedResultDto<IdentityUserDto>> GetMembersAsync(Guid id, GetOrganizationUnitMembersInput input)
    {
        var organizationUnit = await OrganizationUnitRepository.GetAsync(id);

        var totalCount = await UserRepository.GetCountAsync(
            organizationUnitId: id,
            filter: input.Filter);

        var users = await UserRepository.GetListAsync(
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount,
            filter: input.Filter,
            organizationUnitId: id);

        return new PagedResultDto<IdentityUserDto>(
            totalCount,
            ObjectMapper.Map<List<IdentityUser>, List<IdentityUserDto>>(users));
    }

    [Authorize(JcStackAbpIdentityPermissions.OrganizationUnits.ManageMembers)]
    public virtual async Task AddMemberAsync(Guid id, Guid userId)
    {
        var user = await UserRepository.GetAsync(userId, includeDetails: true);

        if (user.OrganizationUnits.Any(ou => ou.OrganizationUnitId == id))
        {
            return;
        }

        user.AddOrganizationUnit(id);
        await UserRepository.UpdateAsync(user);
    }

    [Authorize(JcStackAbpIdentityPermissions.OrganizationUnits.ManageMembers)]
    public virtual async Task RemoveMemberAsync(Guid id, Guid userId)
    {
        var user = await UserRepository.GetAsync(userId, includeDetails: true);

        user.RemoveOrganizationUnit(id);
        await UserRepository.UpdateAsync(user);
    }

    [Authorize(JcStackAbpIdentityPermissions.OrganizationUnits.ManageRoles)]
    public virtual async Task<PagedResultDto<IdentityRoleDto>> GetRolesAsync(Guid id, PagedAndSortedResultRequestDto input)
    {
        var organizationUnit = await OrganizationUnitRepository.GetAsync(id, includeDetails: true);

        var roleIds = organizationUnit.Roles.Select(r => r.RoleId).ToList();

        if (!roleIds.Any())
        {
            return new PagedResultDto<IdentityRoleDto>(0, []);
        }

        var roles = await RoleRepository.GetListAsync(
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount);

        var filteredRoles = roles.Where(r => roleIds.Contains(r.Id)).ToList();

        return new PagedResultDto<IdentityRoleDto>(
            roleIds.Count,
            ObjectMapper.Map<List<IdentityRole>, List<IdentityRoleDto>>(filteredRoles));
    }

    [Authorize(JcStackAbpIdentityPermissions.OrganizationUnits.ManageRoles)]
    public virtual async Task AddRoleAsync(Guid id, Guid roleId)
    {
        var organizationUnit = await OrganizationUnitRepository.GetAsync(id, includeDetails: true);
        var role = await RoleRepository.GetAsync(roleId);

        if (organizationUnit.Roles.Any(r => r.RoleId == roleId))
        {
            return;
        }

        organizationUnit.AddRole(roleId);
        await OrganizationUnitRepository.UpdateAsync(organizationUnit);
    }

    [Authorize(JcStackAbpIdentityPermissions.OrganizationUnits.ManageRoles)]
    public virtual async Task RemoveRoleAsync(Guid id, Guid roleId)
    {
        var organizationUnit = await OrganizationUnitRepository.GetAsync(id, includeDetails: true);

        organizationUnit.RemoveRole(roleId);
        await OrganizationUnitRepository.UpdateAsync(organizationUnit);
    }

    public virtual async Task<List<OrganizationUnitLookupDto>> GetLookupAsync()
    {
        var organizationUnits = await OrganizationUnitRepository.GetListAsync(includeDetails: false);

        return organizationUnits.Select(ou => new OrganizationUnitLookupDto
        {
            Id = ou.Id,
            DisplayName = ou.DisplayName
        }).ToList();
    }

    protected virtual async Task<OrganizationUnitDto> ToOrganizationUnitDtoAsync(OrganizationUnit organizationUnit)
    {
        var dto = ObjectMapper.Map<OrganizationUnit, OrganizationUnitDto>(organizationUnit);

        dto.MemberCount = (int)await UserRepository.GetCountAsync(organizationUnitId: organizationUnit.Id);
        dto.RoleCount = organizationUnit.Roles?.Count ?? 0;

        return dto;
    }

    protected virtual async Task<Dictionary<Guid, int>> GetMemberCountsAsync(List<OrganizationUnit> organizationUnits)
    {
        var counts = new Dictionary<Guid, int>();
        foreach (var ou in organizationUnits)
        {
            counts[ou.Id] = (int)await UserRepository.GetCountAsync(organizationUnitId: ou.Id);
        }
        return counts;
    }

    protected virtual Task<Dictionary<Guid, int>> GetRoleCountsAsync(List<OrganizationUnit> organizationUnits)
    {
        var counts = new Dictionary<Guid, int>();
        foreach (var ou in organizationUnits)
        {
            counts[ou.Id] = ou.Roles?.Count ?? 0;
        }
        return Task.FromResult(counts);
    }

    protected virtual OrganizationUnitTreeNodeDto ToTreeNodeDto(
        OrganizationUnit organizationUnit,
        Dictionary<Guid, int> memberCounts,
        Dictionary<Guid, int> roleCounts)
    {
        return new OrganizationUnitTreeNodeDto
        {
            Id = organizationUnit.Id,
            ParentId = organizationUnit.ParentId,
            Code = organizationUnit.Code,
            DisplayName = organizationUnit.DisplayName,
            MemberCount = memberCounts.GetValueOrDefault(organizationUnit.Id),
            RoleCount = roleCounts.GetValueOrDefault(organizationUnit.Id),
            Children = []
        };
    }

    protected virtual OrganizationUnitTreeNodeDto? FindNode(List<OrganizationUnitTreeNodeDto> nodes, Guid id)
    {
        foreach (var node in nodes)
        {
            if (node.Id == id)
            {
                return node;
            }

            var found = FindNode(node.Children, id);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
}
