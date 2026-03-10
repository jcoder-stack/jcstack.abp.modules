using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace JcStack.Abp.Keycloak.Identity.Samples;

public interface ISampleAppService : IApplicationService
{
    /// <summary>
    /// 获取当前用户信息（用于测试登录是否正确）
    /// </summary>
    Task<CurrentUserInfoDto> GetCurrentUserInfoAsync();
}
