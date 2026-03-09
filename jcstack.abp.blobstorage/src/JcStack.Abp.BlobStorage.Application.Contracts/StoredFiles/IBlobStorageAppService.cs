using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace JcStack.Abp.BlobStorage.StoredFiles;

/// <summary>
/// 文件存储应用服务接口
/// </summary>
public interface IBlobStorageAppService : IApplicationService
{
    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="input">文件流</param>
    /// <returns>存储文件信息</returns>
    Task<StoredFileDto> UploadAsync(IRemoteStreamContent input);

    /// <summary>
    /// 获取文件信息
    /// </summary>
    /// <param name="id">文件ID</param>
    /// <returns>存储文件信息</returns>
    Task<StoredFileDto> GetAsync(Guid id);

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="id">文件ID</param>
    /// <returns>文件流</returns>
    Task<IRemoteStreamContent> DownloadAsync(Guid id);

    /// <summary>
    /// 获取文件列表
    /// </summary>
    /// <param name="input">查询参数</param>
    /// <returns>文件列表</returns>
    Task<PagedResultDto<StoredFileDto>> GetListAsync(GetStoredFileListInput input);

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="id">文件ID</param>
    Task DeleteAsync(Guid id);
}
