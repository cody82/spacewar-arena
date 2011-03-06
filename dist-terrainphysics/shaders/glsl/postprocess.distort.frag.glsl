uniform sampler2D Texture;

uniform vec2 WindowSize;
uniform float Time;

void main()
{
	const float invsize=50.0;
	float t=(cos(Time*10.0)+1.0)*0.5;
	const vec2 center=vec2(0.5,0.5);

	vec2 dist=(gl_TexCoord[0].st-center);

	float len=length(dist);

	dist=dist/(1.0+t*(1.0/(len*len*invsize)));

	vec2 tex=center+dist;

	vec4 c1=texture2D(Texture,tex);



	gl_FragColor=gl_Color * c1;

	gl_FragColor[3]=1.0;

}
