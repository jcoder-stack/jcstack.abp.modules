using System.Threading.Tasks;
using JcStack.Abp.Identity.OrganizationUnits;
using Shouldly;
using Volo.Abp.Application.Dtos;
using Xunit;

namespace JcStack.Abp.Identity;

public class OrganizationUnitAppServiceTests : JcStackAbpIdentityTestBase<JcStackAbpIdentityApplicationTestModule>
{
    private readonly IOrganizationUnitAppService _organizationUnitAppService;

    public OrganizationUnitAppServiceTests()
    {
        _organizationUnitAppService = GetRequiredService<IOrganizationUnitAppService>();
    }

    [Fact]
    public async Task Should_Get_Organization_Unit()
    {
        var result = await _organizationUnitAppService.GetAsync(
            JcStackAbpIdentityTestConsts.RootOrganizationUnitId);

        result.ShouldNotBeNull();
        result.DisplayName.ShouldBe("Root Organization");
    }

    [Fact]
    public async Task Should_Get_Organization_Unit_List()
    {
        var result = await _organizationUnitAppService.GetListAsync(
            new PagedAndSortedResultRequestDto());

        result.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task Should_Get_Tree()
    {
        var result = await _organizationUnitAppService.GetTreeAsync();

        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
        result[0].DisplayName.ShouldBe("Root Organization");
        result[0].Children.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Should_Create_Organization_Unit()
    {
        var result = await _organizationUnitAppService.CreateAsync(
            new CreateOrganizationUnitInput
            {
                DisplayName = "New Organization"
            });

        result.ShouldNotBeNull();
        result.DisplayName.ShouldBe("New Organization");
        result.ParentId.ShouldBeNull();
    }

    [Fact]
    public async Task Should_Get_Lookup()
    {
        var result = await _organizationUnitAppService.GetLookupAsync();

        result.ShouldNotBeNull();
        result.ShouldNotBeEmpty();
    }
}
