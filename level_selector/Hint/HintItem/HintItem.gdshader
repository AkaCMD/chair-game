shader_type canvas_item;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	vec2 uv = UV;
	uv = vec2(round(uv.x*40.)/40., round(uv.y*40.)/40.);
	vec3 color = vec3(28./255.,34./255.,48./255.);
	vec3 alpha = 1. - vec3(pow(distance(uv, vec2(.5)),1));
	alpha = step(.5, alpha);
	COLOR.rgb = color;
	COLOR.a = alpha.r;
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
