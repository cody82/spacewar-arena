SHADERTEXT

vertexshader:
uniform mat4 Bones[60];
void main()
{
        gl_TexCoord[0] = gl_MultiTexCoord0;
        vec4 pos = Bones[int(gl_MultiTexCoord0[2])] * gl_Vertex;
        //vec4 pos = Bones[1] * gl_Vertex;
        gl_Position = gl_ModelViewProjectionMatrix * pos;
	//gl_FrontColor=vec4(gl_Color.xyz,0.6);
}


fragmentshader:
uniform sampler2D Texture;

void main()
{
	vec4 emissive =texture2D(Texture, gl_TexCoord[0].xy);

	gl_FragColor = emissive;
}