using JcStack.Abp.BlobStorage.EntityFrameworkCore;
using JcStack.Abp.BlobStorage.StoredFiles;

namespace JcStack.Abp.BlobStorage.StoredFiles;

/// <summary>
/// BlobStorageAppService EF Core 集成测试实现
/// </summary>
public class EfCoreBlobStorageAppService_Tests : BlobStorageAppService_Tests<BlobStorageEntityFrameworkCoreTestModule>
{
}
