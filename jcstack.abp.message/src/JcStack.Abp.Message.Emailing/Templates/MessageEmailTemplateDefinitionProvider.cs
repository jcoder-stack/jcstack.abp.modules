using JcStack.Abp.Message.Localization;
using Volo.Abp.TextTemplating;
using Volo.Abp.TextTemplating.Scriban;

namespace JcStack.Abp.Message.Emailing.Templates;

/// <summary>
/// 消息模块邮件模板定义
/// </summary>
public class MessageEmailTemplateDefinitionProvider : TemplateDefinitionProvider
{
    public override void Define(ITemplateDefinitionContext context)
    {
        // 邮件布局模板
        context.Add(
            new TemplateDefinition(
                MessageEmailTemplates.Layout,
                displayName: null,
                isLayout: true,
                localizationResource: typeof(MessageResource)
            )
            .WithScribanEngine()
            .WithVirtualFilePath(
                "/Templates/Scriban/Layout.tpl",
                isInlineLocalized: true
            )
        );

        // 通知邮件模板
        context.Add(
            new TemplateDefinition(
                MessageEmailTemplates.Notification,
                displayName: null,
                layout: MessageEmailTemplates.Layout,
                localizationResource: typeof(MessageResource)
            )
            .WithScribanEngine()
            .WithVirtualFilePath(
                "/Templates/Scriban/Notification.tpl",
                isInlineLocalized: true
            )
        );
    }
}
