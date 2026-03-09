using System.Collections.Generic;

namespace JcStack.Abp.BlobStorage;

/// <summary>
/// 文件存储配置选项
/// </summary>
public class BlobStorageOptions
{
    /// <summary>
    /// 最大文件大小（字节），默认 50MB
    /// </summary>
    public long MaxFileSize { get; set; } = StoredFileConsts.DefaultMaxFileSize;

    /// <summary>
    /// 允许的文件扩展名
    /// </summary>
    public List<string> AllowedExtensions { get; set; } =
    [
        ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".txt", ".csv", ".json", ".xml",
        ".zip", ".rar", ".7z"
    ];

    /// <summary>
    /// 默认 Blob 容器名称
    /// </summary>
    public string DefaultBlobContainerName { get; set; } = StoredFileConsts.DefaultBlobContainerName;

    /// <summary>
    /// 是否计算文件校验和
    /// </summary>
    public bool CalculateChecksum { get; set; } = true;
}
