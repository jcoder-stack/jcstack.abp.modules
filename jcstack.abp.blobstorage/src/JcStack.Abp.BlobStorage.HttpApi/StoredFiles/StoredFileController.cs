using System;
using System.Threading.Tasks;
using JcStack.Abp.BlobStorage.StoredFiles;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;

namespace JcStack.Abp.BlobStorage.Controllers;

/// <summary>
/// 文件存储 API 控制器
/// </summary>
[RemoteService(Name = BlobStorageRemoteServiceConsts.RemoteServiceName)]
[Area(BlobStorageRemoteServiceConsts.ModuleName)]
[Route("api/file-storage")]
public class StoredFileController : BlobStorageController
{
    private readonly IBlobStorageAppService _fileStorageAppService;

    public StoredFileController(IBlobStorageAppService fileStorageAppService)
    {
        _fileStorageAppService = fileStorageAppService;
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    /// <param name="file">文件</param>
    /// <returns>存储文件信息</returns>
    [HttpPost("upload")]
    [DisableRequestSizeLimit]
    public async Task<StoredFileDto> UploadAsync(IRemoteStreamContent file)
    {
        return await _fileStorageAppService.UploadAsync(file);
    }

    /// <summary>
    /// 获取文件信息
    /// </summary>
    /// <param name="id">文件ID</param>
    /// <returns>存储文件信息</returns>
    [HttpGet("{id}")]
    public async Task<StoredFileDto> GetAsync(Guid id)
    {
        return await _fileStorageAppService.GetAsync(id);
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    /// <param name="id">文件ID</param>
    /// <returns>文件流</returns>
    [HttpGet("{id}/download")]
    public async Task<IRemoteStreamContent> DownloadAsync(Guid id)
    {
        return await _fileStorageAppService.DownloadAsync(id);
    }

    /// <summary>
    /// 获取文件列表
    /// </summary>
    /// <param name="input">查询参数</param>
    /// <returns>文件列表</returns>
    [HttpGet]
    public async Task<PagedResultDto<StoredFileDto>> GetListAsync([FromQuery] GetStoredFileListInput input)
    {
        return await _fileStorageAppService.GetListAsync(input);
    }

    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="id">文件ID</param>
    [HttpDelete("{id}")]
    public async Task DeleteAsync(Guid id)
    {
        await _fileStorageAppService.DeleteAsync(id);
    }
}
