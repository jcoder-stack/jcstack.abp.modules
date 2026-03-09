# JcStack.Abp.BlobStorage 模块

## 概述

`JcStack.Abp.BlobStorage` 是一个**通用的 ABP Framework 文件存储模块**，封装 ABP Blob Storing，提供文件元数据管理、统一上传下载 API。可被任意 ABP 项目复用。

## 特性

- ✅ **元数据管理**：存储文件名、大小、类型、校验和等元数据
- ✅ **多租户支持**：原生支持 ABP 多租户架构
- ✅ **可插拔存储**：基于 ABP Blob Storing，支持 FileSystem/MinIO/OSS/S3
- ✅ **文件校验**：支持 SHA256 校验和、文件大小/类型限制
- ✅ **权限控制**：完整的上传/下载/删除权限体系
- ✅ **RESTful API**：标准化的文件上传下载接口

## 安装

### 1. 添加项目引用

在你的 ABP 应用模块中添加依赖：

```csharp
[DependsOn(
    typeof(FileStorageApplicationModule),      // Application 层
    typeof(FileStorageEntityFrameworkCoreModule), // EF Core 层
    typeof(FileStorageHttpApiModule)           // HTTP API 层
)]
public class YourApplicationModule : AbpModule
{
}
```

### 2. 配置数据库

在 `DbContext` 中添加 FileStorage 表映射：

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    
    builder.ConfigureFileStorage();
}
```

### 3. 配置 Blob Storing

在 `appsettings.json` 或模块配置中配置 Blob 存储：

```csharp
// 使用文件系统存储（开发环境）
Configure<AbpBlobStoringOptions>(options =>
{
    options.Containers.Configure<FileStorageBlobContainer>(container =>
    {
        container.UseFileSystem(fileSystem =>
        {
            fileSystem.BasePath = "D:/FileStorage";
        });
    });
});
```

### 4. 配置选项（可选）

```csharp
Configure<FileStorageOptions>(options =>
{
    options.MaxFileSize = 52428800; // 50MB
    options.AllowedExtensions = [".jpg", ".png", ".pdf"];
    options.CalculateChecksum = true;
});
```

或在 `appsettings.json` 中：

```json
{
  "FileStorage": {
    "MaxFileSize": 52428800,
    "AllowedExtensions": [".jpg", ".png", ".pdf", ".docx"]
  }
}
```

## API 接口

| 方法 | 路径 | 描述 |
|------|------|------|
| POST | `/api/file-storage/upload` | 上传文件 |
| GET | `/api/file-storage/{id}` | 获取文件元数据 |
| GET | `/api/file-storage/{id}/download` | 下载文件 |
| GET | `/api/file-storage` | 获取文件列表 |
| DELETE | `/api/file-storage/{id}` | 删除文件 |

## 权限

| 权限 | 描述 |
|------|------|
| `FileStorage.Files` | 文件管理基础权限 |
| `FileStorage.Files.Upload` | 上传文件 |
| `FileStorage.Files.Download` | 下载文件 |
| `FileStorage.Files.Delete` | 删除文件 |

## 与业务模块集成

业务模块通过 `FileId` 引用存储文件：

```csharp
// 在业务实体中引用文件
public class AfterSalesAttachment : Entity<Guid>
{
    public Guid FileId { get; private set; } // 引用 StoredFile.Id
}

// 上传文件
var fileDto = await _fileStorageAppService.UploadAsync(fileStream);
var attachment = new AfterSalesAttachment(id, fileDto.Id, ...);

// 下载文件
var content = await _fileStorageAppService.DownloadAsync(attachment.FileId);
```

## 模块结构

```
JcStack.Abp.BlobStorage/
├── src/
│   ├── JcStack.Abp.BlobStorage.Domain.Shared/       # 常量、枚举、本地化
│   ├── JcStack.Abp.BlobStorage.Domain/              # StoredFile 聚合根、仓储接口
│   ├── JcStack.Abp.BlobStorage.Application.Contracts/ # DTOs、服务接口
│   ├── JcStack.Abp.BlobStorage.Application/         # 应用服务实现
│   ├── JcStack.Abp.BlobStorage.EntityFrameworkCore/ # EF Core 仓储实现
│   ├── JcStack.Abp.BlobStorage.HttpApi/             # API 控制器
│   ├── JcStack.Abp.BlobStorage.HttpApi.Client/      # HTTP 客户端代理
│   └── JcStack.Abp.BlobStorage.Installer/           # 模块安装器
└── test/
    ├── JcStack.Abp.BlobStorage.TestBase/
    ├── JcStack.Abp.BlobStorage.Domain.Tests/
    └── JcStack.Abp.BlobStorage.Application.Tests/
```

## 技术栈

- .NET 10
- ABP Framework 10.0.2
- ABP Blob Storing

## License

MIT
