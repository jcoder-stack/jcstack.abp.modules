using System;
using Volo.Abp.Application.Dtos;

namespace JcStack.Abp.BlobStorage.StoredFiles;

/// <summary>
/// 获取文件列表输入参数
/// </summary>
[Serializable]
public class GetStoredFileListInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 文件名过滤
    /// </summary>
    public string? Filter { get; set; }
}
