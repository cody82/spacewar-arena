SHADERTEXT

fragmentshader:
//uniform sampler2D Texture;
//uniform vec2 WindowSize;
//uniform float Time;

void main()
{
	//vec4 emissive =texture2D(Texture, gl_TexCoord[0].xy);

float x=length(gl_TexCoord[0].xy-vec2(0.5,0.5))*2.0;
float brightness=1.0-x*x;

	gl_FragColor=vec4(gl_Color.xyz,brightness);

}

vertexshader:
//uniform float WorldSize;
//uniform vec3 Attenuation;

void main()
{
        //gl_TexCoord[0] = gl_TextureMatrix[0] * gl_MultiTexCoord0;
	gl_Position = ftransform();
	//vec3 vertexpos=(gl_ModelViewMatrix*gl_Vertex).xyz;
	//float d=length(vertexpos);
	gl_PointSize=gl_MultiTexCoord0.s;
	gl_FrontColor=gl_Color;
}