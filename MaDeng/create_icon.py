from PIL import Image, ImageDraw

def create_icon():
    size = 256
    img = Image.new('RGBA', (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # 背景圆角矩形
    bg_color = (30, 30, 46, 230)
    draw.rounded_rectangle([(0, 0), (size-1, size-1)], radius=40, fill=bg_color)
    
    light_radius = 35
    center_y = size // 2
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
    
    icon_sizes = [(16, 16), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)]
    img.save('icon.ico', format='ICO', sizes=icon_sizes)
    print("图标已创建: icon.ico")

if __name__ == "__main__":
    create_icon()
