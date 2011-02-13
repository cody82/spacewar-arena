uniform float Time;
void main()
{	
        gl_TexCoord[0] = gl_MultiTexCoord0;
        
	vec4 pos=gl_Vertex;

	//float param=Time*10.0++pos.z/100.0;

	float h=(cos(pos.x/100.0+pos.z/100.0)+cos(Time*10.0))/2.0;

	pos=vec4(pos.x,pos.y+h*100.0,pos.z,pos.w);


        gl_Position = gl_ModelViewProjectionMatrix * pos;
	gl_FrontColor=(h+1.0)*0.5*vec4(1,1,1,1);
} 