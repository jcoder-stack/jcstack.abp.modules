<h2 style="margin: 0 0 16px; font-size: 18px; color: #333;">{{ model.title }}</h2>
<div style="margin-bottom: 16px; color: #555;">
    {{ model.body }}
</div>
{{ if model.source_module }}
<p style="margin: 16px 0 0; padding-top: 12px; border-top: 1px solid #f0f0f0; font-size: 12px; color: #999;">
    {{L "Email:SourceModule"}}: {{ model.source_module }}
    {{ if model.source_event }} / {{ model.source_event }}{{ end }}
</p>
{{ end }}
