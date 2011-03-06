SHADERTEXT

vertexshader:
uniform vec4 hitpos;
uniform float strength;

void main()
{
	gl_Position = ftransform();
	gl_TexCoord[0] = gl_MultiTexCoord0;

	vec4 color=vec4(0,0,0,1);
	//vec4 hitpos=vec4(0,0,100,1);
	vec4 v=gl_Vertex;
	v-=hitpos;
	float dist=length(v);
	float a=1.0 / (dist * dist) * 1000.0;
	color=vec4(a,a,a,1);
	gl_FrontColor = color * strength;

}
fragmentshader:
uniform sampler2D tex;

void main()
{
	vec4 color = texture2D(tex,gl_TexCoord[0].st);
	color[3]=1.0;
	gl_FragColor = gl_Color * color;
}