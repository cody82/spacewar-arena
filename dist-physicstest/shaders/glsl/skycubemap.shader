SHADERTEXT

vertexshader:

uniform float Aspect;
void main()
{
	//Out.pos = float4(In.pos.xy, 0, 1);
	// Multiply x by aspect ratio, else we get a streched image!
	//Out.texCoord = mul(float4(In.pos.x * 2.0f, In.pos.y, scale, 0), viewInverse).xyz; 

	gl_Position = vec4(gl_Vertex.xy,0,1);
      //gl_TexCoord[0] = mul(vec4(gl_Vertex.x * 1.33, gl_Vertex.y, 1.0, 0), gl_ModelViewMatrix ).xyz;
	
	vec4 v=vec4(gl_Vertex.x * Aspect, gl_Vertex.y, 1.0, 0.0);
	gl_TexCoord[0] = v * gl_ModelViewMatrixInverse;

}

fragmentshader:
uniform samplerCube Texture;

void main()
{
	vec4 texCol = textureCube(Texture, gl_TexCoord[0].xyz);
	gl_FragColor = texCol;
}