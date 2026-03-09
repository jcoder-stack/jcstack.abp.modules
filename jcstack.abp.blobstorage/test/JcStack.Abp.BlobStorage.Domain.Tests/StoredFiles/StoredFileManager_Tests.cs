using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JcStack.Abp.BlobStorage.StoredFiles;
using Shouldly;
using Volo.Abp.Modularity;
using Xunit;

namespace JcStack.Abp.BlobStorage.StoredFiles;

/// <summary>
/// StoredFileManager 领域服务抽象测试类
/// 在 EntityFrameworkCore.Tests 中继承并指定具体的 TestModule
/// </summary>
public abstract class StoredFileManager_Tests<TStartupModule> : FileStorageDomainTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly StoredFileManager _manager;
    private readonly IStoredFileRepository _repository;

    protected StoredFileManager_Tests()
    {
        _manager = GetRequiredService<StoredFileManager>();
        _repository = GetRequiredService<IStoredFileRepository>();
    }

    #region CreateAsync Tests

    [Fact]
    public async Task Should_Create_StoredFile_Successfully()
    {
        // Arrange
        var fileName = "test-document.pdf";
        var contentType = "application/pdf";
        var fileSize = 1024L;
        var blobContainerName = StoredFileConsts.DefaultBlobContainerName;

        // Act
        var storedFile = await _manager.CreateAsync(
            fileName,
            contentType,
            fileSize,
            blobContainerName);

        // Assert
        storedFile.ShouldNotBeNull();
        storedFile.FileName.ShouldBe(fileName);
        storedFile.ContentType.ShouldBe(contentType);
        storedFile.FileSize.ShouldBe(fileSize);
        storedFile.BlobContainerName.ShouldBe(blobContainerName);
        storedFile.BlobName.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Create_StoredFile_With_Checksum()
    {
        // Arrange
        var checksum = "a".PadLeft(64, 'a');

        // Act
        var storedFile = await _manager.CreateAsync(
            "test.txt",
            "text/plain",
            100L,
            "container",
            checksum);

        // Assert
        storedFile.Checksum.ShouldBe(checksum);
    }

    #endregion

    #region GenerateBlobName Tests

    [Fact]
    public void GenerateBlobName_Should_Include_HostFolder_When_No_Tenant()
    {
        // Act
        var blobName = _manager.GenerateBlobName("test.pdf");

        // Assert
        blobName.ShouldStartWith("host/");
    }

    [Fact]
    public void GenerateBlobName_Should_Include_YearMonth_Folders()
    {
        // Act
        var blobName = _manager.GenerateBlobName("test.pdf");

        // Assert
        // 格式: host/yyyy/MM/guid.ext
        var pattern = @"^host/\d{4}/\d{2}/[0-9a-f-]+\.pdf$";
        Regex.IsMatch(blobName, pattern).ShouldBeTrue($"BlobName '{blobName}' does not match expected pattern");
    }

    [Fact]
    public void GenerateBlobName_Should_Preserve_Extension()
    {
        // Act
        var blobNamePdf = _manager.GenerateBlobName("document.pdf");
        var blobNameTxt = _manager.GenerateBlobName("readme.TXT");
        var blobNameJpg = _manager.GenerateBlobName("image.JPG");

        // Assert
        blobNamePdf.ShouldEndWith(".pdf");
        blobNameTxt.ShouldEndWith(".txt"); // 应该转为小写
        blobNameJpg.ShouldEndWith(".jpg"); // 应该转为小写
    }

    [Fact]
    public void GenerateBlobName_Should_Handle_File_Without_Extension()
    {
        // Act
        var blobName = _manager.GenerateBlobName("README");

        // Assert
        blobName.ShouldNotBeNullOrWhiteSpace();
        // 不应该以.结尾
        blobName.ShouldNotEndWith(".");
    }

    [Fact]
    public void GenerateBlobName_Should_Generate_Unique_Names()
    {
        // Act
        var blobName1 = _manager.GenerateBlobName("test.pdf");
        var blobName2 = _manager.GenerateBlobName("test.pdf");

        // Assert
        blobName1.ShouldNotBe(blobName2);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void GenerateBlobName_Should_Throw_When_FileName_Is_Invalid(string? fileName)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => _manager.GenerateBlobName(fileName!));
    }

    [Fact]
    public void GenerateBlobName_Should_Sanitize_Dangerous_Extension()
    {
        // Arrange - 包含非法字符的文件名
        var fileName = "test<script>.txt";

        // Act
        var blobName = _manager.GenerateBlobName(fileName);

        // Assert
        // 应该移除非法字符
        blobName.ShouldNotContain("<");
        blobName.ShouldNotContain(">");
    }

    #endregion

    #region IsExtensionAllowed Tests

    [Theory]
    [InlineData("document.pdf", new[] { ".pdf", ".doc" }, true)]
    [InlineData("document.PDF", new[] { ".pdf", ".doc" }, true)]
    [InlineData("image.jpg", new[] { "jpg", "png" }, true)]
    [InlineData("image.JPG", new[] { "jpg", "png" }, true)]
    [InlineData("file.exe", new[] { ".pdf", ".doc" }, false)]
    [InlineData("README", new[] { ".pdf", ".txt" }, false)]
    public void IsExtensionAllowed_Should_Return_Expected_Result(
        string fileName,
        string[] allowedExtensions,
        bool expected)
    {
        // Act
        var result = _manager.IsExtensionAllowed(fileName, allowedExtensions);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void IsExtensionAllowed_Should_Return_False_When_FileName_Is_Invalid(string? fileName)
    {
        // Arrange
        var allowedExtensions = new[] { ".pdf", ".txt" };

        // Act
        var result = _manager.IsExtensionAllowed(fileName!, allowedExtensions);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public void IsExtensionAllowed_Should_Return_False_When_Empty_AllowedExtensions()
    {
        // Arrange
        var allowedExtensions = Array.Empty<string>();

        // Act
        var result = _manager.IsExtensionAllowed("test.pdf", allowedExtensions);

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region IsFileSizeAllowed Tests

    [Theory]
    [InlineData(1024, 1048576, true)]        // 1KB < 1MB
    [InlineData(1048576, 1048576, true)]     // 1MB = 1MB (边界)
    [InlineData(1048577, 1048576, false)]    // 1MB + 1 > 1MB
    [InlineData(0, 1048576, false)]          // 0 字节不允许
    [InlineData(-1, 1048576, false)]         // 负数不允许
    public void IsFileSizeAllowed_Should_Return_Expected_Result(
        long fileSize,
        long maxFileSize,
        bool expected)
    {
        // Act
        var result = _manager.IsFileSizeAllowed(fileSize, maxFileSize);

        // Assert
        result.ShouldBe(expected);
    }

    #endregion

    #region Repository Integration Tests

    [Fact]
    public async Task Should_Find_StoredFile_By_BlobName()
    {
        // Arrange
        var storedFile = await _manager.CreateAsync(
            "find-by-blob.txt",
            "text/plain",
            512L,
            StoredFileConsts.DefaultBlobContainerName);

        // Act
        var found = await _repository.FindByBlobNameAsync(storedFile.BlobName);

        // Assert
        found.ShouldNotBeNull();
        found.Id.ShouldBe(storedFile.Id);
    }

    [Fact]
    public async Task Should_Get_StoredFile_List()
    {
        // Arrange
        await _manager.CreateAsync("list-test-1.pdf", "application/pdf", 100L, "container");
        await _manager.CreateAsync("list-test-2.pdf", "application/pdf", 200L, "container");

        // Act
        var list = await _repository.GetListAsync();

        // Assert
        list.ShouldNotBeEmpty();
        list.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    #endregion
}
