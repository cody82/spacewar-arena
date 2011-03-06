SHADERTEXT

vertexshader:
uniform float Time;
void main()
{	
        gl_TexCoord[0] = gl_MultiTexCoord0;
        
	vec4 pos=gl_Vertex;

	pos=vec4(pos.x,pos.y,pos.z+cos(pos.x+Time*5.0)*5.0,pos.w);


        gl_Position = gl_ModelViewProjectionMatrix * pos;
}

fragmentshader:
uniform sampler2D EmissiveMap;
uniform vec4 Color;
void main()
{
	vec4 emissive =texture2D(EmissiveMap, gl_TexCoord[0].xy);

	gl_FragColor = Color * emissive;
}