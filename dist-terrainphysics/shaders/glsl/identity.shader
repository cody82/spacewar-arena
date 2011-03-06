SHADERTEXT

vertexshader:
void main()
{	
	gl_Position = gl_Vertex;
	gl_FrontColor=gl_Color;
       gl_TexCoord[0] = gl_MultiTexCoord0;
}
fragmentshader:
void main()
{
//	gl_FragColor = gl_Color;

	float d=length(gl_TexCoord[0].xy-vec2(0.0,0.0))*5.0;

	gl_FragColor = (1.0/d)*vec4(1.0,1.0,1.0,1.0);
	gl_FragColor.a=1.0;
}