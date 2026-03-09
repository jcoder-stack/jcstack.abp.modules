using Volo.Abp.Modularity;

namespace JcStack.Abp.BlobStorage;

/* Inherit from this class for your application layer tests.
 * See SampleAppService_Tests for example.
 */
public abstract class FileStorageApplicationTestBase<TStartupModule> : BlobStorageTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
