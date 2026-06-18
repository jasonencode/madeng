# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MD-Light (码灯/牛马指示灯) is a screen-top status indicator light for Claude Code. It displays the current working state of Claude Code through visual LED-like indicators. The project provides three independent implementations:

- **statuslight-wpf** - Windows WPF version (.NET 8.0)
- **statuslight-python** - Python version (tkinter, cross-platform)
- **statuslight-go** - Go version (Fyne GUI, cross-platform)

## Architecture

All implementations follow the same architecture:

1. **HTTP Server** - Listens on `http://127.0.0.1:51234` for status updates
2. **GUI Window** - Always-on-top window at screen top showing 3 colored lights
3. **Animation System** - Handles breathing, blinking, and marquee effects
4. **Claude Code Hooks** - Integrates via `~/.claude/settings.json` hooks

### Status States

| API Status | Visual Effect | Meaning |
|------------|---------------|---------|
| `idle` | Green solid | Waiting for input |
| `working` | Marquee breathing | Processing |
| `completed` | Green breathing | Task done |
| `waiting` | All 3 blinking | Awaiting permission |
| `error` | Red solid | Error occurred |

## Build Commands

### WPF Version (Windows)

```bash
cd statuslight-wpf

# Build
dotnet build StatusLight.csproj -c Release

# Run
dotnet run -c Release

# Publish single-file executable
dotnet publish StatusLight.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

### Python Version

```bash
cd statuslight-python

# Install dependencies
pip install -r requirements.txt

# Run
python claude_status_light.py
```

### Go Version

```bash
cd statuslight-go

# Build
go build -o statuslight .

# Run
./statuslight
```

## Key Implementation Details

### WPF Version
- Uses `System.Windows.Threading.DispatcherTimer` for animations
- Settings stored in `settings.json` (port, timing, opacity)
- Supports window dragging via `MouseLeftButtonDown`
- Right-click context menu for settings and controls

### Python Version
- Uses `tkinter.Canvas` for custom light rendering
- Transparent window via `overrideredirect(True)` and `-transparentcolor`
- HTTP server runs in daemon thread
- Animation via `root.after()` callback loop

### Go Version
- Uses Fyne v2 GUI toolkit
- HTTP server via standard `net/http` package
- Animation via goroutine with `time.Ticker`
- Thread-safe status updates with `sync.Mutex`

## Configuration

### settings.json (WPF)

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

### Claude Code Hooks

Add to `~/.claude/settings.json` to enable automatic status updates. See `hooks.json` in statuslight-wpf for the complete hook configuration.

## API Endpoint

All versions expose the same HTTP API:

```
POST http://127.0.0.1:51234
Content-Type: application/json

{"status": "idle|working|completed|waiting|error"}
```
