# Claude Code Status Light

屏幕顶部三色灯状态指示器，用于显示 Claude Code 的工作状态。

## 状态说明

| 灯状态 | 颜色 | 含义 |
|--------|------|------|
| 绿色常亮 | 🟢 | 空闲/等待输入 |
| 黄色常亮 | 🟡 | 工作中/思考中 |
| 黄色闪烁 | 🟡 | 等待用户授权 |
| 红色常亮 | 🔴 | 出现错误 |

## 安装依赖

```bash
pip install requests
```

## 启动应用

```bash
python claude_status_light.py
```

应用启动后会在屏幕顶部中间显示三色灯，并在 `http://127.0.0.1:51234` 启动 HTTP 服务器。

## 操作说明

- **拖拽移动**: 鼠标左键拖拽窗口
- **关闭应用**: 右键点击窗口或点击 × 按钮

## API 接口

### POST /

发送状态更新

**请求体:**
```json
{
  "status": "idle" | "working" | "waiting" | "error"
}
```

**示例 (curl):**
```bash
# 设置为工作中状态
curl -X POST http://127.0.0.1:51234 -H "Content-Type: application/json" -d '{"status": "working"}'

# 设置为空闲状态
curl -X POST http://127.0.0.1:51234 -H "Content-Type: application/json" -d '{"status": "idle"}'
```

## 配置 Claude Code Hook

在 Claude Code 的配置文件中添加以下 hook 配置:

### 方式一: 使用 curl (Windows/Linux/Mac)

```json
{
  "hooks": {
    "on_start": [
      {
        "type": "command",
        "command": "curl -X POST http://127.0.0.1:51234 -H 'Content-Type: application/json' -d '{\"status\": \"working\"}'"
      }
    ],
    "on_stop": [
      {
        "type": "command",
        "command": "curl -X POST http://127.0.0.1:51234 -H 'Content-Type: application/json' -d '{\"status\": \"idle\"}'"
      }
    ],
    "on_permission_request": [
      {
        "type": "command",
        "command": "curl -X POST http://127.0.0.1:51234 -H 'Content-Type: application/json' -d '{\"status\": \"waiting\"}'"
      }
    ],
    "on_error": [
      {
        "type": "command",
        "command": "curl -X POST http://127.0.0.1:51234 -H 'Content-Type: application/json' -d '{\"status\": \"error\"}'"
      }
    ]
  }
}
```

### 方式二: 使用 Python 脚本

```json
{
  "hooks": {
    "on_start": [
      {
        "type": "command",
        "command": "python /path/to/send_status.py working"
      }
    ],
    "on_stop": [
      {
        "type": "command",
        "command": "python /path/to/send_status.py idle"
      }
    ],
    "on_permission_request": [
      {
        "type": "command",
        "command": "python /path/to/send_status.py waiting"
      }
    ],
    "on_error": [
      {
        "type": "command",
        "command": "python /path/to/send_status.py error"
      }
    ]
  }
}
```

## 文件说明

- `claude_status_light.py` - 主程序
- `send_status.py` - 命令行发送状态的辅助脚本
- `claude_hooks_example.json` - Claude Code hook 配置示例
