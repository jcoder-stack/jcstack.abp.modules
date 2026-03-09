using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using MiniExcelLibs;
using MiniExcelLibs.OpenXml;
using Volo.Abp.DependencyInjection;

namespace JcStack.Abp.Excel.MiniExcel;

/// <summary>
/// 基于 MiniExcel 的 Excel 导出实现。
/// internal 类型：业务代码无法直接引用，只能通过 <see cref="IExcelExporter"/> 接口使用。
/// </summary>
internal sealed class MiniExcelExporter : IExcelExporter, ITransientDependency
{
    public Task<MemoryStream> ExportAsync<T>(
        IEnumerable<T> data,
        ExcelExportOptions? options = null) where T : class
    {
        options ??= new ExcelExportOptions();
        var mappedData = MapToExcelData(data);

        var stream = new MemoryStream();
        stream.SaveAs(mappedData, sheetName: options.SheetName);
        stream.Position = 0;

        return Task.FromResult(stream);
    }

    public Task<MemoryStream> ExportMultiSheetAsync(
        Dictionary<string, object> sheets,
        ExcelExportOptions? options = null)
    {
        var sheetData = new Dictionary<string, object>();

        foreach (var (sheetName, data) in sheets)
        {
            if (data is System.Collections.IEnumerable enumerable)
            {
                sheetData[sheetName] = MapToExcelDataUntyped(enumerable);
            }
            else
            {
                sheetData[sheetName] = data;
            }
        }

        var stream = new MemoryStream();
        stream.SaveAs(sheetData);
        stream.Position = 0;

        return Task.FromResult(stream);
    }

    /// <summary>
    /// 将强类型集合转换为 MiniExcel 可识别的字典列表，
    /// 根据 <see cref="ExcelColumnAttribute"/> 生成中文列名并按 Order 排序。
    /// </summary>
    private static IEnumerable<Dictionary<string, object?>> MapToExcelData<T>(IEnumerable<T> data) where T : class
    {
        var columns = GetColumnMappings(typeof(T));
        foreach (var item in data)
        {
            var row = new Dictionary<string, object?>();
            foreach (var col in columns)
            {
                var value = col.Property.GetValue(item);
                if (col.Format != null && value is IFormattable formattable)
                {
                    row[col.DisplayName] = formattable.ToString(col.Format, null);
                }
                else
                {
                    row[col.DisplayName] = value;
                }
            }
            yield return row;
        }
    }

    /// <summary>
    /// 非泛型版本，用于多 Sheet 导出
    /// </summary>
    private static IEnumerable<Dictionary<string, object?>> MapToExcelDataUntyped(System.Collections.IEnumerable data)
    {
        Type? elementType = null;
        List<ColumnMapping>? columns = null;

        foreach (var item in data)
        {
            if (item == null) continue;

            if (columns == null)
            {
                elementType = item.GetType();
                columns = GetColumnMappings(elementType);
            }

            var row = new Dictionary<string, object?>();
            foreach (var col in columns)
            {
                var value = col.Property.GetValue(item);
                if (col.Format != null && value is IFormattable formattable)
                {
                    row[col.DisplayName] = formattable.ToString(col.Format, null);
                }
                else
                {
                    row[col.DisplayName] = value;
                }
            }
            yield return row;
        }
    }

    private static List<ColumnMapping> GetColumnMappings(Type type)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var mappings = new List<ColumnMapping>();

        foreach (var prop in properties)
        {
            var attr = prop.GetCustomAttribute<ExcelColumnAttribute>();
            if (attr is { Ignore: true })
                continue;

            mappings.Add(new ColumnMapping
            {
                Property = prop,
                DisplayName = attr?.Name ?? prop.Name,
                Order = attr?.Order ?? int.MaxValue,
                Format = attr?.Format,
            });
        }

        return mappings.OrderBy(x => x.Order).ToList();
    }

    private sealed class ColumnMapping
    {
        public PropertyInfo Property { get; init; } = null!;
        public string DisplayName { get; init; } = null!;
        public int Order { get; init; }
        public string? Format { get; init; }
    }
}
