using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using JcStack.Abp.AuditLogging.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AuditLogging;
using Volo.Abp.Auditing;

namespace JcStack.Abp.AuditLogging.AuditLogs;

[Authorize(AuditLoggingPermissions.AuditLogs.Default)]
public class AuditLogAppService : AuditLoggingAppService, IAuditLogAppService
{
    protected IAuditLogRepository AuditLogRepository { get; }

    public AuditLogAppService(IAuditLogRepository auditLogRepository)
    {
        AuditLogRepository = auditLogRepository;
    }

    public virtual async Task<PagedResultDto<AuditLogDto>> GetListAsync(GetAuditLogsInput input)
    {
        var count = await AuditLogRepository.GetCountAsync(
            startTime: input.StartTime,
            endTime: input.EndTime,
            httpMethod: input.HttpMethod,
            url: input.Url,
            userName: input.UserName,
            correlationId: input.CorrelationId,
            httpStatusCode: input.HttpStatusCode != null ? (HttpStatusCode)input.HttpStatusCode : null,
            hasException: input.HasException);

        var list = await AuditLogRepository.GetListAsync(
            sorting: input.Sorting,
            maxResultCount: input.MaxResultCount,
            skipCount: input.SkipCount,
            startTime: input.StartTime,
            endTime: input.EndTime,
            httpMethod: input.HttpMethod,
            url: input.Url,
            userName: input.UserName,
            correlationId: input.CorrelationId,
            httpStatusCode: input.HttpStatusCode != null ? (HttpStatusCode)input.HttpStatusCode : null,
            hasException: input.HasException,
            includeDetails: false);

        return new PagedResultDto<AuditLogDto>(count, MapToDto(list));
    }

    public virtual async Task<AuditLogDto> GetAsync(Guid id)
    {
        var auditLog = await AuditLogRepository.GetAsync(id, includeDetails: true);
        return MapToDto(auditLog);
    }

    [Authorize(AuditLoggingPermissions.AuditLogs.EntityChanges)]
    public virtual async Task<PagedResultDto<EntityChangeDto>> GetEntityChangesAsync(GetEntityChangesInput input)
    {
        var count = await AuditLogRepository.GetEntityChangeCountAsync(
            startTime: input.StartTime,
            endTime: input.EndTime,
            changeType: input.ChangeType,
            entityId: input.EntityId,
            entityTypeFullName: input.EntityTypeFullName);

        var list = await AuditLogRepository.GetEntityChangeListAsync(
            sorting: input.Sorting,
            maxResultCount: input.MaxResultCount,
            skipCount: input.SkipCount,
            startTime: input.StartTime,
            endTime: input.EndTime,
            changeType: input.ChangeType,
            entityId: input.EntityId,
            entityTypeFullName: input.EntityTypeFullName);

        return new PagedResultDto<EntityChangeDto>(count, MapEntityChangesToDto(list));
    }

    [Authorize(AuditLoggingPermissions.AuditLogs.EntityChanges)]
    public virtual async Task<EntityChangeDto> GetEntityChangeAsync(Guid entityChangeId)
    {
        var result = await AuditLogRepository.GetEntityChangeWithUsernameAsync(entityChangeId);
        return MapEntityChangeToDto(result.EntityChange);
    }

    private List<AuditLogDto> MapToDto(List<AuditLog> auditLogs)
    {
        return auditLogs.Select(MapToDto).ToList();
    }

    private AuditLogDto MapToDto(AuditLog auditLog)
    {
        return new AuditLogDto
        {
            Id = auditLog.Id,
            ApplicationName = auditLog.ApplicationName,
            UserId = auditLog.UserId,
            UserName = auditLog.UserName,
            TenantId = auditLog.TenantId,
            TenantName = auditLog.TenantName,
            ImpersonatorUserId = auditLog.ImpersonatorUserId,
            ImpersonatorUserName = auditLog.ImpersonatorUserName,
            ImpersonatorTenantId = auditLog.ImpersonatorTenantId,
            ImpersonatorTenantName = auditLog.ImpersonatorTenantName,
            ExecutionTime = auditLog.ExecutionTime,
            ExecutionDuration = auditLog.ExecutionDuration,
            ClientIpAddress = auditLog.ClientIpAddress,
            ClientName = auditLog.ClientName,
            ClientId = auditLog.ClientId,
            CorrelationId = auditLog.CorrelationId,
            BrowserInfo = auditLog.BrowserInfo,
            HttpMethod = auditLog.HttpMethod,
            Url = auditLog.Url,
            Exceptions = auditLog.Exceptions,
            Comments = auditLog.Comments,
            HttpStatusCode = auditLog.HttpStatusCode,
            Actions = auditLog.Actions?.Select(a => new AuditLogActionDto
            {
                Id = a.Id,
                ServiceName = a.ServiceName,
                MethodName = a.MethodName,
                Parameters = a.Parameters,
                ExecutionTime = a.ExecutionTime,
                ExecutionDuration = a.ExecutionDuration
            }).ToList() ?? [],
            EntityChanges = auditLog.EntityChanges?.Select(MapEntityChangeToDto).ToList() ?? []
        };
    }

    private List<EntityChangeDto> MapEntityChangesToDto(List<EntityChange> entityChanges)
    {
        return entityChanges.Select(MapEntityChangeToDto).ToList();
    }

    private EntityChangeDto MapEntityChangeToDto(EntityChange entityChange)
    {
        return new EntityChangeDto
        {
            Id = entityChange.Id,
            AuditLogId = entityChange.AuditLogId,
            ChangeTime = entityChange.ChangeTime,
            ChangeType = entityChange.ChangeType,
            EntityId = entityChange.EntityId,
            EntityTypeFullName = entityChange.EntityTypeFullName,
            PropertyChanges = entityChange.PropertyChanges?.Select(p => new EntityPropertyChangeDto
            {
                Id = p.Id,
                PropertyName = p.PropertyName,
                PropertyTypeFullName = p.PropertyTypeFullName,
                OriginalValue = p.OriginalValue,
                NewValue = p.NewValue
            }).ToList() ?? []
        };
    }
}
