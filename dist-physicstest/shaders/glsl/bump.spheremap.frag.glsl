uniform sampler2D DiffuseMap;
uniform sampler2D BumpMap;
uniform sampler2D EnvironmentMap;

varying vec3 toLight;
varying vec3 vertexpos;//for spheremapping
varying mat3 tbn;

vec2 SphereMap(const in vec3 U,const in vec3 N)
{
	vec3 R;

	R = reflect(U,N);

	R=normalize(transpose(tbn)*R);

	R.z += 1.0;
	R = normalize(R);
	return R.xy*0.5+0.5;
}

vec4 Light0()
{
	vec3 surfaceNormal = texture2D(BumpMap, gl_TexCoord[0].xy).xyz;
        surfaceNormal = (surfaceNormal - 0.5) * 2.0;
	surfaceNormal=vec3(0.0,0.0,1.0);

	float NdotL = max(0.0, dot(surfaceNormal,normalize(toLight)));
	
	vec4 diffuse = texture2D(DiffuseMap, gl_TexCoord[0].xy);

	//float NdotV = max(0.0, dot(normalize(gl_NormalMatrix*surfaceNormal),normalize(-vertexpos)));


	vec4 refl=texture2D(EnvironmentMap, SphereMap(normalize(tbn*vertexpos),normalize(surfaceNormal)));


	vec4 color = refl;
	//vec4 color = refl;
	color.a=1.0;
	return color;
}


void main()
{

	gl_FragColor = Light0();
	gl_FragColor.a = 1.0;
}
