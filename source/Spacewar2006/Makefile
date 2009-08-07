all:
	#cd Properties && resgen Resources.resx Spacewar2006.Properties.Resources.resources
	gmcs -define:LINUX -resource:Properties/Spacewar2006.Properties.Resources.resources -reference:System.Drawing -reference:System.Runtime.Serialization.Formatters.Soap -reference:System.Data -lib:../libs -reference:System.Windows.Forms -reference:../Cheetah/bin/Release/Cheetah.dll -out:bin/Release/Spacewar2006.exe *.cs Forms/*.cs Properties/*.cs
