/********************************************************************************
** Form generated from reading ui file 'Ui_Form.ui'
**
** Created: Sat Jul 3 00:41:16 2010
**      by: Qt User Interface Compiler for C# version 4.6.2
**
** WARNING! All changes made in this file will be lost when recompiling ui file!
********************************************************************************/


using Qyoto;

public class Ui_Form
{
    public QVBoxLayout vboxLayout;
    public QTabWidget tabWidget;
    public QWidget tab;
    public QWidget tab_2;
    public QVBoxLayout vboxLayout1;
    public QListView listView;
    public QGroupBox groupBox;
    public QPushButton pushButton;
    public QRadioButton radioButton;
    public QRadioButton radioButton_2;
    public QLineEdit lineEdit;
    public QWidget tab_3;

    public void SetupUi(QWidget Form)
    {
    if (Form.ObjectName == "")
        Form.ObjectName = "Form";
    QSize Size = new QSize(887, 679);
    Size = Size.ExpandedTo(Form.MinimumSizeHint());
    Form.Size = Size;
    vboxLayout = new QVBoxLayout(Form);
    vboxLayout.ObjectName = "vboxLayout";
    tabWidget = new QTabWidget(Form);
    tabWidget.ObjectName = "tabWidget";
    tab = new QWidget();
    tab.ObjectName = "tab";
    tabWidget.AddTab(tab, QApplication.Translate("Form", "Tab 1", null, QApplication.Encoding.UnicodeUTF8));
    tab_2 = new QWidget();
    tab_2.ObjectName = "tab_2";
    vboxLayout1 = new QVBoxLayout(tab_2);
    vboxLayout1.ObjectName = "vboxLayout1";
    listView = new QListView(tab_2);
    listView.ObjectName = "listView";

    vboxLayout1.AddWidget(listView);

    groupBox = new QGroupBox(tab_2);
    groupBox.ObjectName = "groupBox";
    groupBox.MinimumSize = new QSize(0, 64);
    pushButton = new QPushButton(groupBox);
    pushButton.ObjectName = "pushButton";
    pushButton.Geometry = new QRect(690, 6, 151, 51);
    radioButton = new QRadioButton(groupBox);
    radioButton.ObjectName = "radioButton";
    radioButton.Geometry = new QRect(120, 10, 109, 22);
    radioButton_2 = new QRadioButton(groupBox);
    radioButton_2.ObjectName = "radioButton_2";
    radioButton_2.Geometry = new QRect(120, 40, 109, 22);
    lineEdit = new QLineEdit(groupBox);
    lineEdit.ObjectName = "lineEdit";
    lineEdit.Geometry = new QRect(280, 20, 371, 27);

    vboxLayout1.AddWidget(groupBox);

    tabWidget.AddTab(tab_2, QApplication.Translate("Form", "Join", null, QApplication.Encoding.UnicodeUTF8));
    tab_3 = new QWidget();
    tab_3.ObjectName = "tab_3";
    tabWidget.AddTab(tab_3, QApplication.Translate("Form", "Page", null, QApplication.Encoding.UnicodeUTF8));

    vboxLayout.AddWidget(tabWidget);


    RetranslateUi(Form);

    tabWidget.CurrentIndex = 1;


    QMetaObject.ConnectSlotsByName(Form);
    } // SetupUi

    public void RetranslateUi(QWidget Form)
    {
    Form.WindowTitle = QApplication.Translate("Form", "Form", null, QApplication.Encoding.UnicodeUTF8);
    tabWidget.SetTabText(tabWidget.IndexOf(tab), QApplication.Translate("Form", "Tab 1", null, QApplication.Encoding.UnicodeUTF8));
    groupBox.Title = QApplication.Translate("Form", "GroupBox", null, QApplication.Encoding.UnicodeUTF8);
    pushButton.Text = QApplication.Translate("Form", "PushButton", null, QApplication.Encoding.UnicodeUTF8);
    radioButton.Text = QApplication.Translate("Form", "RadioButton", null, QApplication.Encoding.UnicodeUTF8);
    radioButton_2.Text = QApplication.Translate("Form", "RadioButton", null, QApplication.Encoding.UnicodeUTF8);
    tabWidget.SetTabText(tabWidget.IndexOf(tab_2), QApplication.Translate("Form", "Join", null, QApplication.Encoding.UnicodeUTF8));
    tabWidget.SetTabText(tabWidget.IndexOf(tab_3), QApplication.Translate("Form", "Page", null, QApplication.Encoding.UnicodeUTF8));
    } // RetranslateUi

}

namespace Ui {
    public class Form : Ui_Form {}
} // namespace Ui

