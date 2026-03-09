using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace JcStack.Abp.BlobStorage.Samples;

public interface ISampleAppService : IApplicationService
{
    Task<SampleDto> GetAsync();

    Task<SampleDto> GetAuthorizedAsync();
}
