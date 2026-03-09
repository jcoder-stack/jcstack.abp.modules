using System;

namespace JcStack.Abp.Excel;

/// <summary>
/// 标记 DTO 属性的 Excel 列配置。
/// 用于定义列显示名称、顺序和格式。
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ExcelColumnAttribute : Attribute
{
    /// <summary>
    /// 列显示名称（表头）
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 列顺序（从小到大排列，未标记的属性排在最后）
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 列宽度（字符数，0 表示自动）
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// 日期/数字格式字符串（如 "yyyy-MM-dd HH:mm"、"0.00%"）
    /// </summary>
    public string? Format { get; set; }

    /// <summary>
    /// 是否忽略该列（不导出）
    /// </summary>
    public bool Ignore { get; set; }

    public ExcelColumnAttribute(string name)
    {
        Name = name;
    }
}
