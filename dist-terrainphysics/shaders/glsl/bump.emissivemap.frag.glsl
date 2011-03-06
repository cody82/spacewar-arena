uniform sampler2D DiffuseMap;
uniform sampler2D BumpMap;
uniform sampler2D EmissiveMap;

varying vec3 toLight;

vec4 Light0()
{
	vec3 surfaceNormal = texture2D(BumpMap, gl_TexCoord[0].xy).xyz;
        surfaceNormal = (surfaceNormal - 0.5) * 2.0;
	//surfaceNormal=vec3(0.0,0.0,1.0);

	float NdotL = max(0.0, dot(surfaceNormal,normalize(toLight)));
	
	vec4 diffuse = vec4(1.0,1.0,1.0,1.0);

	vec4 color = diffuse * NdotL;
	color.a=1.0;
	return color;
}


void main()
{
	vec4 c=texture2D(DiffuseMap, gl_TexCoord[0].xy);
	vec4 e=texture2D(EmissiveMap, gl_TexCoord[0].xy);
	c.a=1.0;
	gl_FragColor = Light0()*c+e;
}
