using Volo.Abp.MailKit;
using Volo.Abp.Modularity;
using Volo.Abp.TextTemplating.Scriban;
using Volo.Abp.VirtualFileSystem;

namespace JcStack.Abp.Message.Emailing;

[DependsOn(
    typeof(MessageApplicationModule),
    typeof(AbpMailKitModule),
    typeof(AbpTextTemplatingScribanModule))]
public class MessageEmailingModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<MessageEmailingModule>();
        });
    }
}
