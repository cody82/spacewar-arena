using System;
using Qyoto;

/**
 * ZetCode Qyoto C# tutorial
 *
 * This program creates a quit
 * button. When we press the button,
 * the application terminates. 
 *
 * @author jan bodnar
 * website zetcode.com
 * last modified April 2009
 */


public class QyotoApp : QWidget {

    public QyotoApp() {

        SetWindowTitle("Quit button");

        InitUI();

        Resize(250, 150);
        Move(300, 300);
        Show();
    }

    public void InitUI() {
        
        //QPushButton quit = new QPushButton("Quit", this);

        //Connect(quit, SIGNAL("clicked()"), qApp, SLOT("quit()"));
        //quit.SetGeometry(50, 40, 80, 30);
		ui=new Ui_Form();
		ui.SetupUi(this);
    }

    public static int Main(String[] args) {
        new QApplication(args);
        new QyotoApp();
        return QApplication.Exec();
    }
	
	Ui_Form ui;
}
