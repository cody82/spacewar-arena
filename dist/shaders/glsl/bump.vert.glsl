attribute vec3 tangent;
attribute vec3 binormal;

varying vec3 toLight;

void main()
{	
        gl_TexCoord[0] = gl_TextureMatrix[0] * gl_MultiTexCoord0;
	
        vec4 vertexPos = gl_ModelViewMatrix * gl_Vertex;
	
	vec3 n = normalize(gl_NormalMatrix * gl_Normal);
	vec3 t = normalize(gl_NormalMatrix * tangent);
	vec3 b = normalize(gl_NormalMatrix * binormal);

	
	mat3 rotMat = mat3(t, b, n);
	vec4 lightpos=gl_LightSource[0].position;
	//vec4 lightpos=vec4(0.0,0.0,10.0,1.0);
	toLight = (lightpos- vertexPos).xyz; 
//toLight=vec3(0.0,1.0,0.0);
	toLight = toLight * rotMat;  
        
        gl_Position = ftransform();                       	
} 