using JcStack.Abp.AuditLogging.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace JcStack.Abp.AuditLogging;

public abstract class AuditLoggingControllerBase : AbpControllerBase
{
    protected AuditLoggingControllerBase()
    {
        LocalizationResource = typeof(AuditLoggingResource);
    }
}
