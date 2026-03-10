using Volo.Abp.Modularity;

namespace JcStack.Abp.Keycloak.Identity;

/* Inherit from this class for your application layer tests.
 * See SampleAppService_Tests for example.
 */
public abstract class JcStackAbpKeycloakIdentityApplicationTestBase<TStartupModule> : JcStackAbpKeycloakIdentityTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
