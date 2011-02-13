uniform sampler2D Texture;
uniform vec2 WindowSize;
uniform float Time;
uniform vec4 Scissor;

void main()
{
	vec2 realpos=WindowSize*gl_TexCoord[0].st;

	vec2 a=vec2(realpos.y-Scissor[1],Scissor[3]-realpos.y);
	a=max(vec2(0.0),sign(a));
	float a2=a[0]*a[1];

	gl_FragColor=a2*gl_Color*texture2D(Texture,gl_TexCoord[0].st);


}
