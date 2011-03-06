uniform sampler2D Texture;

uniform vec2 WindowSize;
uniform float Time;

void main()
{
	float strength=(cos(Time*10.0)+1.0)*0.5;
	const float dist=0.03;
	//float strength=cos(Time*10.0)*0.5;
	vec4 c1=texture2D(Texture,gl_TexCoord[0].st+vec2(dist*strength,0.0));
	vec4 c2=texture2D(Texture,gl_TexCoord[0].st+vec2(-dist*strength,0.0));

	vec4 c3=texture2D(Texture,gl_TexCoord[0].st+vec2(0.0,dist*strength));
	vec4 c4=texture2D(Texture,gl_TexCoord[0].st+vec2(0.0,-dist*strength));

	gl_FragColor=gl_Color * (c1+c2+c3+c4) * 0.25;

	gl_FragColor[3]=1.0;

}
