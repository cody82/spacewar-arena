SHADERTEXT

vertexshader:
void main()
{	
	gl_TexCoord[0] = gl_MultiTexCoord0;
	gl_Position = ftransform();
	gl_FrontColor=gl_Color;
}
fragmentshader:
uniform sampler2D Texture;
uniform float Alpha;

void main()
{
	vec4 emissive =texture2D(Texture, gl_TexCoord[0].xy);

	gl_FragColor = Alpha * emissive;
}