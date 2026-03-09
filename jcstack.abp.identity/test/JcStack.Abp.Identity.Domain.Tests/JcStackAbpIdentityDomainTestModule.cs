using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Sqlite;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.Uow;

namespace JcStack.Abp.Identity;

[DependsOn(
    typeof(JcStackAbpIdentityDomainModule),
    typeof(JcStackAbpIdentityTestBaseModule),
    typeof(AbpIdentityEntityFrameworkCoreModule),
    typeof(AbpPermissionManagementEntityFrameworkCoreModule),
    typeof(AbpEntityFrameworkCoreSqliteModule)
)]
public class JcStackAbpIdentityDomainTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var sqliteConnection = CreateDatabaseAndGetConnection();

        Configure<AbpUnitOfWorkDefaultOptions>(options =>
        {
            options.TransactionBehavior = UnitOfWorkTransactionBehavior.Disabled;
        });

        Configure<AbpDbContextOptions>(options =>
        {
            options.Configure(abpDbContextConfigurationContext =>
            {
                abpDbContextConfigurationContext.DbContextOptions.UseSqlite(sqliteConnection);
            });
        });
    }

    private static SqliteConnection CreateDatabaseAndGetConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        // Create Identity tables
        using (var identityDbContext = new IdentityDbContext(
            new DbContextOptionsBuilder<IdentityDbContext>()
                .UseSqlite(connection)
                .Options))
        {
            identityDbContext.GetService<IRelationalDatabaseCreator>().CreateTables();
        }

        // Create PermissionManagement tables
        using (var permissionDbContext = new PermissionManagementDbContext(
            new DbContextOptionsBuilder<PermissionManagementDbContext>()
                .UseSqlite(connection)
                .Options))
        {
            permissionDbContext.GetService<IRelationalDatabaseCreator>().CreateTables();
        }

        return connection;
    }
}
