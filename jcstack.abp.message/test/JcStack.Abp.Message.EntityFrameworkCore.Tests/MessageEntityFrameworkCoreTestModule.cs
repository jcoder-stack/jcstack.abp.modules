using System.Security.Claims;
using JcStack.Abp.Message.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Modularity;
using Volo.Abp.Security.Claims;
using Volo.Abp.Uow;

namespace JcStack.Abp.Message;

[DependsOn(
    typeof(MessageApplicationTestModule),
    typeof(MessageEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule))]
public class MessageEntityFrameworkCoreTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAlwaysDisableUnitOfWorkTransaction();

        var sqliteConnection = CreateDatabaseAndGetConnection();

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(abpDbContextConfigurationContext =>
            {
                abpDbContextConfigurationContext.DbContextOptions.UseSqlite(sqliteConnection);
            });
        });

        // 模拟当前用户
        context.Services.AddSingleton<ICurrentPrincipalAccessor, FakeCurrentPrincipalAccessor>();
    }

    private static SqliteConnection CreateDatabaseAndGetConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        new MessageDbContext(
            new DbContextOptionsBuilder<MessageDbContext>()
                .UseSqlite(connection)
                .Options
        ).GetService<IRelationalDatabaseCreator>().CreateTables();

        return connection;
    }
}

/// <summary>
/// 模拟当前用户主体访问器
/// </summary>
public class FakeCurrentPrincipalAccessor : ICurrentPrincipalAccessor
{
    public ClaimsPrincipal Principal { get; }

    public FakeCurrentPrincipalAccessor()
    {
        Principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(AbpClaimTypes.UserId, MessageTestConsts.TestUserId1.ToString()),
            new Claim(AbpClaimTypes.UserName, MessageTestConsts.TestUserName1)
        ], "FakeAuth"));
    }

    public IDisposable Change(ClaimsPrincipal principal)
    {
        return new DisposeAction(() => { });
    }
}

public class DisposeAction(Action action) : IDisposable
{
    public void Dispose() => action();
}
