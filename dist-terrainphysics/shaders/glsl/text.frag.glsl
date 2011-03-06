uniform sampler2D Texture;
uniform vec4 Scissor;

varying vec2 Position;

void main()
{
	//vec2 realpos=WindowSize*gl_TexCoord[0].st;

	vec2 a=vec2(Position.y-Scissor[1],Scissor[3]-Position.y);
	a=max(vec2(0.0),sign(a));
	float a2=a[0]*a[1];

	gl_FragColor = a2 * texture2D(Texture,gl_TexCoord[0].st) * gl_Color;
}