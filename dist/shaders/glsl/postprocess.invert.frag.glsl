uniform sampler2D Texture;

uniform vec2 WindowSize;
uniform float Time;

void main()
{
	vec4 c=gl_Color*texture2D(Texture,gl_TexCoord[0].st);
	gl_FragColor=vec4(1.0,1.0,1.0,0.0)-c;
	gl_FragColor[3]=1.0;

}
