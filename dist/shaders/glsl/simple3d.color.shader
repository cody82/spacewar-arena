SHADERTEXT

vertexshader:
void main()
{	
	gl_Position = ftransform();
	gl_FrontColor=gl_Color;
	gl_BackColor=gl_Color;
}
fragmentshader:

void main()
{
	gl_FragColor = gl_Color;
}