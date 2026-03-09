using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace JcStack.Abp.BlobStorage.StoredFiles;

/// <summary>
/// 存储文件仓储接口
/// </summary>
public interface IStoredFileRepository : IBasicRepository<StoredFile, Guid>
{
    /// <summary>
    /// 根据 BlobName 查找文件
    /// </summary>
    /// <param name="blobName">Blob 存储名称</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>存储文件实体</returns>
    Task<StoredFile?> FindByBlobNameAsync(
        string blobName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取文件列表
    /// </summary>
    /// <param name="skipCount">跳过数量</param>
    /// <param name="maxResultCount">最大返回数量</param>
    /// <param name="sorting">排序字段</param>
    /// <param name="filter">过滤条件（文件名模糊匹配）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>文件列表</returns>
    Task<List<StoredFile>> GetListAsync(
        int skipCount,
        int maxResultCount,
        string? sorting = null,
        string? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 获取文件总数
    /// </summary>
    /// <param name="filter">过滤条件（文件名模糊匹配）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>文件总数</returns>
    Task<long> GetCountAsync(
        string? filter = null,
        CancellationToken cancellationToken = default);
}
