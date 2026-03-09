using Volo.Abp.Application;
using Volo.Abp.AuditLogging;
using Volo.Abp.Modularity;

namespace JcStack.Abp.AuditLogging;

[DependsOn(
    typeof(AbpAuditLoggingDomainModule),
    typeof(AuditLoggingDomainModule),
    typeof(AuditLoggingApplicationContractsModule),
    typeof(AbpDddApplicationModule)
)]
public class AuditLoggingApplicationModule : AbpModule
{
}
