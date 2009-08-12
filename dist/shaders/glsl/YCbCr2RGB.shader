SHADERTEXT

fragmentshader:
uniform sampler2D EmissiveMap;

vec4 YCbCr2RGB(vec4 yuv)
{
	/*return vec4(
		yuv[0] + 1.402 *(yuv[2]-0.5),
		yuv[0] - 0.34414 *(yuv[1]-0.5) - 0.71414 *(yuv[2]-0.5),
		yuv[0] + 1.772 *(yuv[1]-0.5),
		yuv[3]
		);*/
//return yuv;
	return vec4(
		yuv[0] + 1.402 *(yuv[2]-0.5),
		yuv[0] - 0.34414 *(yuv[1]-0.5) - 0.71414 *(yuv[2]-0.5),
		yuv[0] + 1.772 *(yuv[1]-0.5),
		yuv[3]
		);
}			

void main()
{
	vec4 emissive =texture2D(EmissiveMap, gl_TexCoord[0].xy);
	emissive=YCbCr2RGB(emissive.bgra);
	emissive[3]=1.0;
	gl_FragColor = emissive;
}


vertexshader:
void main()
{	
        gl_TexCoord[0] = gl_TextureMatrix[0] * gl_MultiTexCoord0;
        
        gl_Position = ftransform();                       	
}

