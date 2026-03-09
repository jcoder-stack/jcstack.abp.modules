using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Excel.MiniExcel;

/// <summary>
/// MiniExcel 实现模块。
/// 仅在组合根（Host 层）引用此模块，业务模块只引用 <see cref="AbpExcelModule"/>。
/// </summary>
[DependsOn(typeof(AbpExcelModule))]
public class AbpExcelMiniExcelModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // MiniExcelExporter 是 internal + ITransientDependency，
        // ABP 自动注册需要显式注册 internal 类型
        context.Services.AddTransient<IExcelExporter, MiniExcelExporter>();
    }
}
