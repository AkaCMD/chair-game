shader_type canvas_item;

// 矩形尺寸 (单位：像素)
uniform vec2 size = vec2(100.0, 100.0);
// 边框宽度
uniform float border_width = 2.0;
// 边框颜色
uniform vec4 border_color : source_color = vec4(1.0, 0.0, 0.0, 1.0);
// 填充颜色
uniform vec4 fill_color : source_color = vec4(1.0, 0.0, 0.0, 0.2);
// 脉冲速度
uniform float pulse_speed = 2.0;
// 脉冲强度 (透明度变化范围)
uniform float pulse_strength = 0.5;

// 标准矩形SDF（有符号距离场）
float sdf_rectangle(vec2 pos, vec2 half_size) {
    vec2 d = abs(pos) - half_size;
    // 标准SDF公式：内部为负值，外部为正值
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

void fragment() {
    // 将UV从[0,1]转换到[-size/2, size/2]
    vec2 pos = (UV - 0.5) * size;
    vec2 half_size = size * 0.5;

    // 计算有符号距离
    float dist = sdf_rectangle(pos, half_size);

    // 使用SDF计算边框和填充
    float border_alpha = 1.0 - smoothstep(-border_width * 0.5, border_width * 0.5, abs(dist));
    float fill_alpha = 1.0 - smoothstep(0.0, max(size.x, size.y) * 0.01, dist);

    // 脉冲效果 - 透明度随时间变化
    float pulse = 0.5 + 0.5 * sin(TIME * pulse_speed);
    float alpha_mult = 1.0 - pulse_strength * pulse;

    // 混合边框和填充颜色
    vec4 final_color = mix(fill_color, border_color, border_alpha);

    // 应用透明度：边框全透明，填充半透明，并添加脉冲效果
    if (dist < -border_width * 0.5) {
        // 内部区域（填充）
        final_color.a *= fill_color.a * alpha_mult * 0.5;
    } else if (abs(dist) <= border_width * 0.5) {
        // 边框区域
        final_color.a *= border_alpha * alpha_mult;
    } else {
        // 外部区域
        final_color.a = 0.0;
    }

    COLOR = final_color;
}
