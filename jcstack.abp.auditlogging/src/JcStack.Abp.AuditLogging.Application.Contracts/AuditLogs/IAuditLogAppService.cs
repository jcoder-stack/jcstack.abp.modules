using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace JcStack.Abp.AuditLogging.AuditLogs;

public interface IAuditLogAppService : IApplicationService
{
    Task<PagedResultDto<AuditLogDto>> GetListAsync(GetAuditLogsInput input);

    Task<AuditLogDto> GetAsync(Guid id);

    Task<PagedResultDto<EntityChangeDto>> GetEntityChangesAsync(GetEntityChangesInput input);

    Task<EntityChangeDto> GetEntityChangeAsync(Guid entityChangeId);
}
