//uniform sampler2D Texture;
uniform vec2 WindowSize;
uniform float Time;
uniform vec4 Gradiant;

void main()
{
	gl_FragColor=gl_Color;

	vec2 g=gl_TexCoord[0].xy*Gradiant.xy+Gradiant.zw;

	gl_FragColor[3]=gl_Color[3]*g.x*g.x*g.y*g.y;
}
