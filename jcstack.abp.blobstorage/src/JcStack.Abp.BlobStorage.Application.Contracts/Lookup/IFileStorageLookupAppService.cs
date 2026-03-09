using System;
using System.Threading.Tasks;
using JcStack.Abp.BlobStorage.StoredFiles;
using Volo.Abp.Application.Services;

namespace JcStack.Abp.BlobStorage.Lookup;

/// <summary>
/// 文件存储 Lookup 服务接口
/// 提供独立的查找权限，用于其他模块引用文件
/// </summary>
public interface IFileStorageLookupAppService : IApplicationService
{
    /// <summary>
    /// 获取文件基本信息（用于显示已关联的文件）
    /// </summary>
    Task<StoredFileLookupDto?> GetAsync(Guid id);
}
