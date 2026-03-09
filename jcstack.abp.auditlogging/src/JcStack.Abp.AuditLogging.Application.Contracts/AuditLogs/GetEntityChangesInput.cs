using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;

namespace JcStack.Abp.AuditLogging.AuditLogs;

public class GetEntityChangesInput : PagedAndSortedResultRequestDto
{
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? EntityTypeFullName { get; set; }
    public string? EntityId { get; set; }
    public EntityChangeType? ChangeType { get; set; }
}
