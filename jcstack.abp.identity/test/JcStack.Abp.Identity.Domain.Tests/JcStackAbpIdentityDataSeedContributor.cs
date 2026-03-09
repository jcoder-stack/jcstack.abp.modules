using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace JcStack.Abp.Identity;

public class JcStackAbpIdentityDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    protected IOrganizationUnitRepository OrganizationUnitRepository { get; }
    protected OrganizationUnitManager OrganizationUnitManager { get; }
    protected ICurrentTenant CurrentTenant { get; }

    public JcStackAbpIdentityDataSeedContributor(
        IOrganizationUnitRepository organizationUnitRepository,
        OrganizationUnitManager organizationUnitManager,
        ICurrentTenant currentTenant)
    {
        OrganizationUnitRepository = organizationUnitRepository;
        OrganizationUnitManager = organizationUnitManager;
        CurrentTenant = currentTenant;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        var rootOu = new OrganizationUnit(
            JcStackAbpIdentityTestConsts.RootOrganizationUnitId,
            "Root Organization",
            null,
            CurrentTenant.Id);

        await OrganizationUnitManager.CreateAsync(rootOu);

        var childOu = new OrganizationUnit(
            JcStackAbpIdentityTestConsts.ChildOrganizationUnitId,
            "Child Organization",
            JcStackAbpIdentityTestConsts.RootOrganizationUnitId,
            CurrentTenant.Id);

        await OrganizationUnitManager.CreateAsync(childOu);
    }
}
