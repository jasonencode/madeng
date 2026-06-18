#!/usr/bin/env python3
"""
Claude Code Status Light - 屏幕顶部三色灯状态指示器
"""

import tkinter as tk
from http.server import HTTPServer, BaseHTTPRequestHandler
import json
import threading
from enum import Enum
from typing import Optional


# ==================== 时间配置（毫秒）====================
MARQUEE_ON_TIME = 500     # 跑马灯每个灯亮的时间
MARQUEE_OFF_TIME = 200    # 跑马灯循环一圈后的间隔
BLINK_ON_TIME = 600       # 闪烁时亮的时间
BLINK_OFF_TIME = 400      # 闪烁时灭的时间
# ========================================================


class Status(Enum):
    IDLE = "idle"
    WORKING = "working"
    WAITING = "waiting"
    ERROR = "error"


LIGHT_COLORS = {
    "green": {"color": "#22C55E", "glow": "#16A34A", "highlight": "#4ADE80"},
    "yellow": {"color": "#FACC15", "glow": "#EAB308", "highlight": "#FDE047"},
    "red": {"color": "#EF4444", "glow": "#DC2626", "highlight": "#F87171"},
}

STATUS_CONFIG = {
    Status.IDLE: {"name": "Ready"},
    Status.WORKING: {"name": "Working"},
    Status.WAITING: {"name": "Waiting"},
    Status.ERROR: {"name": "Error"},
}

TRANSPARENT_COLOR = "#FF00FF"


