using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace JcStack.Abp.BlobStorage.StoredFiles;

/// <summary>
/// 存储文件聚合根 - 文件元数据
/// </summary>
public class StoredFile : AuditedAggregateRoot<Guid>, IMultiTenant
{
    /// <summary>
    /// 租户ID
    /// </summary>
    public virtual Guid? TenantId { get; protected set; }

    /// <summary>
    /// 原始文件名
    /// </summary>
    public virtual string FileName { get; private set; } = null!;

    /// <summary>
    /// MIME 类型
    /// </summary>
    public virtual string ContentType { get; private set; } = null!;

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public virtual long FileSize { get; private set; }

    /// <summary>
    /// Blob 容器名称
    /// </summary>
    public virtual string BlobContainerName { get; private set; } = null!;

    /// <summary>
    /// Blob 存储名称（用于定位文件）
    /// </summary>
    public virtual string BlobName { get; private set; } = null!;

    /// <summary>
    /// SHA256 校验和（可选）
    /// </summary>
    public virtual string? Checksum { get; private set; }

    /// <summary>
    /// ORM 构造函数
    /// </summary>
    protected StoredFile()
    {
    }

    /// <summary>
    /// 业务构造函数
    /// </summary>
    /// <param name="id">文件ID</param>
    /// <param name="fileName">原始文件名</param>
    /// <param name="contentType">MIME 类型</param>
    /// <param name="fileSize">文件大小</param>
    /// <param name="blobContainerName">Blob 容器名称</param>
    /// <param name="blobName">Blob 存储名称</param>
    /// <param name="checksum">校验和（可选）</param>
    public StoredFile(
        Guid id,
        string fileName,
        string contentType,
        long fileSize,
        string blobContainerName,
        string blobName,
        string? checksum = null)
        : base(id)
    {
        SetFileName(fileName);
        SetContentType(contentType);
        SetFileSize(fileSize);
        SetBlobContainerName(blobContainerName);
        SetBlobName(blobName);
        Checksum = checksum;
    }

    /// <summary>
    /// 设置文件名
    /// </summary>
    public void SetFileName(string fileName)
    {
        FileName = Check.NotNullOrWhiteSpace(
            fileName,
            nameof(fileName),
            StoredFileConsts.MaxFileNameLength);
    }

    /// <summary>
    /// 设置 ContentType
    /// </summary>
    public void SetContentType(string contentType)
    {
        ContentType = Check.NotNullOrWhiteSpace(
            contentType,
            nameof(contentType),
            StoredFileConsts.MaxContentTypeLength);
    }

    /// <summary>
    /// 设置文件大小
    /// </summary>
    public void SetFileSize(long fileSize)
    {
        if (fileSize < 0)
        {
            throw new ArgumentException("File size cannot be negative", nameof(fileSize));
        }
        FileSize = fileSize;
    }

    /// <summary>
    /// 设置 Blob 容器名称
    /// </summary>
    public void SetBlobContainerName(string blobContainerName)
    {
        BlobContainerName = Check.NotNullOrWhiteSpace(
            blobContainerName,
            nameof(blobContainerName),
            StoredFileConsts.MaxBlobContainerNameLength);
    }

    /// <summary>
    /// 设置 Blob 存储名称
    /// </summary>
    public void SetBlobName(string blobName)
    {
        BlobName = Check.NotNullOrWhiteSpace(
            blobName,
            nameof(blobName),
            StoredFileConsts.MaxBlobNameLength);
    }

    /// <summary>
    /// 设置校验和
    /// </summary>
    public void SetChecksum(string? checksum)
    {
        if (!string.IsNullOrEmpty(checksum))
        {
            Check.Length(checksum, nameof(checksum), StoredFileConsts.MaxChecksumLength);
        }
        Checksum = checksum;
    }
}
