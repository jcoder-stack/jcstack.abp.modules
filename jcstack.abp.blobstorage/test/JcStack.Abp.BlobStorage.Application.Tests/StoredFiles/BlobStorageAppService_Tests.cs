using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JcStack.Abp.BlobStorage.StoredFiles;
using Shouldly;
using Volo.Abp.Content;
using Volo.Abp.Modularity;
using Xunit;

namespace JcStack.Abp.BlobStorage.StoredFiles;

/// <summary>
/// BlobStorageAppService 抽象测试类
/// 在 EntityFrameworkCore.Tests 中继承并指定具体的 TestModule
/// </summary>
public abstract class BlobStorageAppService_Tests<TStartupModule> : FileStorageApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IBlobStorageAppService _appService;
    private readonly IStoredFileRepository _repository;

    protected BlobStorageAppService_Tests()
    {
        _appService = GetRequiredService<IBlobStorageAppService>();
        _repository = GetRequiredService<IStoredFileRepository>();
    }

    #region Get Tests

    [Fact]
    public async Task Should_Get_StoredFile_By_Id()
    {
        // Act
        var result = await _appService.GetAsync(BlobStorageTestConsts.StoredFile1Id);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(BlobStorageTestConsts.StoredFile1Id);
        result.FileName.ShouldBe(BlobStorageTestConsts.StoredFile1FileName);
        result.ContentType.ShouldBe(BlobStorageTestConsts.StoredFile1ContentType);
        result.FileSize.ShouldBe(BlobStorageTestConsts.StoredFile1FileSize);
    }

    [Fact]
    public async Task Should_Throw_When_StoredFile_Not_Found()
    {
        // Act & Assert
        await Should.ThrowAsync<Exception>(async () =>
        {
            await _appService.GetAsync(BlobStorageTestConsts.NonExistentFileId);
        });
    }

    #endregion

    #region GetList Tests

    [Fact]
    public async Task Should_Get_StoredFile_List()
    {
        // Arrange
        var input = new GetStoredFileListInput
        {
            MaxResultCount = 10,
            SkipCount = 0
        };

        // Act
        var result = await _appService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(3);
        result.Items.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task Should_Get_StoredFile_List_With_Filter()
    {
        // Arrange
        var input = new GetStoredFileListInput
        {
            Filter = "pdf",
            MaxResultCount = 10,
            SkipCount = 0
        };

        // Act
        var result = await _appService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldNotBeEmpty();
        result.Items.ShouldAllBe(x =>
            x.FileName.Contains("pdf", StringComparison.OrdinalIgnoreCase) ||
            x.ContentType.Contains("pdf", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Should_Get_StoredFile_List_With_Pagination()
    {
        // Arrange
        var input = new GetStoredFileListInput
        {
            MaxResultCount = 2,
            SkipCount = 0
        };

        // Act
        var result = await _appService.GetListAsync(input);

        // Assert
        result.ShouldNotBeNull();
        result.TotalCount.ShouldBeGreaterThanOrEqualTo(3);
        result.Items.Count.ShouldBeLessThanOrEqualTo(2);
    }

    #endregion

    #region Upload Tests

    [Fact]
    public async Task Should_Upload_File_Successfully()
    {
        // Arrange
        var fileContent = Encoding.UTF8.GetBytes("Test file content for upload test.");
        var memoryStream = new MemoryStream(fileContent);
        var remoteStreamContent = new RemoteStreamContent(
            memoryStream,
            "test-upload.txt",
            "text/plain",
            fileContent.Length);

        // Act
        var result = await _appService.UploadAsync(remoteStreamContent);

        // Assert
        result.ShouldNotBeNull();
        result.FileName.ShouldBe("test-upload.txt");
        result.ContentType.ShouldBe("text/plain");
        result.FileSize.ShouldBe(fileContent.Length);
    }

    [Fact]
    public async Task Should_Upload_Pdf_File_Successfully()
    {
        // Arrange
        var fileContent = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF magic bytes
        var memoryStream = new MemoryStream(fileContent);
        var remoteStreamContent = new RemoteStreamContent(
            memoryStream,
            "document.pdf",
            "application/pdf",
            fileContent.Length);

        // Act
        var result = await _appService.UploadAsync(remoteStreamContent);

        // Assert
        result.ShouldNotBeNull();
        result.FileName.ShouldBe("document.pdf");
        result.ContentType.ShouldBe("application/pdf");
    }

    #endregion

    #region Download Tests

    [Fact]
    public async Task Should_Download_File_Successfully()
    {
        // Arrange - 先上传文件
        var fileContent = Encoding.UTF8.GetBytes("Content for download test.");
        var memoryStream = new MemoryStream(fileContent);
        var remoteStreamContent = new RemoteStreamContent(
            memoryStream,
            "download-test.txt",
            "text/plain",
            fileContent.Length);

        var uploadResult = await _appService.UploadAsync(remoteStreamContent);

        // Act
        var downloadResult = await _appService.DownloadAsync(uploadResult.Id);

        // Assert
        downloadResult.ShouldNotBeNull();
        downloadResult.FileName.ShouldBe("download-test.txt");
        downloadResult.ContentType.ShouldBe("text/plain");

        // 验证内容
        using var resultStream = downloadResult.GetStream();
        using var reader = new StreamReader(resultStream);
        var content = await reader.ReadToEndAsync();
        content.ShouldBe("Content for download test.");
    }

    #endregion

    #region Delete Tests

    [Fact]
    public async Task Should_Delete_File_Successfully()
    {
        // Arrange - 先上传文件
        var fileContent = Encoding.UTF8.GetBytes("Content for delete test.");
        var memoryStream = new MemoryStream(fileContent);
        var remoteStreamContent = new RemoteStreamContent(
            memoryStream,
            "delete-test.txt",
            "text/plain",
            fileContent.Length);

        var uploadResult = await _appService.UploadAsync(remoteStreamContent);

        // 确认文件存在
        var existsBefore = await _repository.FindAsync(uploadResult.Id);
        existsBefore.ShouldNotBeNull();

        // Act
        await _appService.DeleteAsync(uploadResult.Id);

        // Assert - 验证文件已删除
        var existsAfter = await _repository.FindAsync(uploadResult.Id);
        existsAfter.ShouldBeNull();
    }

    #endregion

    #region Full Workflow Test

    [Fact]
    public async Task Should_Complete_Full_File_Lifecycle()
    {
        // 1. Upload
        var content = "Full lifecycle test content.";
        var fileContent = Encoding.UTF8.GetBytes(content);
        var memoryStream = new MemoryStream(fileContent);
        var uploadInput = new RemoteStreamContent(
            memoryStream,
            "lifecycle-test.txt",
            "text/plain",
            fileContent.Length);

        var uploaded = await _appService.UploadAsync(uploadInput);
        uploaded.Id.ShouldNotBe(Guid.Empty);

        // 2. Get metadata
        var metadata = await _appService.GetAsync(uploaded.Id);
        metadata.FileName.ShouldBe("lifecycle-test.txt");

        // 3. Download and verify content
        var downloaded = await _appService.DownloadAsync(uploaded.Id);
        using var stream = downloaded.GetStream();
        using var reader = new StreamReader(stream);
        var downloadedContent = await reader.ReadToEndAsync();
        downloadedContent.ShouldBe(content);

        // 4. List should include this file
        var listResult = await _appService.GetListAsync(new GetStoredFileListInput
        {
            Filter = "lifecycle-test",
            MaxResultCount = 10,
            SkipCount = 0
        });
        listResult.Items.ShouldContain(x => x.Id == uploaded.Id);

        // 5. Delete
        await _appService.DeleteAsync(uploaded.Id);

        // 6. Verify deleted
        await Should.ThrowAsync<Exception>(async () =>
        {
            await _appService.GetAsync(uploaded.Id);
        });
    }

    #endregion
}
