//uniform sampler2D Texture;
uniform vec2 WindowSize;
uniform float Time;

void main()
{
//if(mod(floor(gl_FragCoord.y),2.0)>0.5)
//	gl_FragColor=vec4(1.0,1.0,1.0,1.0);
//else
//	gl_FragColor=vec4(0.0,1.0,1.0,1.0);

	vec2 realpos=WindowSize*gl_TexCoord[0].st;


	const float border=8.0;
	const float inv=1.0/border;

	vec2 alpha=min(realpos*inv,(WindowSize-realpos)*inv);

	float alpha2=(cos((Time+realpos.y)*3.0)+1.5)*0.5;

	vec2 a=vec2(realpos.y-Scissor[1],Scissor[3]-realpos.y);
	a=max(vec2(0.0),sign(a));
	float a2=a[0]*a[1];

	gl_FragColor=gl_Color;

	gl_FragColor[3]=a2*min(min(alpha[0],alpha[1]),1.0)*gl_Color[3]*alpha2;

}
