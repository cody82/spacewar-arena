all:
	cd Properties && resgen Resources.resx Spacewar2006.Properties.Resources.resources
	gmcs -define:LINUX -resource:Properties/Spacewar2006.Properties.Resources.resources -reference:cheetah.dll -reference:System.Drawing -reference:cheetah.dll -reference:System.Runtime.Serialization.Formatters.Soap -reference:System.Data -reference:System.Windows.Forms -lib:../../dist/bin/linux -out:../../dist/bin/linux/spacewar2006.exe *.cs Forms/*.cs Properties/*.cs
