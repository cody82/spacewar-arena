using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Math;
using Cheetah;
using OpenTK.Graphics.OpenGL;

namespace Cheetah.Graphics
{
	public class SimpleUserInterface
	{
		GameWindow GameWindow;
		SimpleRenderer Renderer;
		public SimpleUserInterface()
		{
			GameWindow=new GameWindow(512,512);
			Renderer=new SimpleRenderer();
			Renderer.Init();
		}
	}
	
	public class SimpleRenderer
	{
		public void Init()
		{
			GL.ClearColor(0,0,0,1);
		}
		
		public void Draw(Scene s)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit|ClearBufferMask.DepthBufferBit);
			
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadIdentity();
			GL.Ortho(-10000,10000,-10000,10000,-1000,1000);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
			
			foreach(Node n in s.FindEntitiesByType<Node>())
			{
				GL.PushMatrix();
				//GL.MultMatrix(n.Matrix);
				Draw(n);
				GL.PopMatrix();
			}
		}
		
		public void Draw(Node n)
		{
			GL.Scale(100,100,100);
			
			GL.Begin(BeginMode.Lines);
			GL.Color3(1,0,0);
			GL.Vertex3(0,0,0);
			GL.Color3(1,0,0);
			GL.Vertex3(1,0,0);
			
			GL.Color3(0,1,0);
			GL.Vertex3(0,0,0);
			GL.Color3(0,1,0);
			GL.Vertex3(0,1,0);
			
			GL.Color3(0,0,1);
			GL.Vertex3(0,0,0);
			GL.Color3(0,0,1);
			GL.Vertex3(0,0,1);
			GL.End();
		}
	}
	
	public class OpenGL1
	{
		public OpenGL1 ()
		{
		}
		
		public void Init()
		{
			GL.ClearColor(0,0,0,1);
		}
		
		public void Clear()
		{
			GL.Clear(ClearBufferMask.ColorBufferBit|ClearBufferMask.DepthBufferBit);
		}

	}
}
