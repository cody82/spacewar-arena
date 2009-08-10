attribute vec3 tangent;
attribute vec3 binormal;

varying vec3 toLight;
varying vec3 vertexpos;//for spheremapping
varying mat3 rotMat;

void main()
{	
        gl_TexCoord[0] = gl_TextureMatrix[0] * gl_MultiTexCoord0;
	
        vec4 vertexPos = gl_ModelViewMatrix * gl_Vertex;
	
	vec3 n = normalize(gl_NormalMatrix * gl_Normal);
	vec3 t = normalize(gl_NormalMatrix * tangent);
	vec3 b = normalize(gl_NormalMatrix * binormal);

	
	rotMat = mat3(t, b, n);
	vec4 lightpos=gl_LightSource[0].position;

	toLight = (lightpos- vertexPos).xyz; 

	toLight = toLight * rotMat;  
        
        gl_Position = ftransform();

//spheremapping
	vertexpos=rotMat*(gl_ModelViewMatrix*gl_Vertex).xyz;
         	
} 