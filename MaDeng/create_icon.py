from PIL import Image, ImageDraw, ImageFont

def create_icon():
    size = 256
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # 背景圆角矩形
    bg_color = (30, 30, 46, 230)
    draw.rounded_rectangle([(0, 0), (size-1, size-1)], radius=40, fill=bg_color)

    light_radius = 35
    center_y = 100  # 灯的中心 Y 坐标（上移，为文字留空间）
    positions = [64, 128, 192]

    # 绿灯 - 亮
    green_color = (34, 197, 94)
    green_glow = (22, 163, 74)
    draw.ellipse([positions[0]-light_radius-3, center_y-light_radius-3,
                  positions[0]+light_radius+3, center_y+light_radius+3],
                 fill=green_glow)
    draw.ellipse([positions[0]-light_radius, center_y-light_radius,
                  positions[0]+light_radius, center_y+light_radius],
                 fill=green_color)
    draw.ellipse([positions[0]-15, center_y-20, positions[0]+15, center_y+10],
                 fill=(74, 222, 128))

    # 黄灯 - 亮
    yellow_color = (250, 204, 21)
    yellow_glow = (234, 179, 8)
    draw.ellipse([positions[1]-light_radius-3, center_y-light_radius-3,
                  positions[1]+light_radius+3, center_y+light_radius+3],
                 fill=yellow_glow)
    draw.ellipse([positions[1]-light_radius, center_y-light_radius,
                  positions[1]+light_radius, center_y+light_radius],
                 fill=yellow_color)
    draw.ellipse([positions[1]-15, center_y-20, positions[1]+15, center_y+10],
                 fill=(253, 224, 71))

    # 红灯 - 亮
    red_color = (239, 68, 68)
    red_glow = (220, 38, 38)
    draw.ellipse([positions[2]-light_radius-3, center_y-light_radius-3,
                  positions[2]+light_radius+3, center_y+light_radius+3],
                 fill=red_glow)
    draw.ellipse([positions[2]-light_radius, center_y-light_radius,
                  positions[2]+light_radius, center_y+light_radius],
                 fill=red_color)
    draw.ellipse([positions[2]-15, center_y-20, positions[2]+15, center_y+10],
                 fill=(248, 113, 113))

    # MaDeng 文字
    text = "MaDeng"
    text_color = (224, 224, 224, 255)  # #E0E0E0
    text_y = 180  # 文字 Y 坐标

    # 尝试加载字体，如果没有则使用默认字体
    try:
        font = ImageFont.truetype("arial.ttf", 36)
    except:
        try:
            font = ImageFont.truetype("C:/Windows/Fonts/arial.ttf", 36)
        except:
            font = ImageFont.load_default()

    # 计算文字宽度并居中
    bbox = draw.textbbox((0, 0), text, font=font)
    text_width = bbox[2] - bbox[0]
    text_x = (size - text_width) // 2

    draw.text((text_x, text_y), text, fill=text_color, font=font)

    # 保存为 ICO 格式
    icon_sizes = [(16, 16), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)]
    img.save('icon.ico', format='ICO', sizes=icon_sizes)
    print("图标已创建: icon.ico")

if __name__ == "__main__":
    create_icon()
