shader_type canvas_item;

uniform float tile_size = 32.0;     // 每块的像素大小
uniform float shake_amount = 2.0;   // 抖动强度(像素)
uniform float speed = 2.0;          // 抖动速度

uniform sampler2D screen_texture: hint_screen_texture, repeat_disable, filter_linear;

group_uniforms Internal;

uniform vec2 resolution;

group_uniforms;

float gold_noise(vec2 xy, float seed) {
    return fract(tan(distance(xy * 1.61803398874989484820459, xy) * seed) * xy.x);
}

void fragment() {
    vec2 uv = SCREEN_UV;

    vec2 grid = floor(uv * resolution / tile_size);

    float rnd = gold_noise(grid + floor(TIME * speed), 1.0);

    // Calculate offset direction
    float angle = rnd * 6.28318; // 2*pi
    vec2 offset = vec2(cos(angle), sin(angle)) * shake_amount * rnd;

    vec2 offset_uv = offset * SCREEN_PIXEL_SIZE.xy;

    COLOR = texture(screen_texture, uv + offset_uv);
}
