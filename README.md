# NM-Light (StatusLight)

屏幕顶部状态指示灯，配合 Claude Code 使用。

## 项目结构

```
├── claude-status-light-wpf/   # WPF 版本 (Windows)
└── claude-status-light/       # Python 版本 (跨平台)
```

## 状态说明

| API 状态 | 灯效 | 含义 |
|----------|------|------|
| `idle` | 绿灯常亮 | 等待输入 |
| `working` | 跑马灯呼吸 | 工作中 |
| `completed` | 绿灯呼吸 | 任务完成 |
| `waiting` | 三灯闪烁 | 等待授权 |
| `error` | 红灯常亮 | 出现错误 |

## 运行要求

**WPF 版本：**
- Windows 10/11 (x64)
- .NET 8.0 Desktop Runtime（开发时需要，打包后不需要）

**Python 版本：**
- Python 3.8+
- tkinter（通常自带）

## 快速开始

### WPF 版本

```bash
cd claude-status-light-wpf

# 编译运行
dotnet build StatusLight.csproj -c Release
dotnet run -c Release

# 打包发布
dotnet publish StatusLight.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

### Python 版本

```bash
cd claude-status-light

# 安装依赖
pip install -r requirements.txt

# 运行
python claude_status_light.py
```

## Claude Code Hooks 配置

将以下配置添加到 `~/.claude/settings.json`：

```json
{
  "hooks": {
    "SessionStart": [{ "hooks": [{ "type": "command", "command": "curl -X POST http://127.0.0.1:51234 -H \"Content-Type: application/json\" -d \"{\\\"status\\\": \\\"idle\\\"}\"" }] }],
    "UserPromptSubmit": [{ "hooks": [{ "type": "command", "command": "curl -X POST http://127.0.0.1:51234 -H \"Content-Type: application/json\" -d \"{\\\"status\\\": \\\"working\\\"}\"" }] }],
    "Stop": [{ "hooks": [{ "type": "command", "command": "curl -X POST http://127.0.0.1:51234 -H \"Content-Type: application/json\" -d \"{\\\"status\\\": \\\"completed\\\"}\"" }] }],
    "StopFailure": [{ "hooks": [{ "type": "command", "command": "curl -X POST http://127.0.0.1:51234 -H \"Content-Type: application/json\" -d \"{\\\"status\\\": \\\"error\\\"}\"" }] }],
    "PermissionRequest": [{ "hooks": [{ "type": "command", "command": "curl -X POST http://127.0.0.1:51234 -H \"Content-Type: application/json\" -d \"{\\\"status\\\": \\\"waiting\\\"}\"" }] }],
    "PostToolUse": [{ "matcher": "", "hooks": [{ "type": "command", "command": "curl -X POST http://127.0.0.1:51234 -H \"Content-Type: application/json\" -d \"{\\\"status\\\": \\\"working\\\"}\"" }] }],
    "PostToolUseFailure": [{ "matcher": "", "hooks": [{ "type": "command", "command": "curl -X POST http://127.0.0.1:51234 -H \"Content-Type: application/json\" -d \"{\\\"status\\\": \\\"error\\\"}\"" }] }],
    "PermissionDenied": [{ "hooks": [{ "type": "command", "command": "curl -X POST http://127.0.0.1:51234 -H \"Content-Type: application/json\" -d \"{\\\"status\\\": \\\"idle\\\"}\"" }] }],
    "SessionEnd": [{ "hooks": [{ "type": "command", "command": "curl -X POST http://127.0.0.1:51234 -H \"Content-Type: application/json\" -d \"{\\\"status\\\": \\\"idle\\\"}\"" }] }]
  }
}
```

## 操作说明

- **左键拖拽** - 移动窗口
- **右键点击** - 显示菜单（设置/还原位置/关于/退出）

## 许可证

MIT
