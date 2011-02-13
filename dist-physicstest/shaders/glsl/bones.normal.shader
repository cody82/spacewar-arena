SHADERTEXT

vertexshader:
uniform mat4 Bones[60];
void main()
{
        vec4 pos = Bones[int(gl_MultiTexCoord0[0])] * gl_Vertex;

        gl_Position = gl_ModelViewProjectionMatrix * pos;
	gl_FrontColor=gl_Color;
}


fragmentshader:

void main()
{
	gl_FragColor = gl_Color;
}