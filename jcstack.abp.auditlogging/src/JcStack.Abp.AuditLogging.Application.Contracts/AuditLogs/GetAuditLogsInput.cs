using System;
using Volo.Abp.Application.Dtos;

namespace JcStack.Abp.AuditLogging.AuditLogs;

public class GetAuditLogsInput : PagedAndSortedResultRequestDto
{
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? UserName { get; set; }
    public string? HttpMethod { get; set; }
    public string? Url { get; set; }
    public int? HttpStatusCode { get; set; }
    public bool? HasException { get; set; }
    public string? CorrelationId { get; set; }
}
