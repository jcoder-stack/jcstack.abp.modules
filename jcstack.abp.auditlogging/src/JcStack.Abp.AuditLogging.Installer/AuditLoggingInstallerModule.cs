using JcStack.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace JcStack.Abp.AuditLogging;

[DependsOn(
    typeof(AuditLoggingEntityFrameworkCoreModule),
    typeof(AuditLoggingApplicationModule),
    typeof(AuditLoggingHttpApiModule)
)]
public class AuditLoggingInstallerModule : AbpModule
{
}
