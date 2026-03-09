using System;
using Volo.Abp.Application.Dtos;

namespace JcStack.Abp.BlobStorage.StoredFiles;

/// <summary>
/// 存储文件 Lookup DTO（用于下拉选择）
/// </summary>
[Serializable]
public class StoredFileLookupDto : EntityDto<Guid>
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
}
