using JcStack.Abp.Message.Emailing.Templates;
using JcStack.Abp.Message.Localization;
using JcStack.Abp.Message.Notifications;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Volo.Abp.Emailing;
using Volo.Abp.TextTemplating;

namespace JcStack.Abp.Message.Emailing;

/// <summary>
/// 默认邮件渠道发送器（可选基类）。
/// 不自动注册到 DI。简单项目可手动注册：
/// <code>context.Services.AddTransient&lt;IChannelSender, DefaultEmailChannelSender&gt;();</code>
/// 需要业务定制的项目（如 Magic Link、用户查找）应自行实现 IChannelSender，
/// 可参考此类的模板渲染逻辑。
/// </summary>
public class DefaultEmailChannelSender : IChannelSender
{
    public ChannelType Channel => ChannelType.Email;
    public int Order => 10;

    private readonly IEmailSender _emailSender;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IStringLocalizer<MessageResource> _localizer;
    private readonly ILogger<DefaultEmailChannelSender> _logger;

    public DefaultEmailChannelSender(
        IEmailSender emailSender,
        ITemplateRenderer templateRenderer,
        IStringLocalizer<MessageResource> localizer,
        ILogger<DefaultEmailChannelSender> logger)
    {
        _emailSender = emailSender;
        _templateRenderer = templateRenderer;
        _localizer = localizer;
        _logger = logger;
    }

    public async Task<ChannelSendResult> SendAsync(
        ChannelSendContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(context.Email))
        {
            _logger.LogDebug(
                "Email channel skipped: no recipient email for notification {NotificationId}",
                context.NotificationId);
            return ChannelSendResult.Ok(0);
        }

        try
        {
            var templateName = context.TemplateName ?? MessageEmailTemplates.Notification;
            var model = context.TemplateModel ?? BuildDefaultModel(context);

            var body = await _templateRenderer.RenderAsync(templateName, model);

            var subject = context.Title;

            await _emailSender.SendAsync(
                context.Email,
                subject,
                body,
                isBodyHtml: true
            );

            _logger.LogDebug(
                "Email sent to {Email} for notification {NotificationId}: {Subject}",
                context.Email,
                context.NotificationId,
                subject);

            return ChannelSendResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send email to {Email} for notification {NotificationId}",
                context.Email,
                context.NotificationId);
            return ChannelSendResult.Fail(ex.Message);
        }
    }

    private static object BuildDefaultModel(ChannelSendContext context)
    {
        return new
        {
            title = context.Title,
            body = context.Body,
            source_module = context.SourceModule,
            source_event = context.SourceEvent,
            entity_type = context.EntityType,
            entity_id = context.EntityId,
            severity = context.Severity.ToString(),
            type = context.Type.ToString(),
            data = context.Data
        };
    }
}
