# MD-Light 码灯 （牛马指示灯）

屏幕顶部状态指示灯，配合 Claude Code 使用。

通过监听 `~/.claude/sessions/` 目录下的 JSON 文件，实时显示所有 Claude Code 会话的工作状态。每个会话独立显示 PID、三色灯效、状态文本和工作目录。

## 项目结构

```
├── MaDeng/                # 主项目 - WPF 版本 (Windows)
├── statuslight-wpf/       # 旧版 WPF（基于 HTTP 服务器，已废弃）
└── statuslight-python/    # Python 版本 (跨平台)
```

## 状态说明

| 状态 | 灯效 | 含义 |
|------|------|------|
| `idle` | 绿灯常亮 | 等待输入 |
| `working` / `busy` | 跑马灯呼吸（绿→黄→红依次） | 工作中 |
| `completed` | 绿灯呼吸 | 任务完成 |
| `waiting` | 三灯同步闪烁 | 等待授权 |
| `error` | 红灯呼吸 | 出现错误 |

## 运行要求

- Windows 10/11 (x64)
- .NET 8.0 Desktop Runtime（开发时需要，打包后不需要）

## 快速开始

```bash
cd MaDeng

# 编译运行
dotnet build MaDeng.csproj -c Release
dotnet run -c Release

# 打包发布
dotnet publish MaDeng.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

## 工作原理

MaDeng 通过 `FileSystemWatcher` 监听 `~/.claude/sessions/*.json` 文件变化，无需配置 HTTP 服务器或 Hooks。Claude Code 自身会将会话信息写入该目录，MaDeng 自动读取并展示。

每个会话显示：
- **Agent 名称** — 从 sessions 目录的父文件夹名推导（如 `~/.claude/` → Claude）
- **PID** — 进程 ID
- **三色灯** — 绿/黄/红，对应不同状态的动画效果
- **状态文本** — Ready / Working / Done / Waiting / Error
- **打开目录** — 点击文件夹图标可直接打开工作目录

## 配置

设置通过右键菜单 → 设置 打开，保存在 `settings.json`，修改后立即生效（无需重启）：

```json
{
  "MarqueeOnTime": 500,
  "MarqueeOffTime": 200,
  "BlinkOnTime": 600,
  "BlinkOffTime": 400,
  "BreathCycleTime": 3000,
  "BackgroundOpacity": 0.6
}
```

## 操作说明

- **左键拖拽** - 移动窗口
- **右键点击** - 显示菜单（设置 / 还原位置 / 关于 / 退出）

## 旧版说明

`statuslight-wpf` 和 `statuslight-python` 是基于 HTTP 服务器的旧版实现，需要配置 Claude Code Hooks 才能使用。新项目 MaDeng 已不需要这些配置。

## 许可证

MIT
