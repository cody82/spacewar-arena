uniform float WorldSize;
uniform vec3 Attenuation;

void main()
{
        gl_TexCoord[0] = gl_TextureMatrix[0] * gl_MultiTexCoord0;
	gl_Position = ftransform();
	vec3 vertexpos=(gl_ModelViewMatrix*gl_Vertex).xyz;
	float d=length(vertexpos);
	gl_PointSize=clamp((1.0/(d*Attenuation[2]))*WorldSize,1.0,250.0);
	gl_FrontColor=gl_Color;
}