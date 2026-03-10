using Volo.Abp.Modularity;

namespace JcStack.Abp.Keycloak.Identity;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */
public abstract class JcStackAbpKeycloakIdentityDomainTestBase<TStartupModule> : JcStackAbpKeycloakIdentityTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
