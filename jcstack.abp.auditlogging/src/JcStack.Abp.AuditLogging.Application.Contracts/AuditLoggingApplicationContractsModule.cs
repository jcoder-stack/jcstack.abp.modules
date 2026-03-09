using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;

namespace JcStack.Abp.AuditLogging;

[DependsOn(
    typeof(AuditLoggingDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule)
    )]
public class AuditLoggingApplicationContractsModule : AbpModule
{

}
