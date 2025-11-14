shader_type canvas_item;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec2 uv = vec2(floor(UV.x*1280.)/1280., floor(UV.y*720.)/720.);
	vec3 color = vec3(mod(uv.x*200. + TIME/8. + sin(uv.y*20.), 2.));
	color = step(.2, color);
	COLOR = vec4(color.r==1.?vec3(28./255., 34./255., 48./255.):vec3(106./255., 55./255., 49./255.), .8);
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
