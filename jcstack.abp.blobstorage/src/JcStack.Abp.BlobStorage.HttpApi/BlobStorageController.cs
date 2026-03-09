using JcStack.Abp.BlobStorage.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace JcStack.Abp.BlobStorage;

public abstract class BlobStorageController : AbpControllerBase
{
    protected BlobStorageController()
    {
        LocalizationResource = typeof(BlobStorageResource);
    }
}
