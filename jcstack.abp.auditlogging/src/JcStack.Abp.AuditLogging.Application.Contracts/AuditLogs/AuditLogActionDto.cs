using System;
using Volo.Abp.Application.Dtos;

namespace JcStack.Abp.AuditLogging.AuditLogs;

public class AuditLogActionDto : EntityDto<Guid>
{
    public string? ServiceName { get; set; }
    public string? MethodName { get; set; }
    public string? Parameters { get; set; }
    public DateTime ExecutionTime { get; set; }
    public int ExecutionDuration { get; set; }
}
