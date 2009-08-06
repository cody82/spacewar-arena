using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace Cheetah.Forms
{
    public partial class EditorForm : Form
    {
        public EditorForm()
        {
            InitializeComponent();
        }

        Type[] FindClasses()
        {
            Type[] nodes = Root.Instance.Factory.FindTypes(null, typeof(Node));
            nodes = Array.FindAll(nodes, delegate(Type t) { return t.GetCustomAttributes(typeof(EditableAttribute), false).Length > 0; });

            Array.Sort<Type>(nodes, delegate(Type t1, Type t2) { return string.Compare(t1.Name, t2.Name); });

            return nodes;
        }

        private void EditorForm_Load(object sender, EventArgs e)
        {
            CreateList.Items.AddRange(FindClasses());
        }

        void DisplayEntities()
        {
            SelectList.Items.Clear();

            foreach (Entity e in Root.Instance.Scene.FindEntitiesByType<Entity>())
            {
                SelectList.Items.Add(e);
            }
        }

        void DisplayProperties()
        {
            Properties.Items.Clear();

            Entity select=((Editor)Root.Instance.CurrentFlow).Select;
            Type t = select.GetType();
            //MemberInfo[] m=t.GetMembers(BindingFlags.Public | BindingFlags.Instance);
            FieldInfo[] f = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] p = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            object[] list = new object[f.Length + p.Length];
            int i = 0;
            foreach (FieldInfo fi in f)
            {
                //Console.WriteLine(fi.ToString());
                //object v = fi.GetValue(Select);
                //Console.WriteLine(v != null ? v.ToString() : "null");
                if (fi.GetCustomAttributes(typeof(EditableAttribute), true).Length > 0)
                {
                    list[i++] = fi;
                    if(fi.GetValue(select)!=null)
                        Properties.Items.Add(new ListViewItem(new string[] { fi.Name, fi.GetValue(select).ToString() }));
                    else
                         Properties.Items.Add(new ListViewItem(new string[] { fi.Name, "" }));
               }

            }
            foreach (PropertyInfo pi in p)
            {
                //Console.WriteLine(pi.ToString());
                //object v=pi.GetValue(Select, null);
                //Console.WriteLine(v!=null?v.ToString():"null");
                if (pi.GetCustomAttributes(typeof(EditableAttribute), true).Length > 0)
                {
                    list[i++] = pi;
                    if(pi.GetValue(select, null)!=null)
                        Properties.Items.Add(new ListViewItem(new string[] { pi.Name, pi.GetValue(select, null).ToString() }));
                    else
                        Properties.Items.Add(new ListViewItem(new string[] { pi.Name, "" }));

                }
            }

        }

        private void CreateButton_Click(object sender, EventArgs e)
        {
            Node n = (Node)Root.Instance.Factory.CreateInstance((Type)CreateList.SelectedItem);
            Root.Instance.Scene.Spawn(n);
            Select(n);
            DisplayEntities();
        }

        private void CreateTab_Click(object sender, EventArgs e)
        {

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.CheckFileExists = false;

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                FileStream fs = new FileStream(dlg.FileName, FileMode.Create, FileAccess.Write);
                Editor.XmlMapWriter map = new Editor.XmlMapWriter(fs);

                map.Write(Root.Instance.Scene,"test");
                fs.Close();
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {

                FileStream fs = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read);
                Editor.XmlMapReader map = new Editor.XmlMapReader(fs);

                map.Read();

                fs.Close();

                DisplayEntities();
            }
        }

        SelectionMarker selectionmarker;

        void Select(Entity n)
        {
            ((Editor)Root.Instance.CurrentFlow).Select = n;
            DisplayProperties();

            if (n != null)
            {
                if (selectionmarker == null)
                {
                    selectionmarker = new SelectionMarker();
                    Root.Instance.Scene.Spawn(selectionmarker);
                }

                selectionmarker.Position = ((Node)n).AbsolutePosition;
            }
            else
            {
                if (selectionmarker != null)
                    selectionmarker.Visible = false;
            }
        }
        private void SelectList_SelectedValueChanged(object sender, EventArgs e)
        {
            Select((Entity)SelectList.SelectedItem);
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new MapSettings().ShowDialog();
        }
    }
}