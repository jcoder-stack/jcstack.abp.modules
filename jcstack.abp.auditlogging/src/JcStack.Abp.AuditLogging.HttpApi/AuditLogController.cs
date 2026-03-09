using System;
using System.Threading.Tasks;
using JcStack.Abp.AuditLogging.AuditLogs;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace JcStack.Abp.AuditLogging;

[RemoteService(Name = AuditLoggingRemoteServiceConsts.RemoteServiceName)]
[Area(AuditLoggingRemoteServiceConsts.ModuleName)]
[Route("api/audit-logging/audit-logs")]
public class AuditLogController : AuditLoggingControllerBase, IAuditLogAppService
{
    protected IAuditLogAppService AuditLogAppService { get; }

    public AuditLogController(IAuditLogAppService auditLogAppService)
    {
        AuditLogAppService = auditLogAppService;
    }

    [HttpGet]
    public virtual Task<PagedResultDto<AuditLogDto>> GetListAsync([FromQuery] GetAuditLogsInput input)
    {
        return AuditLogAppService.GetListAsync(input);
    }

    [HttpGet]
    [Route("{id}")]
    public virtual Task<AuditLogDto> GetAsync(Guid id)
    {
        return AuditLogAppService.GetAsync(id);
    }

    [HttpGet]
    [Route("entity-changes")]
    public virtual Task<PagedResultDto<EntityChangeDto>> GetEntityChangesAsync([FromQuery] GetEntityChangesInput input)
    {
        return AuditLogAppService.GetEntityChangesAsync(input);
    }

    [HttpGet]
    [Route("entity-changes/{entityChangeId}")]
    public virtual Task<EntityChangeDto> GetEntityChangeAsync(Guid entityChangeId)
    {
        return AuditLogAppService.GetEntityChangeAsync(entityChangeId);
    }
}
