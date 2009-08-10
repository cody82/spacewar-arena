SHADERTEXT

vertexshader:
void main()
{	
        gl_TexCoord[0] = gl_MultiTexCoord0;
	gl_Position = ftransform();
	gl_FrontColor=gl_Color;
}


fragmentshader:
uniform sampler2D DiffuseMap;
uniform sampler2D DetailMap;
void main()
{
	vec4 tex =texture2D(DiffuseMap, gl_TexCoord[0].xy);
	vec4 detail =texture2D(DetailMap, gl_TexCoord[0].xy);

	vec4 color=detail;
	//color.a=1.0;

	gl_FragColor = color;
}