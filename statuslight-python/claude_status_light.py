#!/usr/bin/env python3
"""
StatusLight - 屏幕顶部单灯状态指示器
"""

import tkinter as tk
from http.server import HTTPServer, BaseHTTPRequestHandler
import json
import threading
import math
from enum import Enum
from typing import Optional


# ==================== 时间配置（毫秒）====================
MARQUEE_ON_TIME = 500
MARQUEE_OFF_TIME = 200
BLINK_ON_TIME = 600
BLINK_OFF_TIME = 400
BREATH_CYCLE_TIME = 3000
COLOR_CYCLE_TIME = 3000  # working 颜色循环周期
# ========================================================


class Status(Enum):
    IDLE = "idle"
    WORKING = "working"
    WAITING = "waiting"
    ERROR = "error"
    COMPLETED = "completed"


STATUS_NAMES = {
    Status.IDLE: "Ready",
    Status.WORKING: "Working",
    Status.WAITING: "Waiting",
    Status.ERROR: "Error",
    Status.COMPLETED: "Done",
}

TRANSPARENT_COLOR = "#FF00FF"


class StatusLightApp:
    def __init__(self):
        self.root = tk.Tk()
        self.root.title("StatusLight")
        self.current_status = Status.IDLE
        self.blink_state = True
        self.breath_progress = 0.0
        self.color_progress = 0.0
        self.animation_id: Optional[str] = None

        self.width = 160
        self.height = 52
        self.light_r = 15
        self.light_x = 50
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

    def _draw_light_off(self, cx, cy, radius):
        self.canvas.create_oval(
            cx - radius, cy - radius, cx + radius, cy + radius,
            fill="#1A1A2E", outline="#2A2A3C", width=1
        )

    def _draw_light_on(self, cx, cy, radius, color, glow_color, intensity=1.0):
        """绘制亮灯，支持呼吸效果 intensity 0-1"""
        # 混合颜色
        off_color = (0x1A, 0x1A, 0x2E)
        on_color = tuple(int(color[i:i+2], 16) for i in (1, 3, 5))
        glow_tuple = tuple(int(glow_color[i:i+2], 16) for i in (1, 3, 5))

        r = int(off_color[0] + (on_color[0] - off_color[0]) * intensity)
        g = int(off_color[1] + (on_color[1] - off_color[1]) * intensity)
        b = int(off_color[2] + (on_color[2] - off_color[2]) * intensity)

        gr = int(off_color[0] + (glow_tuple[0] - off_color[0]) * intensity)
        gg = int(off_color[1] + (glow_tuple[1] - off_color[1]) * intensity)
        gb = int(off_color[2] + (glow_tuple[2] - off_color[2]) * intensity)

        color_hex = f"#{r:02x}{g:02x}{b:02x}"
        glow_hex = f"#{gr:02x}{gg:02x}{gb:02x}"

        # 光晕
        glow_r = radius + int(3 * intensity)
        self.canvas.create_oval(
            cx - glow_r, cy - glow_r, cx + glow_r, cy + glow_r,
            fill="", outline=glow_hex, width=2
        )
        # 主体
        self.canvas.create_oval(
            cx - radius, cy - radius, cx + radius, cy + radius,
            fill=glow_hex, outline=""
        )
        self.canvas.create_oval(
            cx - radius + 1, cy - radius + 1, cx + radius - 1, cy + radius - 1,
            fill=color_hex, outline=""
        )
        # 高光
        highlight_r = int(radius * 0.5 * intensity)
        self.canvas.create_oval(
            cx - highlight_r, cy - highlight_r - 2,
            cx + highlight_r, cy + highlight_r - 2,
            fill="#FFFFFF", outline="", width=0
        )

    def _get_cycle_colors(self, progress):
        """根据循环进度 (0-1) 返回当前颜色，绿→黄→红→黄→绿"""
        green = ("#22C55E", "#16A34A")
        yellow = ("#FACC15", "#EAB308")
        red = ("#EF4444", "#DC2626")

        # 三角波: 0→1→0
        t = progress * 2 if progress < 0.5 else 2 - progress * 2

        if t < 0.5:
            p = t * 2
            c = self._lerp_color(green[0], yellow[0], p)
            g = self._lerp_color(green[1], yellow[1], p)
        else:
            p = (t - 0.5) * 2
            c = self._lerp_color(yellow[0], red[0], p)
            g = self._lerp_color(yellow[1], red[1], p)
        return c, g

    @staticmethod
    def _lerp_color(hex_a, hex_b, t):
        """在两个十六进制颜色之间线性插值"""
        ra, ga, ba = int(hex_a[1:3], 16), int(hex_a[3:5], 16), int(hex_a[5:7], 16)
        rb, gb, bb = int(hex_b[1:3], 16), int(hex_b[3:5], 16), int(hex_b[5:7], 16)
        r = int(ra + (rb - ra) * t)
        g = int(ga + (gb - ga) * t)
        b = int(ba + (bb - ba) * t)
        return f"#{r:02x}{g:02x}{b:02x}"

    def _update_display(self):
        self.canvas.delete("all")

        # 背景
        self._draw_rounded_rect(4, 4, self.width - 4, self.height - 4, 14,
                                fill="#1E1E2E", outline="#313244", width=1)

        if self.current_status == Status.IDLE:
            self._draw_light_on(self.light_x, self.light_y, self.light_r, "#22C55E", "#16A34A")

        elif self.current_status == Status.WORKING:
            color, glow = self._get_cycle_colors(self.color_progress)
            intensity = (math.sin(self.breath_progress * 2 * math.pi - math.pi / 2) + 1) / 2
            self._draw_light_on(self.light_x, self.light_y, self.light_r, color, glow, intensity)

        elif self.current_status == Status.COMPLETED:
            intensity = (math.sin(self.breath_progress * 2 * math.pi - math.pi / 2) + 1) / 2
            self._draw_light_on(self.light_x, self.light_y, self.light_r, "#22C55E", "#16A34A", intensity)

        elif self.current_status == Status.WAITING:
            if self.blink_state:
                self._draw_light_on(self.light_x, self.light_y, self.light_r, "#FACC15", "#EAB308")
            else:
                self._draw_light_off(self.light_x, self.light_y, self.light_r)

        elif self.current_status == Status.ERROR:
            intensity = (math.sin(self.breath_progress * 2 * math.pi - math.pi / 2) + 1) / 2
            self._draw_light_on(self.light_x, self.light_y, self.light_r, "#EF4444", "#DC2626", intensity)

        # 状态文字
        self.canvas.create_text(
            110, self.light_y,
            text=STATUS_NAMES[self.current_status],
            fill="#E0E0E0",
            font=("Segoe UI", 10, "bold"),
            anchor="center"
        )

    def _start_animation(self):
        if self.animation_id:
            self.root.after_cancel(self.animation_id)

        def animate():
            dt = 30  # ~33fps

            if self.current_status == Status.WORKING:
                self.color_progress += dt / COLOR_CYCLE_TIME
                if self.color_progress >= 1:
                    self.color_progress = 0
                self.breath_progress += dt / MARQUEE_ON_TIME
                if self.breath_progress >= 1:
                    self.breath_progress = 0

            elif self.current_status == Status.COMPLETED:
                self.breath_progress += dt / BREATH_CYCLE_TIME
                if self.breath_progress >= 1:
                    self.breath_progress = 0

            elif self.current_status == Status.WAITING:
                self.breath_progress += dt / (BLINK_ON_TIME if self.blink_state else BLINK_OFF_TIME)
                if self.breath_progress >= 1:
                    self.breath_progress = 0
                    self.blink_state = not self.blink_state

            elif self.current_status == Status.ERROR:
                self.breath_progress += dt / BREATH_CYCLE_TIME
                if self.breath_progress >= 1:
                    self.breath_progress = 0

            self._update_display()
            self.animation_id = self.root.after(dt, animate)

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
            self.breath_progress = 0
            self.color_progress = 0
            self.blink_state = True
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
                        'idle': Status.IDLE,
                        'working': Status.WORKING,
                        'waiting': Status.WAITING,
                        'error': Status.ERROR,
                        'completed': Status.COMPLETED,
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
            print("StatusLight HTTP Server running on http://127.0.0.1:51234")
            server.serve_forever()

        threading.Thread(target=run_server, daemon=True).start()

    def run(self):
        self.root.mainloop()


if __name__ == "__main__":
    StatusLightApp().run()
