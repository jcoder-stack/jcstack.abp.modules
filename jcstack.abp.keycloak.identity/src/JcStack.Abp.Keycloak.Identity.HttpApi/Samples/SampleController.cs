using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace JcStack.Abp.Keycloak.Identity.Samples;

[Area(IdentityRemoteServiceConsts.ModuleName)]
[RemoteService(Name = IdentityRemoteServiceConsts.RemoteServiceName)]
[Route("api/identity/sample")]
public class SampleController : IdentityController, ISampleAppService
{
    private readonly ISampleAppService _sampleAppService;

    public SampleController(ISampleAppService sampleAppService)
    {
        _sampleAppService = sampleAppService;
    }

    [HttpGet]
    [Route("current-user")]
    [Authorize]
    public async Task<CurrentUserInfoDto> GetCurrentUserInfoAsync()
    {
        return await _sampleAppService.GetCurrentUserInfoAsync();
    }
}
