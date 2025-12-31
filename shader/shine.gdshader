shader_type canvas_item;

/*
  玻璃闪光
*/

uniform float progress : hint_range(-1.0, 2.0) = -1.0;
uniform float width : hint_range(0.01, 0.5) = 0.10;
uniform float angle = 0.785398;
uniform vec4 shine_color : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float intensity : hint_range(0.0, 5.0) = 1.2;

void fragment() {
    vec4 base = texture(TEXTURE, UV);

    float s = sin(angle);
    float c = cos(angle);
    vec2 ruv = vec2(c * UV.x - s * UV.y, s * UV.x + c * UV.y);

    float d = abs(ruv.x - progress);

    float shine = 1.0 - smoothstep(0.0, width, d);

    vec3 add = shine * shine_color.rgb * intensity;
    COLOR = vec4(base.rgb + add, base.a);
}
