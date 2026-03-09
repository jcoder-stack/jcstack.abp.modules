<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <style>
        body {
            margin: 0;
            padding: 0;
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            font-size: 14px;
            line-height: 1.6;
            color: #333;
            background-color: #f5f5f5;
        }
        .email-wrapper {
            max-width: 600px;
            margin: 20px auto;
            background-color: #ffffff;
            border-radius: 8px;
            overflow: hidden;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
        }
        .email-header {
            background-color: #1890ff;
            color: #ffffff;
            padding: 20px 30px;
            text-align: center;
        }
        .email-header h1 {
            margin: 0;
            font-size: 20px;
            font-weight: 600;
        }
        .email-body {
            padding: 30px;
        }
        .email-footer {
            padding: 15px 30px;
            text-align: center;
            font-size: 12px;
            color: #999;
            border-top: 1px solid #f0f0f0;
        }
    </style>
</head>
<body>
    <div class="email-wrapper">
        <div class="email-header">
            <h1>ASMS</h1>
        </div>
        <div class="email-body">
            {{content}}
        </div>
        <div class="email-footer">
            &copy; {{ date.now | date.to_string '%Y' }} ASMS. {{L "Email:FooterText"}}
        </div>
    </div>
</body>
</html>
