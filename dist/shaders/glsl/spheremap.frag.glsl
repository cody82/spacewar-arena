uniform sampler2D EnvironmentMap;

varying vec3 normal;
varying vec3 vertexpos;

vec2 SphereMap(const in vec3 U,const in vec3 N)
{
	vec3 R;

	R = reflect(U,N);

	R.z += 1.0;
	R = normalize(R);
	return R.xy*0.5+0.5;
}

void main()
{
	float NdotL = max(0.0, dot(normal,normalize(-vertexpos)));

	gl_FragColor = (1.0-NdotL)*texture2D(EnvironmentMap, SphereMap(normalize(vertexpos),normalize(normal)));
	gl_FragColor.a=1.0;
}