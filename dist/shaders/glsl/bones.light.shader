SHADERTEXT

vertexshader:
uniform mat4 Bones[40];
attribute vec3 tangent;
attribute vec3 binormal;
varying vec3 toLight[8];

varying vec3 vertexPos;
void main()
{
        gl_TexCoord[0] = gl_MultiTexCoord0;
        //vec4 pos = Bones[int(gl_MultiTexCoord0[2])] * gl_Vertex;
        //gl_Position = gl_ModelViewProjectionMatrix * pos;
	
	mat4 m = gl_ModelViewProjectionMatrix * Bones[int(gl_MultiTexCoord0[2])];
	gl_Position =  m * gl_Vertex;

	mat4 NormalMatrix = gl_ModelViewMatrix * Bones[int(gl_MultiTexCoord0[2])];
	vec3 pos = (NormalMatrix * gl_Vertex).xyz;
	vertexPos=pos;
	NormalMatrix[3]=vec4(0.0,0.0,0.0,1.0);

	vec3 n = normalize(NormalMatrix * vec4(gl_Normal,1.0)).xyz;
	vec3 t = normalize(NormalMatrix * vec4(tangent,1.0)).xyz;
	vec3 b = normalize(NormalMatrix * vec4(binormal,1.0)).xyz;

	mat3 rotMat = mat3(t, b, n);

	vec4 lightpos;
	int i;
	for(i=0;i<8;++i)
	{
		lightpos=gl_LightSource[i].position;
		toLight[i] = lightpos.xyz - pos; 
		toLight[i] = toLight[i] * rotMat;
	}

}


fragmentshader:
uniform sampler2D DiffuseMap;
uniform sampler2D EmissiveMap;
uniform sampler2D BumpMap;
uniform float Time;
varying vec3 toLight[8];

varying vec3 vertexPos;

vec4 light(vec4 emissive,vec4 diffuse,vec3 normal,vec3 tolight,vec3 eye)
{
	float NdotL = max(0.0, dot(normal,normalize(tolight)));
	vec3 Reflected       = normalize( 2.0 * dot(normal , tolight) *  normal - tolight);
	float specular = emissive.r * pow(max(dot(Reflected, eye), 0.0), 8.0);
	vec4 color = vec4(specular) + NdotL * (diffuse*(1.0-emissive.a) + vec4(0.5*cos(Time)+0.5,0.5*cos(Time*2.0)+0.5,0.5*cos(Time*4.0)+0.5,0.0)*emissive.a) + emissive.b*diffuse;
	return color;
}

void main()
{
	vec4 emissive =texture2D(EmissiveMap, gl_TexCoord[0].xy);
	vec4 diffuse = texture2D(DiffuseMap, gl_TexCoord[0].xy);
	vec4 bump=texture2D(BumpMap, gl_TexCoord[0].xy);

	vec4 normal=vec4(bump.a,bump.b,length(vec3(1.0,bump.a,bump.b)),1.0);
	vec3 surfaceNormal = normal.xyz;
      surfaceNormal = (surfaceNormal - 0.5) * 2.0;
	vec3 Eye             = normalize(-vertexPos);


	//float NdotL = max(0.0, dot(surfaceNormal,normalize(toLight)));

	//vec3 Reflected       = normalize( 2.0 * dot(surfaceNormal , toLight) *  surfaceNormal - toLight);
	//float specular = emissive.r * pow(max(dot(Reflected, Eye), 0.0), 8.0);


	//vec4 color = vec4(specular) + NdotL * (diffuse*(1.0-emissive.a) + vec4(0.5*cos(Time)+0.5,0.5*cos(Time*2.0)+0.5,0.5*cos(Time*4.0)+0.5,0.0)*emissive.a) + emissive.b*diffuse;

	int i;
	vec4 color=vec4(0.0,0.0,0.0,0.0);
	for(i=0;i<8;++i)
	{
		color+=light(emissive,diffuse,surfaceNormal,toLight[i],Eye)*gl_LightSource[i].diffuse;
	}

	color.a=1.0;
	gl_FragColor=color;
}