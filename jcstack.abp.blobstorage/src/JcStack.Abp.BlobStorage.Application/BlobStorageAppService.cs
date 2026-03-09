using JcStack.Abp.BlobStorage.Localization;
using Volo.Abp.Application.Services;

namespace JcStack.Abp.BlobStorage;

public abstract class BlobStorageAppService : ApplicationService
{
    protected BlobStorageAppService()
    {
        LocalizationResource = typeof(BlobStorageResource);
        ObjectMapperContext = typeof(BlobStorageApplicationModule);
    }
}