class StatusLightApp:
    def __init__(self):
        self.root = tk.Tk()
        self.root.title("Claude Status")
        self.current_status = Status.IDLE
        self.blink_state = True
        self.marquee_index = 0
        self.animation_id: Optional[str] = None

        self.width = 240
        self.height = 44
        self.light_r = 8
        self.positions = [40, 70, 100]
        self.light_y = self.height // 2

        self._setup_window()
        self._create_widgets()
        self._start_http_server()
        self._start_animation()

    def _setup_window(self):
        screen_width = self.root.winfo_screenwidth()
        x = (screen_width - self.width) // 2

        self.root.geometry(f"{self.width}x{self.height}+{x}+0")
        self.root.overrideredirect(True)
        self.root.attributes("-topmost", True)
        self.root.configure(bg=TRANSPARENT_COLOR)
        self.root.attributes("-transparentcolor", TRANSPARENT_COLOR)

        self._drag_data = {"x": 0, "y": 0}

    def _get_center_x(self):
        screen_width = self.root.winfo_screenwidth()
        return (screen_width - self.width) // 2

    def _create_widgets(self):
        self.canvas = tk.Canvas(
            self.root, width=self.width, height=self.height,
            bg=TRANSPARENT_COLOR, highlightthickness=0
        )
        self.canvas.pack(fill=tk.BOTH, expand=True)

        self.canvas.bind("<Button-1>", self._start_drag)
        self.canvas.bind("<B1-Motion>", self._on_drag)
        self.canvas.bind("<Button-3>", self._reset_position)

        self._update_display()

    def _draw_rounded_rect(self, x1, y1, x2, y2, radius, **kwargs):
        points = [
            x1 + radius, y1, x2 - radius, y1,
            x2, y1, x2, y1 + radius,
            x2, y2 - radius, x2, y2,
            x2 - radius, y2, x1 + radius, y2,
            x1, y2, x1, y2 - radius,
            x1, y1 + radius, x1, y1,
        ]
        return self.canvas.create_polygon(points, smooth=True, **kwargs)

    def _draw_light(self, cx, cy, radius, color_key, is_active):
        if not is_active:
            self.canvas.create_oval(
                cx - radius, cy - radius, cx + radius, cy + radius,
                fill="#1A1A2E", outline="#2A2A3C", width=1
            )
            return

        config = LIGHT_COLORS[color_key]
        color = config["color"]
        glow = config["glow"]
        highlight = config["highlight"]

        glow_r = radius + 3
        self.canvas.create_oval(
            cx - glow_r, cy - glow_r, cx + glow_r, cy + glow_r,
            fill="", outline=glow, width=1
        )
        self.canvas.create_oval(
            cx - radius, cy - radius, cx + radius, cy + radius,
            fill=glow, outline=""
        )
        self.canvas.create_oval(
            cx - radius + 1, cy - radius + 1, cx + radius - 1, cy + radius - 1,
            fill=color, outline=""
        )
        highlight_r = radius * 0.5
        self.canvas.create_oval(
            cx - highlight_r, cy - highlight_r - 1,
            cx + highlight_r, cy + highlight_r - 1,
            fill=highlight, outline=""
        )
        spot_r = radius * 0.2
        spot_cy = cy - radius * 0.2
        self.canvas.create_oval(
            cx - spot_r, spot_cy - spot_r, cx + spot_r, spot_cy + spot_r,
            fill="#FFFFFF", outline=""
        )

    def _update_display(self):
        self.canvas.delete("all")

        config = STATUS_CONFIG[self.current_status]

        self._draw_rounded_rect(2, 2, self.width - 2, self.height - 2, 12,
                                fill="#1E1E2E", outline="#313244", width=1)

        color_keys = ["green", "yellow", "red"]

        if self.current_status == Status.IDLE:
            for i, x in enumerate(self.positions):
                self._draw_light(x, self.light_y, self.light_r, color_keys[i], i == 0)

        elif self.current_status == Status.WORKING:
            for i, x in enumerate(self.positions):
                is_active = (i == self.marquee_index)
                self._draw_light(x, self.light_y, self.light_r, color_keys[i], is_active)

        elif self.current_status == Status.WAITING:
            for i, x in enumerate(self.positions):
                self._draw_light(x, self.light_y, self.light_r, color_keys[i], self.blink_state)

        elif self.current_status == Status.ERROR:
            for i, x in enumerate(self.positions):
                self._draw_light(x, self.light_y, self.light_r, color_keys[i], i == 2)

        self.canvas.create_text(
            155, self.light_y,
            text=config["name"],
            fill="#E0E0E0",
            font=("Segoe UI", 10, "bold"),
            anchor="center"
        )

    def _start_animation(self):
        if self.animation_id:
            self.root.after_cancel(self.animation_id)

        def animate():
            if self.current_status == Status.WORKING:
                self.marquee_index = (self.marquee_index + 1) % 3
                self._update_display()
                delay = MARQUEE_ON_TIME
                if self.marquee_index == 0:
                    delay += MARQUEE_OFF_TIME
                self.animation_id = self.root.after(delay, animate)
            elif self.current_status == Status.WAITING:
                self.blink_state = not self.blink_state
                self._update_display()
                delay = BLINK_ON_TIME if self.blink_state else BLINK_OFF_TIME
                self.animation_id = self.root.after(delay, animate)
            else:
                self.marquee_index = 0
                self.blink_state = True
                self.animation_id = None

        animate()

    def _start_drag(self, event):
        self._drag_data["x"] = event.x
        self._drag_data["y"] = event.y

    def _on_drag(self, event):
        x = self.root.winfo_x() + (event.x - self._drag_data["x"])
        y = self.root.winfo_y() + (event.y - self._drag_data["y"])
        self.root.geometry(f"+{x}+{y}")

    def _reset_position(self, event):
        self.root.geometry(f"+{self._get_center_x()}+0")

    def set_status(self, status: Status):
        if status != self.current_status:
            self.current_status = status
            self._start_animation()
            self._update_display()

    def _start_http_server(self):
        app = self

        class Handler(BaseHTTPRequestHandler):
            def do_POST(self):
                content_length = int(self.headers.get('Content-Length', 0))
                body = self.rfile.read(content_length)
                try:
                    data = json.loads(body)
                    status_str = data.get('status', 'idle')
                    status_map = {
                        'idle': Status.IDLE, 'working': Status.WORKING,
                        'waiting': Status.WAITING, 'error': Status.ERROR,
                    }
                    status = status_map.get(status_str, Status.IDLE)
                    app.root.after(0, app.set_status, status)
                    self.send_response(200)
                    self.send_header('Content-Type', 'application/json')
                    self.send_header('Access-Control-Allow-Origin', '*')
                    self.end_headers()
                    self.wfile.write(json.dumps({"ok": True}).encode())
                except Exception as e:
                    self.send_response(400)
                    self.send_header('Content-Type', 'application/json')
                    self.end_headers()
                    self.wfile.write(json.dumps({"error": str(e)}).encode())

            def do_OPTIONS(self):
                self.send_response(200)
                self.send_header('Access-Control-Allow-Origin', '*')
                self.send_header('Access-Control-Allow-Methods', 'POST, OPTIONS')
                self.send_header('Access-Control-Allow-Headers', 'Content-Type')
                self.end_headers()

            def log_message(self, format, *args):
                pass

        def run_server():
            server = HTTPServer(('127.0.0.1', 51234), Handler)
            server.serve_forever()

        threading.Thread(target=run_server, daemon=True).start()

    def run(self):
        self.root.mainloop()


if __name__ == "__main__":
    StatusLightApp().run()
