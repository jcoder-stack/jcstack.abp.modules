using JcStack.Abp.Message.EntityFrameworkCore;
using JcStack.Abp.Message.Notifications;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Message;

[DependsOn(
    typeof(MessageDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class MessageEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<MessageDbContext>(options =>
        {
            options.AddDefaultRepositories<IMessageDbContext>(includeAllEntities: true);

            options.AddRepository<Notification, EfCoreNotificationRepository>();
            options.AddRepository<UserNotification, EfCoreUserNotificationRepository>();
        });
    }
}
