# StatusLight v1.3.0

屏幕顶部状态指示灯，配合 Claude Code 使用。

## 运行要求

- Windows 10/11 (x64)
- .NET 8.0 Desktop Runtime（开发运行时需要，打包后的单文件版本不需要）

## 状态说明

| API 状态 | 灯效 | 含义 |
|----------|------|------|
| `idle` | 绿灯常亮 | 等待输入 |
| `working` | 跑马灯呼吸 | 工作中 |
| `completed` | 绿灯呼吸 | 任务完成 |
| `waiting` | 三灯闪烁 | 等待授权 |
| `error` | 红灯呼吸 | 出现错误 |

## 编译运行

```bash
# 编译
dotnet build StatusLight.csproj -c Release

# 运行
dotnet run -c Release
```

## 打包发布

```bash
# 发布单文件应用（无需安装 .NET 运行时）
dotnet publish StatusLight.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

发布后在 `publish/` 目录生成：
- `StatusLight.exe` - 主程序（单文件，约 150MB）
- `settings.json` - 配置文件
- `hooks.json` - Claude Code hooks 配置

## 制作安装包

1. 下载安装 [Inno Setup](https://jrsoftware.org/isdl.php)
2. 打开 `installer.iss` 文件
3. 点击 构建 → 编译
4. 安装包生成在 `installer/` 目录

## 配置文件

**settings.json** - 应用配置

```json
{
  "MarqueeOnTime": 500,
  "MarqueeOffTime": 200,
  "BlinkOnTime": 600,
  "BlinkOffTime": 400,
  "BreathCycleTime": 3000,
  "Port": 51234,
  "BackgroundOpacity": 0.6
}
```

## Claude Code Hooks

将以下配置添加到 `~/.claude/settings.json`：

```json
{
  "hooks": {
    "SessionStart": [
      {
        "hooks": [
          {
            "type": "command",
            "command": "curl -X POST http://127.0.0.1:51234 -H \"Content-Type: application/json\" -d \"{\\\"status\\\": \\\"idle\\\"}\""
          }
        ]
      }
    ],
    "UserPromptSubmit": [
      {
        "hooks": [
          {
            "type": "command",
            "command": "curl -X POST http://127.0.0.1:51234 -H \"Content-Type: application/json\" -d \"{\\\"status\\\": \\\"working\\\"}\""
          }
        ]
      }
    ],
    "Stop": [
      {
        "hooks": [
          {
            "type": "command",
            "command": "curl -X POST http://127.0.0.1:51234 -H \"Content-Type: application/json\" -d \"{\\\"status\\\": \\\"completed\\\"}\""
          }
        ]
      }
    ],
    "StopFailure": [
      {
        "hooks": [
          {
            "type": "command",
            "command": "curl -X POST http://127.0.0.1:51234 -H \"Content-Type: application/json\" -d \"{\\\"status\\\": \\\"error\\\"}\""
          }
        ]
      }
    ],
    "PermissionRequest": [
      {
        "hooks": [
          {
            "type": "command",
            "command": "curl -X POST http://127.0.0.1:51234 -H \"Content-Type: application/json\" -d \"{\\\"status\\\": \\\"waiting\\\"}\""
          }
        ]
      }
    ],
    "PostToolUse": [
      {
        "matcher": "",
        "hooks": [
          {
            "type": "command",
            "command": "curl -X POST http://127.0.0.1:51234 -H \"Content-Type: application/json\" -d \"{\\\"status\\\": \\\"working\\\"}\""
          }
        ]
      }
    ],
    "PostToolUseFailure": [
      {
        "matcher": "",
        "hooks": [
          {
            "type": "command",
            "command": "curl -X POST http://127.0.0.1:51234 -H \"Content-Type: application/json\" -d \"{\\\"status\\\": \\\"error\\\"}\""
          }
        ]
      }
    ],
    "PermissionDenied": [
      {
        "hooks": [
          {
            "type": "command",
            "command": "curl -X POST http://127.0.0.1:51234 -H \"Content-Type: application/json\" -d \"{\\\"status\\\": \\\"idle\\\"}\""
          }
        ]
      }
    ],
    "SessionEnd": [
      {
        "hooks": [
          {
            "type": "command",
            "command": "curl -X POST http://127.0.0.1:51234 -H \"Content-Type: application/json\" -d \"{\\\"status\\\": \\\"idle\\\"}\""
          }
        ]
      }
    ]
  }
}
```

**Hook 事件说明：**

| 事件 | 触发时机 | 灯效 |
|------|----------|------|
| `SessionStart` | 会话开始 | 绿灯常亮 (idle) |
| `UserPromptSubmit` | 用户提交提示 | 跑马灯 (working) |
| `PostToolUse` | 工具使用后 | 跑马灯 (working) |
| `Stop` | Claude 完成响应 | 绿灯呼吸 (completed) |
| `StopFailure` | 响应失败 | 红灯呼吸 (error) |
| `PostToolUseFailure` | 工具失败 | 红灯呼吸 (error) |
| `PermissionRequest` | 权限请求弹窗 | 三灯闪烁 (waiting) |
| `PermissionDenied` | 权限拒绝 | 绿灯常亮 (idle) |
| `SessionEnd` | 会话结束 | 绿灯常亮 (idle) |

## 右键菜单

- **设置** - 配置时间参数、透明度、端口
- **恢复Ready状态** - 手动切换到绿灯常亮
- **还原位置** - 窗口回到顶部居中
- **关于** - 查看说明和 hooks 配置
- **退出** - 关闭应用

## 操作说明

- **左键拖拽** - 移动窗口
- **右键点击** - 显示菜单
