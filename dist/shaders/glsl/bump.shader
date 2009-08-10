SHADERTEXT

fragmentshader:
uniform sampler2D DiffuseMap;
uniform sampler2D BumpMap;

varying vec3 toLight[8];

vec4 diffuse;

vec4 Light(int i,vec3 normal)
{
	float NdotL = max(0.0, dot(normal,normalize(toLight[i])));

	vec4 color = gl_LightSource[i].diffuse * diffuse * NdotL;

	if(gl_LightSource[i].position.w>0)
	{
		float range=gl_LightSource[i].quadraticAttenuation;
		if(range>0)
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
	diffuse=texture2D(DiffuseMap, gl_TexCoord[0].xy);
	vec3 surfaceNormal = texture2D(BumpMap, gl_TexCoord[0].xy).xyz;
      surfaceNormal = (surfaceNormal - 0.5) * 2.0;

	vec4 color=vec4(0.0,0.0,0.0,0.0);
	int i;
	for(i=0;i<8;++i)
	{
		color+=Light(i,surfaceNormal);
	}

	gl_FragColor = color;
	gl_FragColor.a=1.0;
}



vertexshader:
attribute vec3 tangent;
attribute vec3 binormal;

varying vec3 toLight[8];

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
} 