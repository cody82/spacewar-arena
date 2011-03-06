uniform sampler2D DiffuseMap;
uniform sampler2D BumpMap;
uniform sampler2D SpecularMap;
uniform sampler2D EmissiveMap;

varying vec3 toLight;
varying vec3 vertexPos;
varying vec3 vertexPosTangentSpace;


vec4 Light0()
{
	vec3 surfaceNormal = texture2D(BumpMap, gl_TexCoord[0].xy).xyz;
        surfaceNormal = (surfaceNormal - 0.5) * 2.0;

 //vec3 Eye             = normalize(-vertexPos);
 vec3 Eye             = normalize(-vertexPosTangentSpace);
 vec3 Reflected       = normalize( 2.0 * dot(surfaceNormal , toLight) *  surfaceNormal - toLight);
 //vec3 Reflected       = normalize( 2.0 * dot(normal, toLightCamSpace) *  normal - toLightCamSpace);


	vec4 diffuse = texture2D(DiffuseMap, gl_TexCoord[0].xy) * max(0.0, dot(surfaceNormal,normalize(toLight)));
	vec4 specular = texture2D(SpecularMap, gl_TexCoord[0].xy) * gl_LightSource[0].specular * pow(max(dot(Reflected, Eye), 0.0), gl_FrontMaterial.shininess);
	vec4 emissive =texture2D(EmissiveMap, gl_TexCoord[0].xy);

	//vec4 diffuse = vec4(1.0,1.0,1.0,1.0);

	vec4 color = diffuse + specular +emissive;
	color.a=1.0;
	return color;
}


void main()
{
	//vec4 c=texture2D(DiffuseMap, gl_TexCoord[0].xy);
	//c.a=1.0;
	gl_FragColor = Light0();
}
