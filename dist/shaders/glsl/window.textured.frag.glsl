uniform sampler2D Texture;
uniform vec2 WindowSize;
uniform float Time;

void main()
{

	vec2 realpos=WindowSize*gl_TexCoord[0].st;


	const float border=5.0;
	const float inv=1.0/border;

	vec2 alpha=min(realpos*inv,(WindowSize-realpos)*inv);

	float alpha2=(cos((Time+realpos.y)*3.0)+1.5)*0.5;

	vec4 c=gl_Color*texture2D(Texture,gl_TexCoord[0].st);
	gl_FragColor=c;
	//gl_FragColor=vec4(length(c.xyz));
	//gl_FragColor=vec4(1.0,1.0,1.0,0.0)-c;

	gl_FragColor[3]=min(min(alpha[0],alpha[1]),1.0)*gl_Color[3]*alpha2;

}
