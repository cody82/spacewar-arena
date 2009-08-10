varying vec3 normal;
varying vec3 vertexpos;

void main()
{	
	gl_Position = ftransform();
	normal=(gl_NormalMatrix*gl_Normal).xyz;
	vertexpos=(gl_ModelViewMatrix*gl_Vertex).xyz;
}