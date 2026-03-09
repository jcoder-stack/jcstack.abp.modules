using System;
using System.Threading.Tasks;
using JcStack.Abp.BlobStorage.Localization;
using JcStack.Abp.BlobStorage.Permissions;
using JcStack.Abp.BlobStorage.StoredFiles;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;

namespace JcStack.Abp.BlobStorage.Lookup;

/// <summary>
/// 文件存储 Lookup 服务
/// 提供独立的查找权限，用于其他模块引用文件
/// </summary>
[Authorize(BlobStoragePermissions.Files.Lookup)]
public class FileStorageLookupAppService : ApplicationService, IFileStorageLookupAppService
{
    private readonly IStoredFileRepository _storedFileRepository;

    public FileStorageLookupAppService(IStoredFileRepository storedFileRepository)
    {
        LocalizationResource = typeof(BlobStorageResource);
        _storedFileRepository = storedFileRepository;
    }

    public async Task<StoredFileLookupDto?> GetAsync(Guid id)
    {
        var storedFile = await _storedFileRepository.FindAsync(id);
        if (storedFile == null)
        {
            return null;
        }

        return new StoredFileLookupDto
        {
            Id = storedFile.Id,
            FileName = storedFile.FileName,
            ContentType = storedFile.ContentType,
            FileSize = storedFile.FileSize
        };
    }
}
