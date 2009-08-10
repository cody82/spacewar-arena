//uniform sampler2D Texture;
uniform vec2 WindowSize;
uniform float Time;
uniform float Value1;
uniform float Value2;
uniform vec4 BarColor;

void main()
{
//const float Value=0.25;
//const vec4 FrontColor=vec4(1.0,0.0,0.0,1.0);
//const vec4 BackColor=vec4(0.0,1.0,0.0,1.0);
	float coord=gl_TexCoord[0].t;

	float d1=max(0.0,sign(coord-Value1));
	float d2=max(0.0,sign(Value2-coord));
	float a=d1*d2;

	vec4 color=(1.0-a)*gl_Color+a*BarColor;

	gl_FragColor=color;

}
