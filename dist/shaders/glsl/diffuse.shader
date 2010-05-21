SHADERTEXT

fragmentshader:
// Fragment Program
// Uniforms
uniform sampler2D DiffuseMap;
uniform sampler2D BumpMap;

// Varyings
varying vec3 ToLight[8];

// Attributes

// Variables
vec4 Diffuse;
vec3 Normal;

// Functions
float NdotL(vec3 normal,vec3 light)
{
return max(0.0, dot(normal,normalize(light)));
}
vec4 Light(int i)
{
vec4 color=(gl_LightSource[i].diffuse * Diffuse * NdotL(Normal,ToLight[i]));
float range = gl_LightSource[i].quadraticAttenuation;
//if(gl_LightSource[i].position.w>0.0)
//{
if (range > 0.0)
{
float dist=length(ToLight[i]);
color *= max(0.0,range - dist) / range;
}
//}
color.a=1.0;
return color;
}

// Main
void main()
{
int i;
vec4 color;
Diffuse=texture2D(DiffuseMap, gl_TexCoord[0].xy);
Normal = (texture2D(BumpMap, gl_TexCoord[0].xy).xyz -0.5)*2.0;
for(i=0;i<8;++i)
{
color+=Light(i);
}
color.a=1.0;
gl_FragColor=color;
}

// End Fragment Program

vertexshader:
// Vertex Program
// Uniforms

// Varyings
varying vec3 ToLight[8];

// Attributes
attribute vec3 tangent;
attribute vec3 binormal;

// Variables

// Functions

// Main
void main()
{
gl_TexCoord[0] = gl_MultiTexCoord0;
vec4 vertexPos = gl_ModelViewMatrix * gl_Vertex;
vec3 n = normalize(gl_NormalMatrix * gl_Normal);
vec3 t = normalize(gl_NormalMatrix * tangent);
vec3 b = normalize(gl_NormalMatrix * binormal);
mat3 tbn = mat3(t, b, n);
int i;
for (i = 0; i < 8; ++i)
{
ToLight[i] = (gl_LightSource[i].position - vertexPos).xyz * tbn;
}
gl_Position=ftransform();
}

// End Vertex Program
