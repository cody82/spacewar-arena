varying vec3 normal;
varying vec3 view;
//varying vec4 world;
//varying vec3 worldn;

void main()
{	
	gl_Position = ftransform();
	normal=gl_Normal*gl_NormalMatrix;
view = -vec3(normalize(gl_ModelViewMatrix * gl_Vertex));

}