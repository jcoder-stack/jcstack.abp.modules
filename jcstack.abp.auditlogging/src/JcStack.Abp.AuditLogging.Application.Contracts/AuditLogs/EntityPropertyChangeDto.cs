using System;
using Volo.Abp.Application.Dtos;

namespace JcStack.Abp.AuditLogging.AuditLogs;

public class EntityPropertyChangeDto : EntityDto<Guid>
{
    public string PropertyName { get; set; } = default!;
    public string PropertyTypeFullName { get; set; } = default!;
    public string? OriginalValue { get; set; }
    public string? NewValue { get; set; }
}
