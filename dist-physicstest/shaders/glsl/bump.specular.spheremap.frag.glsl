uniform sampler2D DiffuseMap;
uniform sampler2D BumpMap;
uniform sampler2D EnvironmentMap;
uniform sampler2D SpecularMap;
uniform sampler2D EmissiveMap;

varying vec3 toLight;
varying vec3 vertexpos;

vec2 SphereMap(const in vec3 U,const in vec3 N)
{
	vec3 R;

	R = reflect(U,N);

	R.z += 1.0;
	R = normalize(R);
	return R.xy*0.5+0.5;
}

vec4 Light0()
{
	vec3 surfaceNormal = texture2D(BumpMap, gl_TexCoord[0].xy).xyz;
        surfaceNormal = (surfaceNormal - 0.5) * 2.0;
	//surfaceNormal=vec3(0.0,0.0,1.0);
	
vec3 Eye             = normalize(-vertexpos);
 vec3 Reflected       = normalize( 2.0 * dot(surfaceNormal , toLight) *  surfaceNormal - toLight);



	vec4 diffuse = texture2D(DiffuseMap, gl_TexCoord[0].xy) * max(0.0, dot(surfaceNormal,normalize(toLight)));
	vec4 specular = texture2D(SpecularMap, gl_TexCoord[0].xy) * gl_LightSource[0].specular * pow(max(dot(Reflected, Eye), 0.0), gl_FrontMaterial.shininess);
	vec4 emissive =texture2D(EmissiveMap, gl_TexCoord[0].xy);

	vec4 refl=texture2D(EnvironmentMap, SphereMap(normalize(vertexpos),normalize(gl_NormalMatrix*surfaceNormal)));


	vec4 color = diffuse + emissive + specular +refl*0.25;
	color.a=1.0;
	return color;
}


void main()
{

	gl_FragColor = Light0();
	gl_FragColor.a = 1.0;
}
