using JcStack.Abp.AuditLogging.Localization;
using Volo.Abp.Application.Services;

namespace JcStack.Abp.AuditLogging;

public abstract class AuditLoggingAppService : ApplicationService
{
    protected AuditLoggingAppService()
    {
        LocalizationResource = typeof(AuditLoggingResource);
        ObjectMapperContext = typeof(AuditLoggingApplicationModule);
    }
}
