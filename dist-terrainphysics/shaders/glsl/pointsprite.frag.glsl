uniform sampler2D EmissiveMap;

void main()
{
	vec4 emissive =texture2D(EmissiveMap, gl_TexCoord[0].xy);
	gl_FragColor = emissive*gl_Color;
	//gl_FragColor.a=0.5;
}