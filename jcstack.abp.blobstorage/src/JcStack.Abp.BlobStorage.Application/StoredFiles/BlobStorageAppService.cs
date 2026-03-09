using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JcStack.Abp.BlobStorage.Localization;
using JcStack.Abp.BlobStorage.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Content;

namespace JcStack.Abp.BlobStorage.StoredFiles;

/// <summary>
/// 文件存储应用服务
/// </summary>
[Authorize(BlobStoragePermissions.Files.Default)]
public class BlobStorageAppService : ApplicationService, IBlobStorageAppService
{
    private readonly IStoredFileRepository _storedFileRepository;
    private readonly StoredFileManager _storedFileManager;
    private readonly IBlobContainer<BlobStorageBlobContainer> _blobContainer;
    private readonly BlobStorageOptions _options;

    public BlobStorageAppService(
        IStoredFileRepository storedFileRepository,
        StoredFileManager storedFileManager,
        IBlobContainer<BlobStorageBlobContainer> blobContainer,
        IOptions<BlobStorageOptions> options)
    {
        LocalizationResource = typeof(BlobStorageResource);

        _storedFileRepository = storedFileRepository;
        _storedFileManager = storedFileManager;
        _blobContainer = blobContainer;
        _options = options.Value;
    }

    /// <inheritdoc />
    [Authorize(BlobStoragePermissions.Files.Upload)]
    public async Task<StoredFileDto> UploadAsync(IRemoteStreamContent input)
    {
        if (input == null || input.ContentLength == null || input.ContentLength == 0)
        {
            throw new UserFriendlyException(L["FileStorage:FileUploadFailed"]);
        }

        var fileName = input.FileName;
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new BusinessException(BlobStorageErrorCodes.InvalidFileName);
        }

        // 验证文件大小
        var fileSize = input.ContentLength.Value;
        if (!_storedFileManager.IsFileSizeAllowed(fileSize, _options.MaxFileSize))
        {
            throw new BusinessException(BlobStorageErrorCodes.FileSizeExceeded)
                .WithData("MaxSize", FormatFileSize(_options.MaxFileSize));
        }

        // 验证文件类型
        if (!_storedFileManager.IsExtensionAllowed(fileName, _options.AllowedExtensions))
        {
            var extension = Path.GetExtension(fileName);
            throw new BusinessException(BlobStorageErrorCodes.FileTypeNotAllowed)
                .WithData("Extension", extension ?? "unknown");
        }

        // 读取文件内容
        using var memoryStream = new MemoryStream();
        await input.GetStream().CopyToAsync(memoryStream);
        var fileContent = memoryStream.ToArray();

        // 计算校验和
        string? checksum = null;
        if (_options.CalculateChecksum)
        {
            checksum = CalculateSha256Checksum(fileContent);
        }

        // 创建文件记录
        var storedFile = await _storedFileManager.CreateAsync(
            fileName,
            input.ContentType ?? "application/octet-stream",
            fileSize,
            _options.DefaultBlobContainerName,
            checksum);

        // 保存到 Blob 存储
        await _blobContainer.SaveAsync(storedFile.BlobName, fileContent, overrideExisting: true);

        return MapToDto(storedFile);
    }

    /// <inheritdoc />
    public async Task<StoredFileDto> GetAsync(Guid id)
    {
        var storedFile = await _storedFileRepository.GetAsync(id);
        return MapToDto(storedFile);
    }

    /// <inheritdoc />
    [Authorize(BlobStoragePermissions.Files.Download)]
    public async Task<IRemoteStreamContent> DownloadAsync(Guid id)
    {
        var storedFile = await _storedFileRepository.GetAsync(id);

        var blobStream = await _blobContainer.GetOrNullAsync(storedFile.BlobName);
        if (blobStream == null)
        {
            throw new BusinessException(BlobStorageErrorCodes.FileNotFound);
        }

        return new RemoteStreamContent(blobStream, storedFile.FileName, storedFile.ContentType);
    }

    /// <inheritdoc />
    public async Task<PagedResultDto<StoredFileDto>> GetListAsync(GetStoredFileListInput input)
    {
        var totalCount = await _storedFileRepository.GetCountAsync(input.Filter);
        var items = await _storedFileRepository.GetListAsync(
            input.SkipCount,
            input.MaxResultCount,
            input.Sorting,
            input.Filter);

        return new PagedResultDto<StoredFileDto>(
            totalCount,
            items.Select(MapToDto).ToList());
    }

    /// <inheritdoc />
    [Authorize(BlobStoragePermissions.Files.Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var storedFile = await _storedFileRepository.GetAsync(id);

        // 从 Blob 存储删除
        await _blobContainer.DeleteAsync(storedFile.BlobName);

        // 删除数据库记录
        await _storedFileRepository.DeleteAsync(id);
    }

    /// <summary>
    /// 映射到 DTO
    /// </summary>
    private StoredFileDto MapToDto(StoredFile storedFile)
    {
        return new StoredFileDto
        {
            Id = storedFile.Id,
            FileName = storedFile.FileName,
            ContentType = storedFile.ContentType,
            FileSize = storedFile.FileSize,
            Checksum = storedFile.Checksum,
            CreationTime = storedFile.CreationTime,
            CreatorId = storedFile.CreatorId,
            LastModificationTime = storedFile.LastModificationTime,
            LastModifierId = storedFile.LastModifierId
        };
    }

    /// <summary>
    /// 计算 SHA256 校验和
    /// </summary>
    private static string CalculateSha256Checksum(byte[] content)
    {
        var hashBytes = SHA256.HashData(content);
        return Convert.ToHexStringLower(hashBytes);
    }

    /// <summary>
    /// 格式化文件大小
    /// </summary>
    private static string FormatFileSize(long bytes)
    {
        string[] sizes = ["B", "KB", "MB", "GB", "TB"];
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
