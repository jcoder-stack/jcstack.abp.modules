using JcStack.Abp.Identity.OrganizationUnits;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;

namespace JcStack.Abp.Identity.Application.OrganizationUnits;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class OrganizationUnitMapper : MapperBase<OrganizationUnit, OrganizationUnitDto>
{
    [MapperIgnoreSource(nameof(OrganizationUnit.ExtraProperties))]
    [MapperIgnoreSource(nameof(OrganizationUnit.Roles))]
    public override partial OrganizationUnitDto Map(OrganizationUnit source);

    public override partial void Map(OrganizationUnit source, OrganizationUnitDto destination);

    public partial OrganizationUnitTreeNodeDto ToTreeNodeDto(OrganizationUnit source);

    public partial OrganizationUnitLookupDto ToLookupDto(OrganizationUnit source);
}
