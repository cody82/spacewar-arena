SHADERTEXT

vertexshader:
void main()
{	
	gl_Position = ftransform();
	gl_FrontColor=vec4(gl_Color.xyz,0.6);
}
fragmentshader:
uniform vec4 Color;

void main()
{
	gl_FragColor = Color;
}