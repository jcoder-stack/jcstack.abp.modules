using JcStack.Abp.BlobStorage.EntityFrameworkCore;
using JcStack.Abp.BlobStorage.StoredFiles;

namespace JcStack.Abp.BlobStorage.StoredFiles;

/// <summary>
/// StoredFileManager EF Core 集成测试实现
/// </summary>
public class EfCoreStoredFileManager_Tests : StoredFileManager_Tests<BlobStorageEntityFrameworkCoreTestModule>
{
}
