namespace JcStack.Abp.Excel;

/// <summary>
/// Excel 导出选项
/// </summary>
public class ExcelExportOptions
{
    /// <summary>
    /// Sheet 名称（单 Sheet 导出时使用，默认 "Sheet1"）
    /// </summary>
    public string SheetName { get; set; } = "Sheet1";

    /// <summary>
    /// 导出文件名（不含扩展名，默认 "export"）
    /// </summary>
    public string FileName { get; set; } = "export";
}
