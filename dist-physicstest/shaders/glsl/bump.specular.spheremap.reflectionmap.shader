SHADERTEXT

fragmentshader:
uniform sampler2D DiffuseMap;
uniform sampler2D BumpMap;
uniform sampler2D EnvironmentMap;
uniform sampler2D SpecularMap;
uniform sampler2D EmissiveMap;
uniform sampler2D ReflectionMap;

varying vec3 toLight[8];
varying vec3 vertexpos;

vec2 SphereMap(const in vec3 U,const in vec3 N)
{
	vec3 R;

	R = reflect(U,N);

	R.z += 1.0;
	R = normalize(R);
	return R.xy*0.5+0.5;
}

vec4 Light(vec4 diffuse,vec4 specular,vec3 normal,int i,vec3 Eye)
{
	vec3 Reflected       = normalize( 2.0 * dot(normal, toLight[i]) *  normal- toLight[i]);

	vec4 diff=max(0.0, dot(normal,normalize(toLight[i])))*gl_LightSource[i].diffuse;
	vec4 spec=gl_LightSource[i].specular * pow(max(dot(Reflected, Eye), 0.0), gl_FrontMaterial.shininess);

	vec4 color = diff * diffuse + specular *spec;

	if(gl_LightSource[i].position.w>0.0)
	{
		float range=gl_LightSource[i].quadraticAttenuation;
		if(range>0.0)
		{
			float dist=length(toLight[i]);
			if(dist>range)
				color=vec4(0.0,0.0,0.0,0.0);
			else
				color*=(range-dist)/range;
		}
	}

	color.a=1.0;
	return color;
}


void main()
{
	vec3 surfaceNormal = texture2D(BumpMap, gl_TexCoord[0].xy).xyz;
	surfaceNormal = (surfaceNormal - 0.5) * 2.0;
	//surfaceNormal=vec3(0.0,0.0,1.0);

	vec4 diffuse = texture2D(DiffuseMap, gl_TexCoord[0].xy);
	vec4 specular = texture2D(SpecularMap, gl_TexCoord[0].xy);
	vec4 emissive =texture2D(EmissiveMap, gl_TexCoord[0].xy);

	vec4 refl=texture2D(ReflectionMap, gl_TexCoord[0].xy)*texture2D(EnvironmentMap, SphereMap(normalize(vertexpos),normalize(gl_NormalMatrix*surfaceNormal)));

	vec3 Eye             = normalize(-vertexpos);

	int i;
	for(i=0;i<8;++i)
	{
		gl_FragColor += Light(diffuse,specular,surfaceNormal,i,Eye);
	}
//gl_FragColor=vec4(1.0,1.0,1.0,1.0);

	gl_FragColor.a = 1.0;
}



vertexshader:
attribute vec3 tangent;
attribute vec3 binormal;

varying vec3 toLight[8];
varying vec3 vertexpos;//for spheremapping

void main()
{	
      gl_TexCoord[0] = gl_TextureMatrix[0] * gl_MultiTexCoord0;
	
	vec4 vertexPos = gl_ModelViewMatrix * gl_Vertex;
	
	vec3 n = normalize(gl_NormalMatrix * gl_Normal);
	vec3 t = normalize(gl_NormalMatrix * tangent);
	vec3 b = normalize(gl_NormalMatrix * binormal);

	
	mat3 rotMat = mat3(t, b, n);


	vec4 lightpos;
	int i;
	for(i=0;i<8;++i)
	{
		lightpos=gl_LightSource[i].position;
		toLight[i] = (lightpos- vertexPos).xyz; 
		toLight[i] = toLight[i] * rotMat;  
      }

      gl_Position = ftransform();

	vertexpos=(gl_ModelViewMatrix*gl_Vertex).xyz;
         	
} 
