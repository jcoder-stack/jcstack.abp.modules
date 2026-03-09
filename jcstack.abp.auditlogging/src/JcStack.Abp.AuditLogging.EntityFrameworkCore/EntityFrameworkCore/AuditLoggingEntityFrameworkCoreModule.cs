using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace JcStack.Abp.AuditLogging.EntityFrameworkCore;

[DependsOn(
    typeof(AbpAuditLoggingEntityFrameworkCoreModule),
    typeof(AuditLoggingDomainModule)
)]
public class AuditLoggingEntityFrameworkCoreModule : AbpModule
{
}
