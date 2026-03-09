using Volo.Abp.AuditLogging;
using Volo.Abp.Modularity;

namespace JcStack.Abp.AuditLogging;

[DependsOn(
    typeof(AbpAuditLoggingDomainModule),
    typeof(AuditLoggingDomainSharedModule)
)]
public class AuditLoggingDomainModule : AbpModule
{
}
