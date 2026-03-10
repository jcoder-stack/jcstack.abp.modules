using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace JcStack.Abp.Keycloak.Identity.Samples;

public class SampleAppService : JcStackAbpKeycloakIdentityAppService, ISampleAppService
{
    private readonly ICurrentKeycloakUser _currentKeycloakUser;

    public SampleAppService(ICurrentKeycloakUser currentKeycloakUser)
    {
        _currentKeycloakUser = currentKeycloakUser;
    }

    [Authorize]
    public Task<CurrentUserInfoDto> GetCurrentUserInfoAsync()
    {
        var result = new CurrentUserInfoDto
        {
            AbpUser = new AbpUserDto
            {
                Id = CurrentUser.Id,
                UserName = CurrentUser.UserName,
                Email = CurrentUser.Email,
                IsAuthenticated = CurrentUser.IsAuthenticated,
                Roles = CurrentUser.Roles?.ToArray()
            },
            KeycloakUser = new KeycloakUserDto
            {
                KeycloakUserId = _currentKeycloakUser.KeycloakUserId,
                LocalUserId = _currentKeycloakUser.LocalUserId,
                IsAuthenticated = _currentKeycloakUser.IsAuthenticated,
                HasLocalUser = _currentKeycloakUser.HasLocalUser
            }
        };

        return Task.FromResult(result);
    }
}
