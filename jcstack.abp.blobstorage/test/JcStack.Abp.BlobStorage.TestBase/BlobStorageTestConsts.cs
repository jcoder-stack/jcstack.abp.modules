using System;

namespace JcStack.Abp.BlobStorage;

/// <summary>
/// FileStorage 测试常量
/// </summary>
public static class BlobStorageTestConsts
{
    // ==================== 存储文件测试数据 ====================

    public static readonly Guid StoredFile1Id = Guid.Parse("1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d");
    public const string StoredFile1FileName = "test-document.pdf";
    public const string StoredFile1ContentType = "application/pdf";
    public const long StoredFile1FileSize = 102400; // 100KB
    public const string StoredFile1BlobName = "host/2026/01/1a2b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d.pdf";
    public const string StoredFile1Checksum = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

    public static readonly Guid StoredFile2Id = Guid.Parse("2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e");
    public const string StoredFile2FileName = "test-image.jpg";
    public const string StoredFile2ContentType = "image/jpeg";
    public const long StoredFile2FileSize = 204800; // 200KB
    public const string StoredFile2BlobName = "host/2026/01/2b3c4d5e-6f7a-8b9c-0d1e-2f3a4b5c6d7e.jpg";

    public static readonly Guid StoredFile3Id = Guid.Parse("3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f");
    public const string StoredFile3FileName = "test-spreadsheet.xlsx";
    public const string StoredFile3ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    public const long StoredFile3FileSize = 51200; // 50KB
    public const string StoredFile3BlobName = "host/2026/01/3c4d5e6f-7a8b-9c0d-1e2f-3a4b5c6d7e8f.xlsx";

    // ==================== 测试用的不存在 ID ====================

    public static readonly Guid NonExistentFileId = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");

    // ==================== 测试文件内容 ====================

    public static readonly byte[] TestFileContent = "This is test file content for unit testing."u8.ToArray();

    // ==================== 默认配置值 ====================

    public const string DefaultBlobContainerName = "file-storage";
}
