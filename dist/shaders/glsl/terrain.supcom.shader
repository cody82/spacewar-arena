SHADERTEXT

vertexshader:
void main()
{	
        gl_TexCoord[0] = gl_MultiTexCoord0;
	gl_Position = ftransform();
	gl_FrontColor=gl_Color;
}


fragmentshader:
uniform sampler2D LowerAlbedo;
uniform sampler2D UpperAlbedo;
uniform sampler2D Albedo0;
uniform sampler2D Albedo1;
uniform sampler2D Albedo2;
uniform sampler2D Albedo3;
uniform sampler2D Mask;
uniform float TerrainScale;
uniform float Albedo0Tile;
uniform float Albedo1Tile;
uniform float Albedo2Tile;
uniform float Albedo3Tile;
uniform float LowerAlbedoTile;
uniform float UpperAlbedoTile;
uniform sampler2D NormalMap;

void main()
{
	vec2 texcoord=gl_TexCoord[0].yx;

	vec4 mask = texture2D(Mask, texcoord);
	vec4 norm = texture2D(NormalMap, texcoord);

	vec4 upperAlbedo = texture2D( UpperAlbedo, texcoord  * TerrainScale* UpperAlbedoTile );
	vec4 lowerAlbedo = texture2D( LowerAlbedo, texcoord  * TerrainScale* LowerAlbedoTile );
	vec4 stratum0Albedo = texture2D( Albedo0, texcoord  * TerrainScale* Albedo0Tile );
	vec4 stratum1Albedo = texture2D( Albedo1, texcoord  * TerrainScale* Albedo1Tile );
	vec4 stratum2Albedo = texture2D( Albedo2, texcoord  * TerrainScale* Albedo2Tile );
	vec4 stratum3Albedo = texture2D( Albedo3, texcoord  * TerrainScale* Albedo3Tile );
	
	// blend all albedos together
	vec4 albedo = lowerAlbedo;
	albedo = mix( albedo, stratum0Albedo, mask.x );
	albedo = mix( albedo, stratum1Albedo, mask.y );
	albedo = mix( albedo, stratum2Albedo, mask.z );
	albedo = mix( albedo, stratum3Albedo, mask.w );
	albedo.xyz = mix( albedo.xyz, upperAlbedo.xyz, upperAlbedo.w );

	vec3 normal=vec3(norm.g,length(vec3(1.0,norm.a,norm.g)),norm.a);
	normal = (normal - 0.5) * 2.0;
	normal=gl_NormalMatrix*normal;

	vec3 toLight=normalize(gl_LightSource[0].position.xyz-(gl_ModelViewMatrix*vec4(0.0,0.0,0.0,1.0)).xyz);
	//vec3 toLight=normalize(vec3(0.0,1.0,1.0));
	float NdotL = max(0.0, dot(normal,toLight));
	albedo*=NdotL;


	albedo.a=1.0;
	//mask.a=1.0;

	gl_FragColor = albedo;
}