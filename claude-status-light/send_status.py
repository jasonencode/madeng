#!/usr/bin/env python3
"""
发送状态到 Claude Status Light 的辅助脚本
用法: python send_status.py <status>
状态: idle, working, waiting, error
"""

import sys
import requests


def send_status(status: str):
    """发送状态到指示灯应用"""
    valid_statuses = ["idle", "working", "waiting", "error"]

    if status not in valid_statuses:
        print(f"无效状态: {status}")
        print(f"可用状态: {', '.join(valid_statuses)}")
        sys.exit(1)

    try:
        response = requests.post(
            "http://127.0.0.1:51234",
            json={"status": status},
            timeout=2
        )
        if response.ok:
            print(f"已发送状态: {status}")
        else:
            print(f"发送失败: {response.text}")
    except requests.ConnectionError:
        print("无法连接到状态指示灯应用，请确保应用正在运行")
        sys.exit(1)


if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("用法: python send_status.py <status>")
        print("状态: idle, working, waiting, error")
        sys.exit(1)

    send_status(sys.argv[1])
