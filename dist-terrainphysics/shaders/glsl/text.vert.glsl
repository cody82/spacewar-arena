uniform vec2 FontSize;
uniform vec4 Color;
uniform float CharSize;

attribute vec2 Pos;
attribute vec2 CharPos;

varying vec2 Position;
void main()
{
	vec4 pos;
	pos[0] = Pos[0]+gl_Vertex[0]*FontSize[0];
	pos[1] = Pos[1]+gl_Vertex[1]*FontSize[1];
	pos[2] = 0.0;
	pos[3] = 1.0;
	Position = pos.xy;
	pos=pos*vec4(2.0,-2.0,0.0,1.0);
	pos=pos-vec4(1.0,-1.0,0.0,0.0);
	//pos=pos*0.0001;
	gl_Position = pos;
	gl_TexCoord[0].x = (CharPos[0]+gl_Vertex[0])*CharSize;
	gl_TexCoord[0].y = (CharPos[1]+gl_Vertex[1])*CharSize;
	gl_FrontColor = Color;
}