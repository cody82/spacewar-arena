all:
	mkdir bin
	mkdir bin/Release
	gmcs *.cs Forms/*.cs -optimize+ -unsafe -reference:System.Drawing -lib:../libs -reference:Tao.OpenGl.dll -reference:Tao.Sdl.dll -reference:Tao.DevIl.dll -reference:ICSharpCode.SharpZipLib.dll -reference:System.Windows.Forms -reference:Tao.OpenGl.ExtensionLoader.dll -reference:Tao.OpenGl.dll -reference:Boo.Lang.Compiler -reference:System.Web -reference:Boo.Lang.Parser -reference:Ode.NET -reference:Boo.Lang.Interpreter -reference:Meebey.SmartIrc4net -reference:System.Data -reference:OpenTK.OpenGL -reference:../Lidgren.Library.Network/Lidgren.Library.Network/bin/Release/Lidgren.Library.Network.dll -target:library -out:bin/Release/Cheetah.dll
