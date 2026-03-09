using System;

namespace JcStack.Abp.BlobStorage.StoredFiles;

/// <summary>
/// 文件下载结果
/// </summary>
[Serializable]
public class FileDownloadResult
{
    /// <summary>
    /// 文件名
    /// </summary>
    public string FileName { get; set; } = null!;

    /// <summary>
    /// MIME 类型
    /// </summary>
    public string ContentType { get; set; } = null!;

    /// <summary>
    /// 文件内容
    /// </summary>
    public byte[] Content { get; set; } = null!;
}
