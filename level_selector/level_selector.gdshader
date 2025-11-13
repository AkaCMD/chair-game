shader_type canvas_item;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec2 uv = vec2(floor(UV.x*1280.)/1280., floor(UV.y*1280.)/1280.);
	vec3 color = vec3(mod(uv.x*80. + TIME/8. + sin(uv.y*20.), 1));
	color = step(mod(pow(sin(uv.y*20.),3.),1.), color);
	COLOR = vec4(color.r==1.?vec3(28./255., 34./255., 48./255.):vec3(149./255., 57./255., 24./255.), 1.);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
