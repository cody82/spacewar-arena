SHADERTEXT

vertexshader:
void main()
{	
        gl_TexCoord[0] = gl_TextureMatrix[0] * gl_MultiTexCoord0;
	gl_Position = ftransform();
	gl_FrontColor=gl_Color;
}
fragmentshader:
uniform sampler2D EmissiveMap;
uniform vec4 Color;
void main()
{
	vec4 emissive =texture2D(EmissiveMap, gl_TexCoord[0].xy);

	gl_FragColor = Color * emissive;
}