namespace JcStack.Abp.BlobStorage;

/// <summary>
/// 存储文件常量定义
/// </summary>
public static class StoredFileConsts
{
    /// <summary>
    /// 文件名最大长度
    /// </summary>
    public const int MaxFileNameLength = 256;

    /// <summary>
    /// ContentType 最大长度
    /// </summary>
    public const int MaxContentTypeLength = 128;

    /// <summary>
    /// BlobContainerName 最大长度
    /// </summary>
    public const int MaxBlobContainerNameLength = 128;

    /// <summary>
    /// BlobName 最大长度
    /// </summary>
    public const int MaxBlobNameLength = 512;

    /// <summary>
    /// Checksum 最大长度 (SHA256 = 64 chars hex)
    /// </summary>
    public const int MaxChecksumLength = 64;

    /// <summary>
    /// 默认最大文件大小 (50MB)
    /// </summary>
    public const long DefaultMaxFileSize = 52428800;

    /// <summary>
    /// 默认 Blob 容器名称
    /// </summary>
    public const string DefaultBlobContainerName = "file-storage";
}
