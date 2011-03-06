//uniform sampler2D Texture;
uniform vec2 WindowSize;
uniform float Time;
uniform vec4 Scissor;

void main()
{
//if(mod(floor(gl_FragCoord.y),2.0)>0.5)
//	gl_FragColor=vec4(1.0,1.0,1.0,1.0);
//else
//	gl_FragColor=vec4(0.0,1.0,1.0,1.0);

	vec2 realpos=WindowSize*gl_TexCoord[0].st;


	const float border=1.0;
	//const float inv=1.0/border;

	//vec4 d=vec4(length(realpos-WindowSize),length(realpos-vec2(0.0,0.0)),length(realpos-vec2(0.0,WindowSize.y)),length(realpos-vec2(WindowSize.x,0.0)));
	vec4 d=vec4(realpos.x-0.0,realpos.y-0.0,realpos.x-WindowSize.x,realpos.y-WindowSize.y);
	d=abs(d);

	float mind=min(d[0],d[1]);
	mind=min(mind,d[2]);
	mind=min(mind,d[3]);


	float a=clamp(mind-border,0.0,1.0);


	vec2 a3=vec2(realpos.y-Scissor[1],Scissor[3]-realpos.y);
	a3=max(vec2(0.0),sign(a3));
	float a2=a3[0]*a3[1];

	gl_FragColor=gl_Color;

	gl_FragColor.a=a2*((1.0-a)*gl_Color.a+a*(gl_Color.a*0.5));

}
