using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Modularity;
using Xunit;

namespace JcStack.Abp.Keycloak.Identity.Samples;

public abstract class SampleAppService_Tests<TStartupModule> : JcStackAbpKeycloakIdentityApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ISampleAppService _sampleAppService;

    protected SampleAppService_Tests()
    {
        _sampleAppService = GetRequiredService<ISampleAppService>();
    }

    [Fact]
    public async Task GetCurrentUserInfoAsync_ShouldReturnUserInfo()
    {
        var result = await _sampleAppService.GetCurrentUserInfoAsync();
        result.ShouldNotBeNull();
    }
}
