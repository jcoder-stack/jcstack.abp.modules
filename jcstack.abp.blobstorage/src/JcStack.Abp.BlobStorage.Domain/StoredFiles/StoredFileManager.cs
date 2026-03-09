using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.MultiTenancy;

namespace JcStack.Abp.BlobStorage.StoredFiles;

/// <summary>
/// 存储文件领域服务
/// </summary>
public class StoredFileManager : DomainService
{
    private readonly IStoredFileRepository _storedFileRepository;
    private readonly ICurrentTenant _currentTenant;

    public StoredFileManager(
        IStoredFileRepository storedFileRepository,
        ICurrentTenant currentTenant)
    {
        _storedFileRepository = storedFileRepository;
        _currentTenant = currentTenant;
    }

    /// <summary>
    /// 创建存储文件记录
    /// </summary>
    /// <param name="fileName">原始文件名</param>
    /// <param name="contentType">MIME 类型</param>
    /// <param name="fileSize">文件大小</param>
    /// <param name="blobContainerName">Blob 容器名称</param>
    /// <param name="checksum">校验和（可选）</param>
    /// <returns>存储文件实体</returns>
    public async Task<StoredFile> CreateAsync(
        string fileName,
        string contentType,
        long fileSize,
        string blobContainerName,
        string? checksum = null)
    {
        var blobName = GenerateBlobName(fileName);

        var storedFile = new StoredFile(
            GuidGenerator.Create(),
            fileName,
            contentType,
            fileSize,
            blobContainerName,
            blobName,
            checksum);

        return await _storedFileRepository.InsertAsync(storedFile);
    }

    /// <summary>
    /// 生成 BlobName
    /// 格式: {TenantId}/{yyyy}/{MM}/{Guid}{Extension}
    /// </summary>
    /// <param name="fileName">原始文件名</param>
    /// <returns>Blob 存储名称</returns>
    public string GenerateBlobName(string fileName)
    {
        Check.NotNullOrWhiteSpace(fileName, nameof(fileName));

        var tenantFolder = _currentTenant.Id?.ToString() ?? "host";
        var now = Clock.Now;
        var year = now.ToString("yyyy");
        var month = now.ToString("MM");
        var fileId = GuidGenerator.Create();
        var extension = Path.GetExtension(fileName);

        // 确保扩展名安全
        var safeExtension = GetSafeExtension(extension);

        return $"{tenantFolder}/{year}/{month}/{fileId}{safeExtension}";
    }

    /// <summary>
    /// 验证文件扩展名
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="allowedExtensions">允许的扩展名列表</param>
    /// <returns>是否允许</returns>
    public bool IsExtensionAllowed(string fileName, IEnumerable<string> allowedExtensions)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var extension = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(extension))
        {
            return false;
        }

        return allowedExtensions.Any(e =>
            e.Equals(extension, StringComparison.OrdinalIgnoreCase) ||
            e.Equals(extension.TrimStart('.'), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 验证文件大小
    /// </summary>
    /// <param name="fileSize">文件大小</param>
    /// <param name="maxFileSize">最大文件大小</param>
    /// <returns>是否在限制范围内</returns>
    public bool IsFileSizeAllowed(long fileSize, long maxFileSize)
    {
        return fileSize > 0 && fileSize <= maxFileSize;
    }

    /// <summary>
    /// 获取安全的扩展名
    /// </summary>
    private static string GetSafeExtension(string? extension)
    {
        if (string.IsNullOrEmpty(extension))
        {
            return string.Empty;
        }

        // 只保留字母、数字和点
        var safeChars = extension
            .Where(c => char.IsLetterOrDigit(c) || c == '.')
            .ToArray();

        return new string(safeChars).ToLowerInvariant();
    }
}
