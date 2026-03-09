using Volo.Abp.Modularity;

namespace JcStack.Abp.BlobStorage;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */
public abstract class FileStorageDomainTestBase<TStartupModule> : BlobStorageTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
