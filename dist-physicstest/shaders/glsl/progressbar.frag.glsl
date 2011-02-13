//uniform sampler2D Texture;
uniform vec2 WindowSize;
uniform float Time;
uniform float Value;
uniform vec4 BarColor;
//uniform vec4 BackColor;

void main()
{
//const float Value=0.25;
//const vec4 FrontColor=vec4(1.0,0.0,0.0,1.0);
//const vec4 BackColor=vec4(0.0,1.0,0.0,1.0);


	//vec2 realpos=WindowSize*gl_TexCoord[0].st;

	float d=max(0.0,sign(gl_TexCoord[0].s-Value));

	vec4 color=d*gl_Color+(1.0-d)*BarColor;

	//color.a*=(cos(Time+gl_TexCoord[0].s*10.0)+1.0)*0.5;
	gl_FragColor=color;

	//gl_FragColor[3]=min(min(alpha[0],alpha[1]),1.0)*gl_Color[3]*alpha2;

}
