namespace JcStack.Abp.BlobStorage;

/// <summary>
/// FileStorage 模块错误码
/// </summary>
 public static class BlobStorageErrorCodes
{
    /// <summary>
    /// 文件不存在
    /// </summary>
    public const string FileNotFound = "FileStorage:FileNotFound";

    /// <summary>
    /// 文件大小超出限制
    /// </summary>
    public const string FileSizeExceeded = "FileStorage:FileSizeExceeded";

    /// <summary>
    /// 不允许的文件类型
    /// </summary>
    public const string FileTypeNotAllowed = "FileStorage:FileTypeNotAllowed";

    /// <summary>
    /// 文件上传失败
    /// </summary>
    public const string FileUploadFailed = "FileStorage:FileUploadFailed";

    /// <summary>
    /// 文件名无效
    /// </summary>
    public const string InvalidFileName = "FileStorage:InvalidFileName";
}
