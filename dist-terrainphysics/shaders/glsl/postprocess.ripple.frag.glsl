uniform sampler2D Texture;

uniform vec2 WindowSize;
uniform float Time;

void main()
{
	float t=(cos(Time*10.0)+1.0)*0.5;
	//const vec2 center=vec2(0.5,0.5);

	//vec2 dist=(gl_TexCoord[0].st-center);
	//float len=length(dist);
	
	float d=(cos(Time+gl_TexCoord[0].s*50.0))*0.01;
	float d2=(cos(Time+gl_TexCoord[0].t*40.0))*0.01;


	//dist=dist*(1.0-min(d,1.0));

	//vec2 tex=center+dist;

	vec4 c=gl_Color*texture2D(Texture,gl_TexCoord[0].st+vec2(d2,d));

	gl_FragColor=c;
	gl_FragColor[3]=1.0;

}
