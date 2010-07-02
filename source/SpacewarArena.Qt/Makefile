ui:
	uics Ui_Form.ui > Ui_Form.cs

compile: ui
	gmcs -r:/usr/lib/cli/qt-dotnet-4.5/qt-dotnet.dll Program.cs Ui_Form.cs

run: compile
	LD_LIBRARY_PATH=/usr/lib/cli/qt-dotnet-4.5/ mono Program.exe

all: compile
