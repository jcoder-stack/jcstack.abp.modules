using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace JcStack.Abp.Excel;

/// <summary>
/// Excel 导出服务抽象接口。
/// 业务模块仅依赖此接口，具体实现由基础设施层注册。
/// </summary>
public interface IExcelExporter
{
    /// <summary>
    /// 单 Sheet 导出
    /// </summary>
    /// <typeparam name="T">数据类型，可用 <see cref="ExcelColumnAttribute"/> 配置列名和顺序</typeparam>
    /// <param name="data">数据集合</param>
    /// <param name="options">导出选项（可选）</param>
    /// <returns>包含 xlsx 内容的 MemoryStream（Position 已归零）</returns>
    Task<MemoryStream> ExportAsync<T>(
        IEnumerable<T> data,
        ExcelExportOptions? options = null) where T : class;

    /// <summary>
    /// 多 Sheet 导出
    /// </summary>
    /// <param name="sheets">Sheet 名称 → 数据集合的字典。值应为 IEnumerable&lt;T&gt; 类型。</param>
    /// <param name="options">导出选项（可选）</param>
    /// <returns>包含 xlsx 内容的 MemoryStream（Position 已归零）</returns>
    Task<MemoryStream> ExportMultiSheetAsync(
        Dictionary<string, object> sheets,
        ExcelExportOptions? options = null);
}
