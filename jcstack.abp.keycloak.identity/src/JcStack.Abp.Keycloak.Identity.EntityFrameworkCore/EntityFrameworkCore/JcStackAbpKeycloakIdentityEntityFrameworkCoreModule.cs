using JcStack.Abp.Identity;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace JcStack.Abp.Keycloak.Identity.EntityFrameworkCore;

[DependsOn(
    typeof(JcStackAbpKeycloakIdentityDomainModule),
    typeof(AbpEntityFrameworkCoreModule),
    typeof(JcStackAbpIdentityEntityFrameworkCoreModule)
)]
public class JcStackAbpKeycloakIdentityEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<IdentityDbContext>(options =>
        {
            options.AddDefaultRepositories<IIdentityDbContext>(includeAllEntities: true);

            /* Add custom repositories here. Example:
            * options.AddRepository<Question, EfCoreQuestionRepository>();
            */
        });
    }
}
