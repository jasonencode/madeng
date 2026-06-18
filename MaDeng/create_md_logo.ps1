# PowerShell script to create MD logo PNG
Add-Type -AssemblyName System.Drawing

# Create bitmap
$width = 100
$height = 100
$bmp = New-Object System.Drawing.Bitmap($width, $height)
$graphics = [System.Drawing.Graphics]::FromImage($bmp)

# Set background color (transparent or solid)
$graphics.Clear([System.Drawing.Color]::Transparent)

# Draw text "MD" with gradient
$font = New-Object System.Drawing.Font("Arial", 50, [System.Drawing.FontStyle]::Bold)

# Create gradient brush (green -> yellow -> red)
$point1 = New-Object System.Drawing.Point(0, 0)
$point2 = New-Object System.Drawing.Point(0, $height)
$brush = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
    $point1,
    $point2,
    [System.Drawing.Color]::Green,
    [System.Drawing.Color]::Red
)

# Set color blend for green -> yellow -> red
$blend = New-Object System.Drawing.Drawing2D.ColorBlend(3)
$blend.Colors[0] = [System.Drawing.Color]::Green
$blend.Colors[1] = [System.Drawing.Color]::Yellow
$blend.Colors[2] = [System.Drawing.Color]::Red
$blend.Positions[0] = 0.0
$blend.Positions[1] = 0.5
$blend.Positions[2] = 1.0
$brush.InterpolationColors = $blend

# Center the text
$stringSize = $graphics.MeasureString("MD", $font)
$x = ($width - $stringSize.Width) / 2
$y = ($height - $stringSize.Height) / 2

$graphics.DrawString("MD", $font, $brush, $x, $y)

# Save as PNG
$bmp.Save("d:\Develops\VibeCoding\MaDeng\logo.png", [System.Drawing.Imaging.ImageFormat]::Png)

# Cleanup
$graphics.Dispose()
$bmp.Dispose()

Write-Host "Created md_logo.png"