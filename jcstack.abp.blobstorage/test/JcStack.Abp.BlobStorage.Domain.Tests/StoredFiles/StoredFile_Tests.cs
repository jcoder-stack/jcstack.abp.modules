using System;
using Shouldly;
using Xunit;
using JcStack.Abp.BlobStorage.StoredFiles;

namespace JcStack.Abp.BlobStorage.StoredFiles;

/// <summary>
/// StoredFile 聚合根单元测试
/// </summary>
public class StoredFile_Tests
{
    [Fact]
    public void Should_Create_StoredFile_With_Valid_Parameters()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fileName = "test-document.pdf";
        var contentType = "application/pdf";
        var fileSize = 1024L;
        var blobContainerName = "file-storage";
        var blobName = "host/2025/01/abc123.pdf";
        var checksum = "a".PadLeft(64, 'a');

        // Act
        var storedFile = new StoredFile(
            id,
            fileName,
            contentType,
            fileSize,
            blobContainerName,
            blobName,
            checksum);

        // Assert
        storedFile.Id.ShouldBe(id);
        storedFile.FileName.ShouldBe(fileName);
        storedFile.ContentType.ShouldBe(contentType);
        storedFile.FileSize.ShouldBe(fileSize);
        storedFile.BlobContainerName.ShouldBe(blobContainerName);
        storedFile.BlobName.ShouldBe(blobName);
        storedFile.Checksum.ShouldBe(checksum);
    }

    [Fact]
    public void Should_Create_StoredFile_Without_Checksum()
    {
        // Arrange & Act
        var storedFile = new StoredFile(
            Guid.NewGuid(),
            "test.txt",
            "text/plain",
            100L,
            "container",
            "blob-name");

        // Assert
        storedFile.Checksum.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_FileName_Is_NullOrWhiteSpace(string? fileName)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new StoredFile(
            Guid.NewGuid(),
            fileName!,
            "text/plain",
            100L,
            "container",
            "blob-name"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_ContentType_Is_NullOrWhiteSpace(string? contentType)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new StoredFile(
            Guid.NewGuid(),
            "test.txt",
            contentType!,
            100L,
            "container",
            "blob-name"));
    }

    [Fact]
    public void Should_Throw_When_FileSize_Is_Negative()
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new StoredFile(
            Guid.NewGuid(),
            "test.txt",
            "text/plain",
            -1L,
            "container",
            "blob-name"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_BlobContainerName_Is_NullOrWhiteSpace(string? blobContainerName)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new StoredFile(
            Guid.NewGuid(),
            "test.txt",
            "text/plain",
            100L,
            blobContainerName!,
            "blob-name"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Should_Throw_When_BlobName_Is_NullOrWhiteSpace(string? blobName)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() => new StoredFile(
            Guid.NewGuid(),
            "test.txt",
            "text/plain",
            100L,
            "container",
            blobName!));
    }

    [Fact]
    public void Should_Accept_Zero_FileSize()
    {
        // Act
        var storedFile = new StoredFile(
            Guid.NewGuid(),
            "empty.txt",
            "text/plain",
            0L,
            "container",
            "blob-name");

        // Assert
        storedFile.FileSize.ShouldBe(0L);
    }

    [Fact]
    public void SetFileName_Should_Update_FileName()
    {
        // Arrange
        var storedFile = CreateTestStoredFile();
        var newFileName = "new-name.pdf";

        // Act
        storedFile.SetFileName(newFileName);

        // Assert
        storedFile.FileName.ShouldBe(newFileName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetFileName_Should_Throw_When_Value_Is_Invalid(string? fileName)
    {
        // Arrange
        var storedFile = CreateTestStoredFile();

        // Act & Assert
        Should.Throw<ArgumentException>(() => storedFile.SetFileName(fileName!));
    }

    [Fact]
    public void SetContentType_Should_Update_ContentType()
    {
        // Arrange
        var storedFile = CreateTestStoredFile();
        var newContentType = "application/json";

        // Act
        storedFile.SetContentType(newContentType);

        // Assert
        storedFile.ContentType.ShouldBe(newContentType);
    }

    [Fact]
    public void SetFileSize_Should_Update_FileSize()
    {
        // Arrange
        var storedFile = CreateTestStoredFile();
        var newFileSize = 2048L;

        // Act
        storedFile.SetFileSize(newFileSize);

        // Assert
        storedFile.FileSize.ShouldBe(newFileSize);
    }

    [Fact]
    public void SetFileSize_Should_Throw_When_Negative()
    {
        // Arrange
        var storedFile = CreateTestStoredFile();

        // Act & Assert
        Should.Throw<ArgumentException>(() => storedFile.SetFileSize(-1));
    }

    [Fact]
    public void SetBlobContainerName_Should_Update_BlobContainerName()
    {
        // Arrange
        var storedFile = CreateTestStoredFile();
        var newContainerName = "new-container";

        // Act
        storedFile.SetBlobContainerName(newContainerName);

        // Assert
        storedFile.BlobContainerName.ShouldBe(newContainerName);
    }

    [Fact]
    public void SetBlobName_Should_Update_BlobName()
    {
        // Arrange
        var storedFile = CreateTestStoredFile();
        var newBlobName = "new/blob/name.pdf";

        // Act
        storedFile.SetBlobName(newBlobName);

        // Assert
        storedFile.BlobName.ShouldBe(newBlobName);
    }

    [Fact]
    public void SetChecksum_Should_Update_Checksum()
    {
        // Arrange
        var storedFile = CreateTestStoredFile();
        var newChecksum = "b".PadLeft(64, 'b');

        // Act
        storedFile.SetChecksum(newChecksum);

        // Assert
        storedFile.Checksum.ShouldBe(newChecksum);
    }

    [Fact]
    public void SetChecksum_Should_Accept_Null()
    {
        // Arrange
        var storedFile = CreateTestStoredFile();

        // Act
        storedFile.SetChecksum(null);

        // Assert
        storedFile.Checksum.ShouldBeNull();
    }

    [Fact]
    public void SetChecksum_Should_Accept_Empty_String()
    {
        // Arrange
        var storedFile = CreateTestStoredFile();

        // Act
        storedFile.SetChecksum(string.Empty);

        // Assert
        storedFile.Checksum.ShouldBeEmpty();
    }

    private static StoredFile CreateTestStoredFile()
    {
        return new StoredFile(
            Guid.NewGuid(),
            "test.txt",
            "text/plain",
            1024L,
            "container",
            "blob-name",
            "a".PadLeft(64, 'a'));
    }
}
