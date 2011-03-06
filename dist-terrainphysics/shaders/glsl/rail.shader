SHADERTEXT

fragmentshader:
uniform sampler2D EmissiveMap;
uniform float Alpha;

void main()
{
	vec4 emissive =texture2D(EmissiveMap, gl_TexCoord[0].xy);
	emissive*=Alpha;

	gl_FragColor = emissive;
}

vertexshader:
void main()
{	
        gl_TexCoord[0] = gl_TextureMatrix[0] * gl_MultiTexCoord0;
        
        gl_Position = ftransform();                       	
} 