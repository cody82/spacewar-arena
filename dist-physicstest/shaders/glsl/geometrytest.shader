SHADERTEXT

geometry input: lines
geometry output: linestrip

fragmentshader:
void main()
{
	gl_FragColor = gl_Color;
}

vertexshader:
void main()
{	
	gl_Position = ftransform();
	gl_FrontColor=vec4(1.0,1.0,1.0,1.0);
}


geometryshader:
#version 120
#extension GL_EXT_geometry_shader4 : enable

// a passthrough geometry shader for color and position   
void main()   
{   
//gl_Position = (2.0*gl_PositionIn[0]+gl_PositionIn[1])*0.333;
//gl_FrontColor = gl_FrontColorIn[0];
//EmitVertex();
//gl_Position = (gl_PositionIn[0]+2.0*gl_PositionIn[1])*0.333;
//gl_FrontColor = gl_FrontColorIn[0];
//EmitVertex();

  for(int i = 0; i < gl_VerticesIn; ++i)   
  {   
    gl_FrontColor = gl_FrontColorIn[i]; 
    gl_Position = gl_PositionIn[i];   
    EmitVertex();   
  }   
}  
