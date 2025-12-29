shader_type canvas_item;

// 矩形尺寸 (单位：像素)
uniform vec2 size = vec2(100.0, 100.0);
// 边框宽度
uniform float border_width = 3.0;
// 边框颜色
uniform vec4 border_color : source_color = vec4(1.0, 0.0, 0.0, 0.9);
// 填充颜色
uniform vec4 fill_color : source_color = vec4(1.0, 0.2, 0.2, 0.2);
// 脉冲速度
uniform float pulse_speed = 1.5;

void fragment() {
    // 将UV从[0,1]转换到局部坐标[-size/2, size/2]
    vec2 pos = (UV - 0.5) * size;
    vec2 half_size = size * 0.5;

    // 脉冲效果
    float pulse = 0.7 + 0.3 * sin(TIME * pulse_speed);

    // 检查是否在矩形内部（排除边框）
    bool inside = abs(pos.x) < (half_size.x - border_width * 0.5) &&
                  abs(pos.y) < (half_size.y - border_width * 0.5);

    // 检查是否在边框区域
    bool border = abs(pos.x) < half_size.x &&
                  abs(pos.y) < half_size.y &&
                  !inside;

    if (inside) {
        // 内部填充区域
        COLOR = fill_color;
        COLOR.a *= 0.5 * pulse; // 填充区域更透明
    } else if (border) {
        // 边框区域
        COLOR = border_color;
        COLOR.a *= pulse;
    } else {
        // 外部区域 - 透明
        COLOR = vec4(0.0);
    }
}
