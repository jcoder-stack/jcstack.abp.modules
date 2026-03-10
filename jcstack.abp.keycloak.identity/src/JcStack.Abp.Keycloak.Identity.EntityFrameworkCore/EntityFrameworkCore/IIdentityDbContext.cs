using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace JcStack.Abp.Keycloak.Identity.EntityFrameworkCore;

[ConnectionStringName(IdentityDbProperties.ConnectionStringName)]
public interface IIdentityDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
}
