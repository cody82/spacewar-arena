uniform sampler2D Texture;

uniform vec2 WindowSize;
uniform float Time;

void main()
{
	//float strength=(cos(Time*10.0)+1.0)*0.5;
	//const float dist=0.03;
	//float strength=cos(Time*10.0)*0.5;
	//vec2 dist=2.0*1.0/WindowSize;
	//vec2 dist=1.0/vec2(1024.0,1024.0);
	  float ddx=1.0/1024.0;
	  float ddy=1.0/1024.0;


vec4 outp = vec4(0.0, 0.0, 0.0, 0.0);

  // Texturen auslesen
  // und vertikal bluren (gauss)
  outp += 0.015625 * texture2D(Texture, gl_TexCoord[0].xy + vec2(ddx*-3.0, 0.0) );
  outp += 0.09375 * texture2D(Texture, gl_TexCoord[0].xy + vec2(ddx*-2.0, 0.0) );
  outp += 0.234375 * texture2D(Texture, gl_TexCoord[0].xy + vec2(ddx*-1.0, 0.0) );
  outp += 0.3125 * texture2D(Texture, gl_TexCoord[0].xy + vec2(0.0, 0.0) );
  outp += 0.234375 * texture2D(Texture, gl_TexCoord[0].xy + vec2(ddx*1.0, 0.0) );
  outp += 0.09375 * texture2D(Texture, gl_TexCoord[0].xy + vec2(ddx*2.0, 0.0) );
  outp += 0.015625 * texture2D(Texture, gl_TexCoord[0].xy + vec2(ddx*3.0, 0.0) ); 

  outp += 0.015625 * texture2D(Texture, gl_TexCoord[0].xy + vec2(0.0, ddy*-3.0) );
  outp += 0.09375 * texture2D(Texture, gl_TexCoord[0].xy + vec2(0.0, ddy*-2.0) );
  outp += 0.234375 * texture2D(Texture, gl_TexCoord[0].xy + vec2(0.0, ddy*-1.0) );
  outp += 0.3125 * texture2D(Texture, gl_TexCoord[0].xy + vec2(0.0, 0.0) );
  outp += 0.234375 * texture2D(Texture, gl_TexCoord[0].xy + vec2(0.0, ddy*1.0) );
  outp += 0.09375 * texture2D(Texture, gl_TexCoord[0].xy + vec2(0.0, ddy*2.0) );
  outp += 0.015625 * texture2D(Texture, gl_TexCoord[0].xy + vec2(0.0, ddy*3.0) );

//outp=texture2D(Texture, gl_TexCoord[0].xy);

	//smooth=smooth*smooth;
	//outp=sqrt(outp);
	gl_FragColor=outp;

	gl_FragColor[3]=1.0;

}
