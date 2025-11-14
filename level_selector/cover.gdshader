shader_type canvas_item;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec2 uv = SCREEN_UV;
	uv = vec2(floor(uv.x*500.)/500., floor(uv.y*500.)/500.);
	vec3 color = vec3(28./255., 34./255., 48./255.);
	color = vec3(sin(mod(uv.y*5.+uv.x*2. + TIME/2.,1.)));
	color = step(.5, color);
	COLOR.rgb = color.r==0.?vec3(28./255., 34./255., 48./255.):vec3(149./255.,57./255.,24./255.);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
