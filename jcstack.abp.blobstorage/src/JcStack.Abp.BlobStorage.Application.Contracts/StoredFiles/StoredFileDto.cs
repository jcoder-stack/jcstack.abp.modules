using System;
using Volo.Abp.Application.Dtos;

namespace JcStack.Abp.BlobStorage.StoredFiles;

/// <summary>
/// 存储文件 DTO
/// </summary>
[Serializable]
public class StoredFileDto : AuditedEntityDto<Guid>
{
    /// <summary>
    /// 原始文件名
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// MIME 类型
    /// </summary>
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// SHA256 校验和
    /// </summary>
    public string? Checksum { get; set; }

    /// <summary>
    /// 下载 URL（由应用层填充）
    /// </summary>
    public string? DownloadUrl { get; set; }
}
