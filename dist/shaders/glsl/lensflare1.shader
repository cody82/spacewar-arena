SHADERTEXT

vertexshader:
//uniform vec4 WorldOrigin;

//varying vec4 Origin;

//uniform float Time;

void main()
{	
	//vec4 WorldOrigin=vec4(100.5,1.0,0.0,1.0);
	gl_Position = gl_Vertex;
	//gl_Position = ftransform();
	gl_FrontColor=gl_Color;
      gl_TexCoord[0] = gl_Vertex;
      //gl_TexCoord[0] = gl_MultiTexCoord0;
	//Origin=(gl_ModelViewProjectionMatrix*WorldOrigin);

	//Origin=vec4(0.0,0.0,0.0,1.0);
}


fragmentshader:
//varying vec4 Origin;

//uniform float Time;
uniform vec2 Position2D;
uniform float Age;

void main()
{
const vec2 Center=vec2(0.0,0.0);
const vec4 Positions=vec4(0.0,0.55,1.30,2.0);


	vec2 o=Position2D;
	//vec2 o=Origin.xy;
	//vec2 o=vec2(1.0,1.0);

	vec2 Direction=Center-o;
	//vec2 OrthoDirection=vec2(Direction.y,-Direction.x);
	vec4 factor;

	vec2 flare0pos=o+Direction*Positions[0];
	factor[0]=20.0;
	const vec4 flare0color=vec4(1.0,1.0,1.0,1.0);

	vec2 flare1pos=o+Direction*Positions[1];
	factor[1]=12.0;
	const vec4 flare1color=vec4(0.6,0.6,0.2,1.0);

	vec2 flare2pos=o+Direction*Positions[2];
	factor[2]=5.0;
	const vec4 flare2color=vec4(0.5,0.5,0.0,1.0);

	vec2 flare3pos=o+Direction*Positions[3];
	factor[3]=2.0;
	const vec4 flare3color=vec4(0.4,0.10,0.0,1.0);


	vec4 d=vec4(
		length(gl_TexCoord[0].xy-flare0pos),
		length(gl_TexCoord[0].xy-flare1pos),
		length(gl_TexCoord[0].xy-flare2pos),
		length(gl_TexCoord[0].xy-flare3pos)
		);


	vec4 d2=d*factor;

	d2=min(vec4(1.0),1.0/(d2));

	float sum=d2[0]+d2[1]+d2[2]+d2[3];
	//d/=sum;
	d2*=0.9+(sin(d*20.0)+1.0)*0.05;

	float a=max(1.0-max(0.0,abs(Age-1.0))*0.5,0.0);
	gl_FragColor = (d2[0]*flare0color+d2[1]*flare1color+d2[2]*flare2color+d2[3]*flare3color)*a;

	gl_FragColor.a=sum;
}