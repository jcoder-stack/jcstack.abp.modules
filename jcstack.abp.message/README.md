# JcStack.Abp.Message

ABP 消息通知模块，提供 SignalR 实时推送和邮件通知功能。

## 模块结构

```
JcStack.Abp.Message/           # 核心模块（事件处理、通知调度）
JcStack.Abp.Message.SignalR/   # SignalR 扩展（Hub、实时推送）
```

## 安装

**Application 层**引用核心模块：
```csharp
[DependsOn(typeof(AbpMessageModule))]
public class YourApplicationModule : AbpModule { }
```

**HttpApi.Host** 引用 SignalR 模块：
```csharp
[DependsOn(typeof(AbpMessageSignalRModule))]
public class YourHttpApiHostModule : AbpModule { }
```

---

## 前端对接指南

### 1. SignalR 连接配置

| 配置项 | 值 |
|-------|-----|
| Hub 路由 | `/signalr-hubs/notifications` |
| 认证 | 需要 Bearer Token |
| 监听事件 | `ReceiveNotification` |

### 2. JavaScript/TypeScript 示例

```typescript
import * as signalR from "@microsoft/signalr";

// 创建连接
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/signalr-hubs/notifications", {
    accessTokenFactory: () => getAccessToken(), // 返回你的 JWT Token
  })
  .withAutomaticReconnect()
  .build();

// 监听通知事件
connection.on("ReceiveNotification", (message) => {
  console.log("收到通知:", message);
  // message 结构见下方
});

// 启动连接
connection.start()
  .then(() => console.log("SignalR 已连接"))
  .catch((err) => console.error("SignalR 连接失败:", err));

// 加入组织分组（可选）
async function joinOrganization(orgUnitId: string) {
  await connection.invoke("JoinOrganizationGroup", orgUnitId);
}

// 离开组织分组（可选）
async function leaveOrganization(orgUnitId: string) {
  await connection.invoke("LeaveOrganizationGroup", orgUnitId);
}
```

### 3. 通知消息结构

```typescript
interface NotificationMessage {
  title: string;           // 通知标题
  body: string;            // 通知内容
  type: NotificationType;  // 通知类型
  severity: NotificationSeverity; // 严重程度
  module?: string;         // 来源模块
  eventName?: string;      // 事件名称
  entityId?: string;       // 关联实体 ID
  data?: Record<string, any>; // 扩展数据
  createdAt: string;       // 创建时间 (ISO 8601)
}

enum NotificationType {
  Info = 0,
  Success = 1,
  Warning = 2,
  Error = 3,
}

enum NotificationSeverity {
  Low = 0,
  Normal = 1,
  High = 2,
  Urgent = 3,
}
```

### 4. 自动分组

连接成功后，Hub 自动将用户加入以下分组：

| 分组 | 格式 | 说明 |
|-----|------|-----|
| 用户分组 | `user:{userId}` | 接收个人通知 |
| 租户分组 | `tenant:{tenantId}` | 接收租户广播 |

可手动加入/离开组织分组：
- `JoinOrganizationGroup(orgUnitId)` - 加入组织
- `LeaveOrganizationGroup(orgUnitId)` - 离开组织

---

## 测试 API

### 发送测试通知给当前用户

```http
POST /api/app/notification-test/send-to-current-user?title=测试标题&body=测试内容
Authorization: Bearer {token}
```

### 发送测试通知给当前租户

```http
POST /api/app/notification-test/send-to-current-tenant?title=测试标题&body=测试内容
Authorization: Bearer {token}
```

### Swagger 测试步骤

1. 打开 Swagger UI：`http://localhost:5000/swagger`
2. 点击 **Authorize** 完成 OAuth 登录
3. 找到 `NotificationTest` 分组
4. 执行 `SendToCurrentUser` 或 `SendToCurrentTenant`

---

## 完整测试流程

### 步骤 1：启动后端

```bash
cd src/Asms.HttpApi.Host
dotnet run
```

### 步骤 2：前端连接 SignalR

```javascript
// 浏览器控制台测试
const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5000/signalr-hubs/notifications", {
    accessTokenFactory: () => "your-jwt-token-here",
  })
  .build();

connection.on("ReceiveNotification", (msg) => {
  console.log("📬 收到通知:", msg);
});

await connection.start();
console.log("✅ 已连接");
```

### 步骤 3：触发测试通知

通过 Swagger 或 curl 调用测试 API：

```bash
curl -X POST "http://localhost:5000/api/app/notification-test/send-to-current-user?title=Hello&body=World" \
  -H "Authorization: Bearer YOUR_TOKEN"
```

### 步骤 4：验证结果

浏览器控制台应显示：
```
📬 收到通知: {title: "Hello", body: "World", type: 0, severity: 1, ...}
```

---

## 常量引用

前端可直接使用以下常量值：

```typescript
// Hub 路由
const HUB_ROUTE = "/signalr-hubs/notifications";

// 事件名称
const HUB_METHODS = {
  ReceiveNotification: "ReceiveNotification",
};

// 分组前缀
const GROUP_PREFIXES = {
  User: "user:",
  Tenant: "tenant:",
  Organization: "org:",
};
```

---

## 业务事件通知

模块已内置对 AfterSales 售后工单事件的订阅，以下事件会自动推送通知：

| 事件 | 说明 |
|-----|------|
| `AfterSalesOrderCreatedEto` | 工单创建 |
| `AfterSalesOrderAssignedEto` | 工单指派 |
| `AfterSalesOrderAcceptedEto` | 工单受理 |
| `AfterSalesOrderStartedProcessingEto` | 开始处理 |
| `AfterSalesOrderCompletedEto` | 处理完成 |
| `AfterSalesOrderClosedEto` | 工单关闭 |
| `AfterSalesOrderCancelledEto` | 工单取消 |
| `AfterSalesOrderEscalatedEto` | 工单升级 |
| `AfterSalesOrderCommentAddedEto` | 添加备注 |

通知会自动发送给：
- 工单创建人
- 工单处理人（如已指派）
