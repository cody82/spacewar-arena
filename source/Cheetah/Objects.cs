using System;
using System.IO;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using ICSharpCode.SharpZipLib.Zip;
using System.Threading;
using System.Text;
using ICSharpCode.SharpZipLib.Zip.Compression;
//using OpenDe;
//using System.Drawing;
using System.CodeDom.Compiler;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using Meebey.SmartIrc4net;
using System.Web;
using System.CodeDom;
using Microsoft.CSharp;
using System.Xml;
using System.Runtime.InteropServices;
using OpenTK.Input;
using Cheetah.Graphics;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace Cheetah
{
    /// <summary>
    /// Summary description for Objects.
    /// </summary>
    /*public class Node2D : Node
    {
        public Node2D()
        {
        }

        public Vector2 position;
    }*/


    public interface IControl
    {
        float GetPosition(int axis);
        bool GetButtonState(int n);
    }

    [Serializable]
    public class ControlInfo
    {
        public ControlInfo()
        {
        }
        public ControlInfo(ControlID id, int b1, int b2, float sens, bool inv)
        {
            Id = id;
            AxisButton1 = b1;
            Button2 = b2;
            Sensitivity = sens;
            Invert = inv;
            Deadzone = 0.0f;
            IsAxis = true;
        }
        public ControlInfo(ControlID id, int axis, float sens, bool inv, float dead)
        {
            Id = id;
            AxisButton1 = axis;
            Button2 = -1;
            Sensitivity = sens;
            Invert = inv;
            Deadzone = dead;
            IsAxis = true;
        }
        public ControlInfo(ControlID id, int button)
        {
            Id = id;
            AxisButton1 = button;
            Button2 = -1;
            Sensitivity = 1.0f;
            Invert = false;
            Deadzone = 0.0f;
            IsAxis = false;
        }

        public float GetAxisPosition()
        {
            float f;
            //if(!IsAxis)
            //	throw new Exception("control is no axis.");

            if (Control == null)
                return 0.0f;

            if (Button2 >= 0)// && !IsAxis
            {
                bool p = Control.GetButtonState(AxisButton1);
                bool n = Control.GetButtonState(Button2);
                if ((!(n || p)) || (n && p))
                    f = 0.0f;
                else if (n)
                    f = -1.0f;
                else
                    f = 1.0f;
            }
            else
            {
                f = Control.GetPosition(AxisButton1);

                if (Math.Abs(f) < Deadzone)
                    return 0.0f;
            }
            return Invert ? -f * Sensitivity : f * Sensitivity;
        }

        public bool GetButtonState()
        {
            if (Control == null)
                return false;

            if (IsAxis)
            {
                return Math.Abs(Control.GetPosition(AxisButton1)) > Deadzone;
            }
            return Control.GetButtonState(AxisButton1);
        }

        //[NonSerialized]
        public IControl Control
        {
            get
            {
                try
                {
                    return Root.Instance.UserInterface.GetControl(Id);
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
        public ControlID Id = ControlID.None;

        public int AxisButton1;//axis/positiv
        public int Button2;//negativ
        public float Sensitivity;
        public float Deadzone;
        public bool Invert;
        public bool IsAxis;
    }

    /*public class ControlMapper
    {


        public ControlMapper(Node n)
        {
        }

        public Hashtable Controls=new Hashtable();
    }*/

    public interface ITickable
    {
        void Tick(float dtime);
    }

    public class StandardControl : ITickable
    {
        public StandardControl(Node _n)
        {
            n = _n;
            IUserInterface ui = Root.Instance.UserInterface;
            Yaw = new ControlInfo(ControlID.Keyboard, 'l', 'j', 1, false);
            Pitch = new ControlInfo(ControlID.Keyboard, 'k', 'i', 1, false);
            Roll = new ControlInfo(ControlID.Keyboard, 'a', 'd', 1, false);
            Throttle = new ControlInfo(ControlID.Keyboard, 'w', 's', 1, false);

            MouseYaw = new ControlInfo(ControlID.Mouse, 0, 0.1f, false, 0);
            MousePitch = new ControlInfo(ControlID.Mouse, 1, 0.1f, false, 0);
        }

        public void Tick(float dtime)
        {
            n.localspeed = new Vector3(0, 0, Throttle.GetAxisPosition() * 100);
            /*Quaternion q=
                Quaternion.FromAxisAngle(1,0,0,Pitch.GetAxisPosition()*30)*
                Quaternion.FromAxisAngle(0,1,0,Yaw.GetAxisPosition()*30)*
                Quaternion.FromAxisAngle(0,0,1,Roll.GetAxisPosition()*30);*/
            n.rotationspeed = new Vector3(
                    Pitch.GetAxisPosition(),//+MousePitch.GetAxisPosition(),
                    Yaw.GetAxisPosition(),//+MouseYaw.GetAxisPosition(),
                    Roll.GetAxisPosition()
                );
        }

        public ControlInfo Yaw;
        public ControlInfo Pitch;
        public ControlInfo Roll;
        public ControlInfo Throttle;
        public ControlInfo MouseYaw;
        public ControlInfo MousePitch;
        protected Node n;
    }



    public class Animation
    {
        public struct KeyFrame
        {
            public KeyFrame(float time, float[] values)
            {
                Time = time;
                Values = values;
            }

            public float[] Values;
            public float Time;
        }

        public class Channel
        {
            public float[] Interpolate(float time)
            {
                if (time >= End)
                    return Frames[Frames.Count - 1].Values;
                else if (time <= Start)
                    return Frames[0].Values;

                for (int i = 0; i < Frames.Count; ++i)
                {
                    if (Frames[i].Time == time)
                        return Frames[i].Values;
                    else if (Frames[i].Time > time)
                    {
                        float a = (time - Frames[i - 1].Time) / (Frames[i].Time - Frames[i - 1].Time);
                        return Interpolate(Frames[i - 1].Values, Frames[i].Values, a);
                    }
                }
                throw new Exception();
            }

            public float[] Interpolate(float[] v1, float[] v2, float a)
            {
                if (v1.Length != v2.Length)
                    throw new Exception();

                float[] ret = new float[v1.Length];
                for (int i = 0; i < v1.Length; ++i)
                {
                    ret[i] = v1[i] * (1 - a) + v2[i] * a;
                }
                return ret;
            }

            public float Start
            {
                get
                {
                    return Frames[0].Time;
                }
            }
            public float End
            {
                get
                {
                    return Frames[Frames.Count - 1].Time;
                }
            }
            public float Duration
            {
                get
                {
                    return Frames[Frames.Count - 1].Time - Frames[0].Time;
                }
            }

            public List<KeyFrame> Frames = new List<KeyFrame>();
        }

        public float[] GetValues(string name, float time)
        {
            return Channels[name].Interpolate(time);
        }

        public Channel GetChannel(string name)
        {
            return Channels[name];
        }

        public Dictionary<string, Channel> Channels = new Dictionary<string, Channel>();
    }

    public class KeyboardControl : IControl
    {
        public struct AxisInfo
        {
            public AxisInfo(int pos, int neg)
            {
                PositiveKey = pos;
                NegativeKey = neg;
            }
            public int PositiveKey;
            public int NegativeKey;
        }

        public KeyboardControl(IKeyboard k)
        {
            //n=_n;
            kb = k;
            Axis = WSAD;
        }

        public float GetPosition(int axis)
        {
            AxisInfo ai = ((AxisInfo)Axis[axis]);
            bool p = kb.isKeyPressed(ai.PositiveKey);
            bool n = kb.isKeyPressed(ai.NegativeKey);
            if ((!(n || p)) || (n && p))
                return 0.0f;
            if (n)
                return -1.0f;
            else
                return 1.0f;
        }

        public bool GetButtonState(int n)
        {
            return false;
        }

        IKeyboard kb;

        public AxisInfo[] Axis;

        public static AxisInfo[] WSAD = new AxisInfo[] { new AxisInfo('d', 'a'), new AxisInfo('w', 's') };
    }
    /*
        public class Smooth<T>
        {
            public delegate void InterpolationFunction(T from,T to,float dTime);
            public Smooth(InterpolationFunction f)
            {
            }
            public T Current;
            public T Target;
            public void Tick(float dTime)
            {
            }
        }

        public abstract class Smooth<T>
        {
            public T Original;
            public T Smoothed;
            public abstract void Tick(float dTime);
        }
    */
    public struct SmoothVector3 : ITickable
    {
        public Vector3 Original;
        public Vector3 Smoothed;
        bool tick;
        public void Tick(float dTime)
        {
            if (!tick)
            {
                Smoothed = Original;
                tick = true;
            }
            const float time = 0.2f;
            Vector3 delta = Original - Smoothed;
            float a = Math.Min(1.0f, dTime / time);
            Smoothed = a * Original + (1.0f - a) * Smoothed;
        }
        public void Tick(float dTime, float time)
        {
            if (!tick)
            {
                Smoothed = Original;
                tick = true;
            }
            Vector3 delta = Original - Smoothed;
            float a = Math.Min(1.0f, dTime / time);
            Smoothed = a * Original + (1.0f - a) * Smoothed;
        }

        SmoothVector3(Vector3 v)
        {
            Original = Smoothed = v;
            tick = false;
        }

        static public implicit operator Vector3(SmoothVector3 vec)
        {
            return vec.Original;
        }
        /*
        static public implicit operator SmoothVector3(Vector3 vec)
        {
            return new SmoothVector3(vec);
        }*/
    }

    public struct SmoothQuaternion : ITickable
    {
        public Quaternion Original;
        public Quaternion Smoothed;
        bool tick;
        public void Tick(float dTime)
        {
            if (!tick)
            {
                Smoothed = Original;
                tick = true;
            }
            const float time = 0.2f;
            float a = Math.Min(1.0f, dTime / time);
            //Smoothed = a * Original + (1.0f - a) * Smoothed;
            Smoothed = Quaternion.Slerp(Smoothed, Original, a);
        }

        public void Tick(float dTime, float time)
        {
            if (!tick)
            {
                Smoothed = Original;
                tick = true;
            }
            float a = Math.Min(1.0f, dTime / time);
            //Smoothed = a * Original + (1.0f - a) * Smoothed;
            Smoothed = Quaternion.Slerp(Smoothed, Original, a);
        }

        public SmoothQuaternion(Quaternion q)
        {
            Original = Smoothed = q;
            tick = false;
        }

        static public implicit operator Quaternion(SmoothQuaternion quat)
        {
            return quat.Original;
        }
        /*static public implicit operator SmoothQuaternion(Quaternion quat)
        {
            return new SmoothQuaternion(quat);
        }*/
    }

    public class EditableAttribute : Attribute
    {
    }

    public class Editor : Flow
    {
        public interface IInput
        {
            void Display(object obj);
            object Parse();
        }

        public abstract class BasicInput : TextBox, IInput
        {
            public BasicInput()
            {
                MultiLine = false;
                CenterText = true;
                Size = new Vector2(200, 50);
                Center();
            }

            public void Display(object obj)
            {
                SetLine(0, obj != null ? obj.ToString() : "");
            }
            public abstract object Parse();

        }

        public class Vector3Input : Window, IInput
        {
            public Vector3Input()
                : base(new Layout(3, 1))
            {
                Color = new Color4f(0, 0, 0, 0);
                Size = new Vector2(300, 50);
                Center();

                X = new FloatInput();
                Add(X, 0, 0);
                Y = new FloatInput();
                Add(Y, 1, 0);
                Z = new FloatInput();
                Add(Z, 2, 0);

                Layout.Update(Size);
            }

            public void Display(object obj)
            {
                Vector3 v = (Vector3)obj;
                X.Display(v.X);
                Y.Display(v.Y);
                Z.Display(v.Z);
            }
            public object Parse()
            {
                return new Vector3((float)X.Parse(), (float)Y.Parse(), (float)Z.Parse());
            }

            FloatInput X;
            FloatInput Y;
            FloatInput Z;
        }

        public class QuaternionInput : Window, IInput
        {
            public QuaternionInput()
                : base(new Layout(4, 1))
            {
                Color = new Color4f(0, 0, 0, 0);
                Size = new Vector2(400, 50);
                Center();

                X = new FloatInput();
                Add(X, 0, 0);
                Y = new FloatInput();
                Add(Y, 1, 0);
                Z = new FloatInput();
                Add(Z, 2, 0);
                W = new FloatInput();
                Add(W, 3, 0);

                Layout.Update(Size);
            }

            public void Display(object obj)
            {
                Quaternion v = (Quaternion)obj;
                X.Display(v.X);
                Y.Display(v.Y);
                Z.Display(v.Z);
                W.Display(v.W);
            }
            public object Parse()
            {
                return new Quaternion((float)X.Parse(), (float)Y.Parse(), (float)Z.Parse(), (float)W.Parse());
            }

            FloatInput X;
            FloatInput Y;
            FloatInput Z;
            FloatInput W;
        }

        public class XmlMapReader
        {
            public XmlMapReader(Stream s)
            {
                stream = s;
                //XmlReaderSettings settings = new XmlReaderSettings();
                //settings.
                //xml = XmlReader.Create(s);
                xml = new XmlDocument();
                xml.Load(s);
            }

            Entity ReadEntity(XmlNode n)
            {
                Entity e = (Entity)Root.Instance.Factory.CreateInstance(n.Attributes["class"].Value);
                Type t = e.GetType();
                n = n.FirstChild;

                while (n != null)
                {
                    FieldInfo fi = t.GetField(n.Name);
                    PropertyInfo pi = t.GetProperty(n.Name);
                    if (fi != null)
                    {
                        fi.SetValue(e, ReadObject(n, fi.FieldType));
                    }
                    else if (pi != null)
                    {
                        pi.SetValue(e, ReadObject(n, pi.PropertyType), null);
                    }
                    else
                        throw new Exception("cant find " + n.Name);

                    n = n.NextSibling;
                }

                e.NoReplication = true;

                return e;
            }

            MethodInfo GetMethod(Type t)
            {
                MethodInfo[] mi = GetType().GetMethods();
                foreach (MethodInfo mi2 in mi)
                {
                    if (mi2.ReturnType.FullName == t.FullName && mi2.GetParameters().Length==1 && mi2.GetParameters()[0].ParameterType.FullName==typeof(XmlNode).FullName)
                        return mi2;
                }
                throw new Exception("cant find method for " + t.FullName);
            }

            object ReadObject(XmlNode n,Type t)
            {
                if (t == typeof(string))
                    return n.InnerText;
                else if (t == typeof(int))
                    return XmlConvert.ToInt32(n.InnerText);

                MethodInfo mi = GetMethod(t);
                return mi.Invoke(this, new object[] { n });
            }

            public Vector3 ReadVector3(XmlNode n)
            {
                Vector3 v=Vector3.Zero;
                foreach (XmlNode c in n.ChildNodes)
                {
                    switch (c.Name)
                    {
                        case "x":
                            v.X = XmlConvert.ToSingle(c.InnerText);
                            break;
                        case "y":
                            v.Y = XmlConvert.ToSingle(c.InnerText);
                            break;
                        case "z":
                            v.Z = XmlConvert.ToSingle(c.InnerText);
                            break;
                    }
                }

                return v;
            }
            public Quaternion ReadQuaternion(XmlNode n)
            {
                Quaternion v=Quaternion.Identity;
                foreach (XmlNode c in n.ChildNodes)
                {
                    switch (c.Name)
                    {
                        case "x":
                            v.X = XmlConvert.ToSingle(c.InnerText);
                            break;
                        case "y":
                            v.Y = XmlConvert.ToSingle(c.InnerText);
                            break;
                        case "z":
                            v.Z = XmlConvert.ToSingle(c.InnerText);
                            break;
                        case "w":
                            v.W = XmlConvert.ToSingle(c.InnerText);
                            break;
                    }
                }

                return v;
            }
            public void Read()
            {
                Scene scene = Root.Instance.Scene;

                XmlNode n=xml.FirstChild;
                while(n.Name!="map")
                {
                    if (n == null)
                        throw new Exception("cant find map node in xml.");
                    n = n.NextSibling;
                }

                n = n.FirstChild;
                while (n != null)
                {
                    if (n.Name == "entity")
                    {
                        scene.Spawn(ReadEntity(n));
                    }
                    n = n.NextSibling;
                }
            }

            Stream stream;
            XmlDocument xml;
        }

        public class XmlMapWriter
        {
            public XmlMapWriter(Stream s)
            {
                stream = s;
                XmlWriterSettings settings=new XmlWriterSettings();
                settings.Indent=true;
                xml = XmlWriter.Create(s,settings);
            }
            Stream stream;
            XmlWriter xml;

            public void Write(Scene scene, string title)
            {
                //xml.WriteDocType("Spacewar2006 Map",
                xml.WriteStartDocument();
                xml.WriteStartElement("map");
                xml.WriteAttributeString("version", "1.0");
                xml.WriteAttributeString("game", "spacewar2006");
                xml.WriteStartElement("title");
                xml.WriteString(title);
                xml.WriteEndElement();

                xml.WriteStartElement("rule");
                xml.WriteString("Spacewar2006.Rules.DeathMatch");
                xml.WriteEndElement();

                xml.WriteStartElement("rule");
                xml.WriteString("Spacewar2006.Rules.CaptureTheFlag");
                xml.WriteEndElement();

                foreach (Entity e in scene.FindEntitiesByType<Entity>())
                {
                    Write(e);
                }
                xml.WriteEndElement();
                xml.WriteEndDocument();
                xml.Flush();
            }

            public void Write(Entity e)
            {
                xml.WriteStartElement("entity");
                xml.WriteAttributeString("class", e.GetType().FullName);



                Type t = e.GetType();
                FieldInfo[] f = t.GetFields(BindingFlags.Public | BindingFlags.Instance);
                PropertyInfo[] p = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                object[] list = new object[f.Length + p.Length];
                int i = 0;
                foreach (FieldInfo fi in f)
                {
                    if (fi.GetCustomAttributes(typeof(EditableAttribute), true).Length > 0)
                    {
                        list[i++] = fi;

                        xml.WriteStartElement(fi.Name);
                        WriteObject(fi.GetValue(e));
                        xml.WriteEndElement();
                    }

                }
                foreach (PropertyInfo pi in p)
                {
                    if (pi.GetCustomAttributes(typeof(EditableAttribute), true).Length > 0)
                    {
                        list[i++] = pi;
                        
                        xml.WriteStartElement(pi.Name);
                         WriteObject(pi.GetValue(e, null));
                       xml.WriteEndElement();
                    }
                }

                xml.WriteEndElement();//entity
            }

            public void WriteObject(object o)
            {
                if (o == null)
                    return;

                string s = null;
                if (o is int)
                    s=XmlConvert.ToString((int)o);
                else if (o is uint)
                    s=XmlConvert.ToString((uint)o);
                else if (o is short)
                    s=XmlConvert.ToString((short)o);
                else if (o is ushort)
                    s=XmlConvert.ToString((ushort)o);
                else if (o is bool)
                    s=XmlConvert.ToString((bool)o);
                else if (o is long)
                    s=XmlConvert.ToString((long)o);
                else if (o is ulong)
                    s=XmlConvert.ToString((ulong)o);
                else if (o is string)
                    s = (string)o;
                if (s != null)
                {
                    xml.WriteString(s);
                    return;
                }

                MethodInfo mi = GetType().GetMethod("WriteValue", new Type[] { o.GetType() });
                if (mi == null)
                    throw new Exception("cant find method for " + o.GetType().FullName);
                if (mi.GetParameters()[0].ParameterType != o.GetType())
                    System.Console.WriteLine("warning: most suitable writer for " + o.GetType() + " is " + mi.GetParameters()[0].ParameterType.ToString());
                mi.Invoke(this, new object[] { o });
            }

            public void WriteValue(Vector3 v)
            {
                xml.WriteStartElement("x");
                xml.WriteString(XmlConvert.ToString(v.X));
                xml.WriteEndElement();
                xml.WriteStartElement("y");
                xml.WriteString(XmlConvert.ToString(v.Y));
                xml.WriteEndElement();
                xml.WriteStartElement("z");
                xml.WriteString(XmlConvert.ToString(v.Z));
                xml.WriteEndElement();
            }
            public void WriteValue(Quaternion v)
            {
                xml.WriteStartElement("x");
                xml.WriteString(XmlConvert.ToString(v.X));
                xml.WriteEndElement();
                xml.WriteStartElement("y");
                xml.WriteString(XmlConvert.ToString(v.Y));
                xml.WriteEndElement();
                xml.WriteStartElement("z");
                xml.WriteString(XmlConvert.ToString(v.Z));
                xml.WriteEndElement();
                xml.WriteStartElement("w");
                xml.WriteString(XmlConvert.ToString(v.W));
                xml.WriteEndElement();
            }
            public void Write(Node n)
            {
            }
        }

        public class IntegerInput : BasicInput
        {
            public override object Parse()
            {
                return int.Parse(GetLine(0));
            }
        }
        public class StringInput : BasicInput
        {
            public override object Parse()
            {
                return GetLine(0);
            }
        }
        public class FloatInput : BasicInput
        {
            public override object Parse()
            {
                return float.Parse(GetLine(0));
            }
        }
        public class BoolInput : BasicInput
        {
            public BoolInput()
            {
                Size = new Vector2(100, 50);
                Center();
            }
            public override object Parse()
            {
                return bool.Parse(GetLine(0));
            }
        }

        public class Menu : Window
        {
            public Menu()
                : base(0, 0, 200, Root.Instance.UserInterface.Renderer.Size.Y, new Layout(1, 3))
            {
                Color = new Color4f(0, 0, 0.5f, 0.0f);

                Classes = new ListBox(FindClasses());
                Properties = new ListBox();
                Properties.Visible = false;
                Entities = new ListBox();
                Entities.Visible = false;
                Add(Classes, 0, 1);
                Add(Properties, 0, 1);
                Add(Entities, 0, 1);
                Entities.SelectionChangedEvent += new ListBox.SelectionChangedDelegate(Entities_SelectionChangedEvent);
                Add(new Button(OnSpawnButtonPressed, "Spawn"), 0, 2);
                Add(ModeButton = new Button(OnModeButtonPressed, "Create"), 0, 0);
                Layout.Heights[0] = 48;
                Layout.Heights[1] = Size.y - 4 * 48;
                Layout.Heights[2] = 48;
                Layout.Update(Size);
            }

            ListBoxItem[] FindClasses()
            {
                Type[] nodes = Root.Instance.Factory.FindTypes(null, typeof(Node));
                nodes = Array.FindAll(nodes, delegate(Type t) { return t.GetCustomAttributes(typeof(EditableAttribute), false).Length > 0; });

                Array.Sort<Type>(nodes, delegate(Type t1, Type t2) { return string.Compare(t1.Name, t2.Name); });


                ListBoxItem[] items = new ListBoxItem[nodes.Length];
                for (int i = 0; i < items.Length; ++i)
                    items[i] = new ListBoxItem(nodes[i], null, nodes[i].Name);
                
                return items;
            }

            void Entities_SelectionChangedEvent(ListBox lb)
            {
                Select = (Entity)lb.Selected.Object;
                DisplayProperties();
            }
            public void OnSpawnButtonPressed(Button source, int button, float x, float y)
            {
                if (Classes.Visible)
                {
                    Node n = (Node)Root.Instance.Factory.CreateInstance((Type)Classes.Selected.Object);
                    Root.Instance.Scene.Spawn(n);
                    //Root.Instance.Scene.camera.Position = new Vector3(1000, 1000, 1000);
                    //Root.Instance.Scene.camera.LookAt(0, 0, 0);
                    Select = n;
                    DisplayProperties();
                }
                else
                {

                    if (Properties.Selected.Object is PropertyInfo)
                    {
                        PropertyInfo pi = (PropertyInfo)Properties.Selected.Object;
                        if (Input != null)
                        {
                            pi.SetValue(Select, Input.Parse(), null);
                            Root.Instance.Gui.windows.Remove(Input);
                            Input = null;
                            return;
                        }
                        if (pi.PropertyType == typeof(bool))
                        {
                            Input = new BoolInput();
                            Input.Display(pi.GetValue(Select, null));
                        }
                        else if (pi.PropertyType == typeof(int))
                        {
                            Input = new IntegerInput();
                            Input.Display(pi.GetValue(Select, null));
                        }
                        else if (pi.PropertyType == typeof(float))
                        {
                            Input = new FloatInput();
                            Input.Display(pi.GetValue(Select, null));
                        }
                        else if (pi.PropertyType == typeof(Vector3))
                        {
                            Input = new Vector3Input();
                            Input.Display(pi.GetValue(Select, null));
                        }
                        else if (pi.PropertyType == typeof(Quaternion))
                        {
                            Input = new QuaternionInput();
                            Input.Display(pi.GetValue(Select, null));
                        }
                        else if (pi.PropertyType == typeof(string))
                        {
                            Input = new StringInput();
                            Input.Display(pi.GetValue(Select, null));
                        }
                        else throw new Exception("incompatible type.");
                    }
                    else if (Properties.Selected.Object is FieldInfo)
                    {
                        FieldInfo fi = (FieldInfo)Properties.Selected.Object;
                        if (Input != null)
                        {
                            fi.SetValue(Select, Input.Parse());
                            Root.Instance.Gui.windows.Remove(Input);
                            Input = null;
                            return;
                        }
                        if (fi.FieldType == typeof(bool))
                        {
                            Input = new BoolInput();
                            Input.Display(fi.GetValue(Select));
                        }
                        else if (fi.FieldType == typeof(int))
                        {
                            Input = new IntegerInput();
                            Input.Display(fi.GetValue(Select));
                        }
                        else if (fi.FieldType == typeof(float))
                        {
                            Input = new FloatInput();
                            Input.Display(fi.GetValue(Select));
                        }
                        else if (fi.FieldType == typeof(Vector3))
                        {
                            Input = new Vector3Input();
                            Input.Display(fi.GetValue(Select));
                        }
                        else if (fi.FieldType == typeof(Quaternion))
                        {
                            Input = new QuaternionInput();
                            Input.Display(fi.GetValue(Select));
                        }
                        else if (fi.FieldType == typeof(string))
                        {
                            Input = new StringInput();
                            Input.Display(fi.GetValue(Select));
                        }
                        else throw new Exception("incompatible type.");
                    }
                    Root.Instance.Gui.windows.Add(Input);
                }
            }

            void DisplayProperties()
            {
                Type t = Select.GetType();
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
                    }
                }

                ListBoxItem[] items = new ListBoxItem[i];
                for (int j = 0; j < i; ++j)
                {
                    items[j] = new ListBoxItem(list[j], null, list[j].ToString());
                }
                Properties.SetContents(items);
            }

            public void DisplayEntities()
            {
                IList<Entity> list = Root.Instance.Scene.FindEntitiesByType<Entity>();

                int j = 0;
                for (int i = 0; i < list.Count; ++i)
                {
                    if (list[i].Kill || list[i].GetType().GetCustomAttributes(typeof(EditableAttribute), false).Length == 0)
                        j++;
                }

                ListBoxItem[] items = new ListBoxItem[list.Count - j];
                int k = 0;
                for (int i = 0; i < list.Count; ++i)
                {
                    if (!list[i].Kill && list[i].GetType().GetCustomAttributes(typeof(EditableAttribute), false).Length > 0)
                    {
                        items[k++] = new ListBoxItem(list[i], null, list[i].ToString());
                    }
                    else
                    {
                    }
                }

                Entities.SetContents(items);
            }

            public void OnModeButtonPressed(Button source, int button, float x, float y)
            {
                switch (ModeButton.Caption)
                {
                    case "Create":
                        ModeButton.Caption = "Modify";
                        Classes.Visible = false;
                        Properties.Visible = true;
                        Entities.Visible = false;
                        break;
                    case "Modify":
                        ModeButton.Caption = "Select";
                        Classes.Visible = false;
                        Properties.Visible = false;
                        Entities.Visible = true;
                        DisplayEntities();
                        break;
                    case "Select":
                        ModeButton.Caption = "Create";
                        Classes.Visible = true;
                        Properties.Visible = false;
                        Entities.Visible = false;
                        break;
                }
            }

            ListBox Classes;
            ListBox Properties;
            ListBox Entities;
            Button ModeButton;
            public Entity Select;
            IInput Input;
        }

        Menu menu;
        public override void Start()
        {
            base.Start();
            Root.Instance.Scene.camera = cam = new Camera();
            cam.Position = new Vector3(0f, 10000, -1000);
            cam.LookAt(0, 0, 0);
            //Root.Instance.Gui.windows.Add(menu = new Menu());
        }

        public Editor()
        {
            Root.Instance.Authoritive = true;
        }

        public virtual void LoadMap(string name)
        {
        }

        Camera cam;
        public string MapName = "NewMap";
        public string NameSpace = "SpaceWar2006.Maps";
        public string Using = "SpaceWar2006.GameObjects";

        public override void OnKeyPress(global::OpenTK.Input.Key k)
        {
            base.OnKeyPress(k);

            if (k == Key.S)
            {
                SaveCsMap(NameSpace, MapName, Using);
            }
            else if (k == Key.M)
            {
                LastMousePos = new Vector2(Root.Instance.UserInterface.Mouse.GetPosition(0), Root.Instance.UserInterface.Mouse.GetPosition(1));
                if (Mode == Action.Move)
                    Mode = Action.None;
                else
                    Mode = Action.Move;
            }
            else if (k == global::OpenTK.Input.Key.PageUp)
            {
                cam.Position += new Vector3(0, 1000, 0);
            }
            else if (k == global::OpenTK.Input.Key.PageDown)
            {
                cam.Position += new Vector3(0, -1000, 0);
            }
            else if (k == global::OpenTK.Input.Key.Delete)
            {
                if (Select != null)
                {
                    Select.Kill = true;
                    Select = null;
                    if(menu!=null)
                        menu.DisplayEntities();
                }
            }

        }
        enum Action
        {
            None = 0, Move, Rotate
        }
        Action Mode;

        public string SaveFileName = null;

        virtual protected void CsSave(Entity e, StreamWriter w)
        {
            if (e is Node)
            {
                Node n = (Node)e;
                w.WriteLine("e.Position=new Vector3(" + n.Position.X + "f," + n.Position.Y + "f," + n.Position.Z + "f);");
                w.WriteLine("e.rotationspeed=new Vector3(" + n.rotationspeed.X + "f," + n.rotationspeed.Y + "f," + n.rotationspeed.Z + "f);");
                w.WriteLine("e.Orientation=new Quaternion(" + n.Orientation.X + "f," + n.Orientation.Y + "f," + n.Orientation.Z + "f," + n.Orientation.W + "f);");
            }
            if (e is Light)
            {
                Light n = (Light)e;
                w.WriteLine("e.directional=" + (n.directional ? "true" : "false") + ";");
                w.WriteLine("e.diffuse=new Color4f(" + n.diffuse.r + "f," + n.diffuse.g + "f," + n.diffuse.b + "f," + n.diffuse.a + "f);");
            }
        }
        public void SaveCsMap(string namespace_, string name, string using_)
        {
            FileStream fs = new FileStream(SaveFileName, FileMode.Create, FileAccess.Write);
            StreamWriter w = new StreamWriter(fs);
            IList<Entity> list = Root.Instance.Scene.FindEntitiesByType<Entity>();

            w.WriteLine(@"using System;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Net;
using System.Collections.Generic;
using System.Threading;

using Cheetah;");

            w.WriteLine("using " + using_ + ";");


            w.WriteLine("namespace " + namespace_ + "{");


            w.WriteLine("public partial class " + name + " : Map{");

            //w.WriteLine("public "+name+"(){}");

            //w.WriteLine("public " + name + "(DeSerializationContext context): this(){DeSerialize(context);}");

            w.WriteLine("public override void Create(){");
            foreach (Entity e in list)
            {
                if (!IsSaveable(e))
                    continue;

                w.WriteLine("{");
                w.WriteLine(e.GetType() + " e=new " + e.GetType() + "();");

                CsSave(e, w);

                w.WriteLine("Spawn(e,true);");
                w.WriteLine("}");
            }
            w.WriteLine("}}}");
            w.Flush();
            w.Close();
            fs.Close();
        }

        public virtual bool IsSaveable(Entity e)
        {
            if (e is Camera)
                return false;

            return true;
        }

        Vector2 LastMousePos;
        public override void Tick(float dtime)
        {
            base.Tick(dtime);
            //CamControl.Tick(dtime);
            Vector2 p = new Vector2(Root.Instance.UserInterface.Mouse.GetPosition(0), Root.Instance.UserInterface.Mouse.GetPosition(1));
            Vector2 delta = p - LastMousePos;

                if (Select != null && Select is Node)
                {
                    Node n = (Node)Select;
                    if (Mode == Action.Move)
                    {
                        n.Position += new Vector3(delta.x * 10, 0, delta.y * 10);
                    }
                }

            if (Root.Instance.UserInterface.Mouse.GetButtonState(1))
            {
                cam.Position += new Vector3(delta.x * 10, 0, delta.y * 10);
            }
            LastMousePos = p;
        }

        public Entity Select
        {
            get
            {
                if (menu != null)
                    return menu.Select;
                else
                    return select;
            }
            set
            {
                select = value;
            }
        }
        Entity select;
        //StandardControl CamControl;
    }

    public class Node : Entity, IResource
    {
        public Node()
        {
        }

        public Node(DeSerializationContext context)
        {
            DeSerialize(context);
        }

        public void Dispose()
        {
        }

        public virtual void OnRemove(Scene s)
        {
        }

        public virtual CollisionInfo GetCollisionInfo()
        {
            return null;
        }



        public virtual bool CanCollide(Node other)
        {
            return true;
        }

        public virtual void OnCollide(Node other)
        {
        }

        protected virtual float SmoothTime
        {
            get
            {
                return 0.15f;
            }
        }

        public virtual void SetRenderParameters(IRenderer r, IDrawable draw, Shader shade)
        {
            if (shade != null)
            {
                int loc = shade.GetUniformLocation("Age");
                if (loc >= 0)
                {
                    r.SetUniform(loc, new float[] { age });
                }

            }
        }

        public Channel PlaySound(Sound s, bool loop)
        {
            if (Root.Instance.UserInterface==null)
                return null;
            Channel c = Root.Instance.UserInterface.Audio.Play(s, AbsolutePosition,loop);
            Root.Instance.Scene.Sounds[this] = c;
            return c;
        }

        public override void Tick(float dtime)
        {
            Matrix3 m = ((Quaternion)orientation).ToMatrix3();
            position.Tick(dtime, SmoothTime);
            speed.Tick(dtime, SmoothTime);
            position.Original += speed.Original * dtime;
            position.Original += m.Transform(localspeed) * dtime;

            orientation.Tick(dtime, SmoothTime);
            Quaternion qx = Quaternion.FromAxisAngle(1, 0, 0, rotationspeed.X * dtime);
            Quaternion qy = Quaternion.FromAxisAngle(0, 1, 0, rotationspeed.Y * dtime);
            Quaternion qz = Quaternion.FromAxisAngle(0, 0, 1, rotationspeed.Z * dtime);
            Quaternion q = qx * qy * qz;
            //q=Quaternion.Identity*Quaternion.Identity;
            orientation.Original = q * orientation.Original;

            age += dtime;

            if (Attach != null && Attach.Kill)
                Attach = null;

            if (Draw != null)
                foreach (object o in Draw)
                {
                    if (o is ITickable)
                    {
                        ((ITickable)o).Tick(dtime);
                    }
                }
        }

        public Vector3 Direction
        {
            get
            {
                Matrix3 m = Orientation.ToMatrix3();
                Vector3 forward = m.Transform(new Vector3(0, 0, 1));
                return forward;
            }
        }
        public Vector3 Left
        {
            get
            {
                Matrix3 m = Orientation.ToMatrix3();
                Vector3 forward = m.Transform(new Vector3(1, 0, 0));
                return forward;
            }
        }

        public void LookAt(float x, float y, float z)
        {
            LookAt(new Vector3(x, y, z));
        }

        public Vector3 AbsolutePosition
        {
            get
            {
                return Matrix.ExtractTranslation();
            }
        }
        public Vector3 SmoothAbsolutePosition
        {
            get
            {
                return SmoothMatrix.ExtractTranslation();
            }
        }

        public virtual Matrix3 SmoothMatrix
        {
            get
            {
                if (Attach == null)
                {
                    Matrix3 m = Matrix3.FromQuaternion(orientation.Smoothed);

                    m[12] = position.Smoothed.X;
                    m[13] = position.Smoothed.Y;
                    m[14] = position.Smoothed.Z;

                    return m;
                }
                else
                {
                    Matrix3 m = Matrix3.FromQuaternion(orientation.Smoothed);

                    m[12] = position.Smoothed.X;
                    m[13] = position.Smoothed.Y;
                    m[14] = position.Smoothed.Z;

                    return Attach.SmoothMatrix * m;
                }
            }
        }

        public virtual Matrix3 Matrix
        {
            get
            {
                if (Attach == null)
                {
                    Matrix3 m = Matrix3.FromQuaternion(Orientation);

                    m[12] = Position.X;
                    m[13] = Position.Y;
                    m[14] = Position.Z;

                    return m;
                }
                else
                {
                    if (Attach.Kill)
                        Kill = true;

                    Matrix3 m = Matrix3.FromQuaternion(Orientation);
                    //Matrix3 m = Matrix3.FromQuaternion(orientation.Smoothed);

                    m[12] = Position.X;
                    m[13] = Position.Y;
                    m[14] = Position.Z;
                    //m[12] = position.Smoothed.X;
                    //m[13] = position.Smoothed.Y;
                    //m[14] = position.Smoothed.Z;

                    return Attach.Matrix * m;
                }
            }
        }

        public void LookAt(Vector3 pos)
        {
            try
            {
                Vector3 z = pos - position;
                z.Normalize();
                Vector3 y = new Vector3(Up);
                Vector3 x = Vector3.Cross(y, z);
                if (x != Vector3.Zero)
                    x.Normalize();
                y = Vector3.Cross(z, x);
                //y.Normalize();

                //Matrix3 m1=Matrix3.FromQuaternion(orientation);
                //Quaternion q1=Quaternion.FromMatrix3(m1);

                //x.Z=-x.Z;
                //y.Z=-y.Z;
                //z.Z=-z.Z;
                Matrix3 m = Matrix3.FromBasis(x, y, z);
                //m.Invert();
                Orientation = Quaternion.FromMatrix3(m);
            }
            catch (DivideByZeroException)
            {
            }
        }

        public virtual void DeSerializeRefs(DeSerializationContext context)
        {
            int c = (int)context.ReadByte();
            if (c > 0)
                Draw = new ArrayList(c);

            for (int i = 0; i < c; ++i)
            {
                short typeid = context.ReadInt16();
                string path = context.ReadString();

                IResource res = Root.Instance.ResourceManager.Load(Root.Instance.FileSystem.Get(path), context.Factory.GetType(typeid));

                Draw.Add(res);
            }
        }

        public virtual void SerializeRefs(SerializationContext context)
        {
            if (Draw != null)
            {
                byte i = 0;
                foreach (object o in Draw)
                {
                    if (o is IResource)
                    {
                        i++;
                    }
                }

                context.Write(i);

                foreach (object o in Draw)
                {
                    if (o is IResource)
                    {
                        IResource r = (IResource)o;
                        //HACK
                        //w.Write(r.GetType().ToString());
                        context.Write(context.Factory.GetClassId(r.GetType()));
                        context.Write(Root.Instance.ResourceManager.Find(r).GetFullPath());
                    }
                    /*else if(o is ISerializable)
					{
						ISerializable r=(ISerializable)o;
						w.Write(r.GetType().ToString());
						r.Serialize(f,s,r);
					}*/
                }
            }
            else
                context.Write((byte)0);
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);

            Vector3 pos = Position + Speed * context.Latency;

            context.Write(pos.X); context.Write(pos.Y); context.Write(pos.Z);
            context.Write((short)(Speed.X * 100)); context.Write((short)(Speed.Y * 100)); context.Write((short)(Speed.Z * 100));
            //w.Write(Speed.X); w.Write(Speed.Y); w.Write(Speed.Z);
            context.Write(Orientation);
            //w.Write(Orientation.X); w.Write(Orientation.Y); w.Write(Orientation.Z); w.Write(Orientation.W);

            //w.Write(rotationspeed.X);w.Write(rotationspeed.Y);w.Write(rotationspeed.Z);
            //w.Write(localspeed.X);w.Write(localspeed.Y);w.Write(localspeed.Z);
            context.Write(age);

            int a = (Attach != null) ? Attach.ServerIndex : -1;
            context.Write(a);

            if (SyncRefs)
                SerializeRefs(context);

        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);

            Vector3 pos = context.ReadVector3();
            Vector3 s = new Vector3((float)context.ReadInt16() / 100.0f, (float)context.ReadInt16() / 100.0f, (float)context.ReadInt16() / 100.0f);

            pos += s * context.Latency;

            Quaternion q = context.ReadQuaternion();// new Quaternion((float)context.ReadByte() / 100.0f, (float)context.ReadByte() / 100.0f, (float)context.ReadByte() / 100.0f, (float)context.ReadByte() / 100.0f);
            float a = context.ReadSingle();

            if ((context.Flags & EntityFlags.ServerNoOverride) == 0)//nichts updaten, nur spezielle sachen
            {
                if (!Root.Instance.IsAuthoritive && age > 0.5f)
                {
                    //Vector3 delta = pos - Position;
                    //Position += delta / 2;
                    position.Original = pos;
                }
                else
                {
                    Position = position.Smoothed = pos;
                }

                Speed = s;

                Quaternion quat = q;
                if (!Root.Instance.IsAuthoritive && age > 0.5f)
                {
                    //Orientation = Quaternion.Slerp(Orientation, quat, 0.5f);
                    orientation.Original = quat;
                }
                else
                {
                    Orientation = orientation.Smoothed = quat;
                }
                age = a;
            }

            int attach = context.ReadInt32();
            Attach = (Node)Root.Instance.Scene.ServerListGet(attach);

            if (SyncRefs)
                DeSerializeRefs(context);
        }

        public override string ToString()
        {
            string s = base.ToString();
            //s+="|P:"+position.ToString()+"|S:"+speed.ToString()+"|O:"+orientation.ToString();
            return s;
        }

        [Editable]
        public virtual Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position.Original = value;
            }
        }

        public virtual Vector3 SmoothPosition
        {
            get
            {
                if (IsLocal)
                    return position;
                else
                    return position.Smoothed;
            }
        }
        public virtual Vector3 SmoothSpeed
        {
            get
            {
                if (IsLocal)
                    return speed;
                else
                    return speed.Smoothed;
            }
        }
        public virtual Vector3 Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed.Original = value;
            }
        }

        public virtual bool DrawLocal(IDrawable d)
        {
            if (d is ParticleSystem)
                return false;
            return true;
        }

        public float Distance(Node other)
        {
            return (AbsolutePosition - other.AbsolutePosition).GetMagnitude();
        }

        [Editable]
        public virtual Quaternion Orientation
        {
            get
            {
                return orientation;
            }
            set
            {
                orientation.Original = value;
            }
        }

        protected SmoothVector3 position;
        protected SmoothVector3 speed;
        protected SmoothQuaternion orientation = new SmoothQuaternion(Quaternion.Identity);
        //public ArrayList collide;
        public ArrayList Draw = new ArrayList();
        public Vector3 rotationspeed;
        public Vector3 localspeed;
        //public float scale=1.0f;
        public float age = 0;
        public Node Attach;
        public int Transparent = 0;
        public bool SyncRefs = true;
        public bool Visible = true;
        public static Vector3 Up = Vector3.YAxis;
        public float RenderRadius = -1;
    }


    public class VecRandom : Random
    {
        public VecRandom(int seed)
            : base(seed)
        {
        }

        public VecRandom()
        {
        }

        public Vector3 NextUnitVector3()
        {
            Vector3 v = new Vector3(
                     (float)NextDouble() - 0.5f,
                    (float)NextDouble() - 0.5f,
                    (float)NextDouble() - 0.5f
                    );
            v.Normalize();
            return v;
        }
        public Vector3 NextUnitScaledVector3(float scalex, float scaley, float scalez)
        {
            Vector3 v = new Vector3(
                     (float)NextDouble() - 0.5f,
                    (float)NextDouble() - 0.5f,
                    (float)NextDouble() - 0.5f
                    );
            v.Normalize();
            v.X *= scalex;
            v.Y *= scaley;
            v.Z *= scalez;
            return v;
        }
        public Vector3 NextScaledVector3(float scalex, float scaley, float scalez)
        {
            Vector3 v = new Vector3(
                     (float)NextDouble() * scalex,
                    (float)NextDouble() * scaley,
                    (float)NextDouble() * scalez
                    );
            return v;
        }
        public float NextFloat()
        {
            return (float)NextDouble();
        }

        public static VecRandom Instance = new VecRandom();
    }

    public interface IResource : IDisposable
    {
    }

    public interface ICustomResource : IResource
    {
        void Load(BinaryReader r);
    }

    public interface IResourceLoader
    {
        IResource Load(FileSystemNode n);
        Type LoadType
        {
            get;
        }
        bool CanLoad(FileSystemNode n);
    }

    public interface ISaver<T>
    {
        void Save(T obj, Stream s);
    }

    public abstract class CollisionInfo
    {
        public abstract bool Check(CollisionInfo other);
        public abstract bool Check(SphereCollisionInfo other);
        public abstract bool Check(RayCollisionInfo other);
        public abstract bool Check(AABBCollisionInfo other);
        public abstract bool Check(BoxCollisionInfo other);
    }

    public class AlwaysCollisionInfo : CollisionInfo
    {
        public override bool Check(CollisionInfo other)
        {
            return true;
        }
        public override bool Check(RayCollisionInfo other)
        {
            return true;
        }
        public override bool Check(SphereCollisionInfo other)
        {
            return true;
        }

        public override bool Check(AABBCollisionInfo other)
        {
            return true;
        }
        public override bool Check(BoxCollisionInfo other)
        {
            return true;
        }
    }

    public class BoxCollisionInfo : CollisionInfo
    {
        public BoxCollisionInfo(Matrix3 matrix, BoundingBox bbox)
        {
            //Center = center;
            BBox = bbox;
            //Orientation = orientation;
            //Matrix = Matrix3.FromTranslation(center) * Matrix3.FromQuaternion(Quaternion);
            Matrix = matrix;
            InvMatrix = matrix.GetInverse();
        }

        //Vector3 Center;
        //Quaternion Orientation;
        BoundingBox BBox;
        Matrix3 Matrix;
        Matrix3 InvMatrix;

        public override bool Check(CollisionInfo other)
        {
            return other.Check(this);
        }
        public override bool Check(RayCollisionInfo other)
        {
            //return false;

            Vector3 v1 = InvMatrix * other.Ray.Start;
            Vector3 v2 = InvMatrix * other.Ray.End;

            return intersect(new Ray(v1, v2), BBox);
        }
        public override bool Check(SphereCollisionInfo other)
        {
            Vector3 spherecenter = InvMatrix * other.Sphere.Center;
            float r = other.Sphere.Radius;

            if (spherecenter.X + r < BBox.Min.X ||
                spherecenter.Y + r < BBox.Min.Y ||
                spherecenter.Z + r < BBox.Min.Z ||
                spherecenter.X - r > BBox.Max.X ||
                spherecenter.Y - r > BBox.Max.Y ||
                spherecenter.Z - r > BBox.Max.Z
                )
            {
                return false;
            }

            return true;
        }

        public override bool Check(AABBCollisionInfo other)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override bool Check(BoxCollisionInfo other)
        {
            Vector3 v1, v2;
            Matrix3 m1 = other.InvMatrix * Matrix;
            Matrix3 m2 = InvMatrix * other.Matrix;

            foreach (Vector3 p in GetPoints(other.BBox))
            {
                v1 = m2 * p;
                if (PointInBox(v1, BBox))
                    return true;
            }
            foreach (Vector3 p in GetPoints(BBox))
            {
                v1 = m1 * p;
                if (PointInBox(v1, other.BBox))
                    return true;
            }

            return false;
        }

        bool intersect(Ray r, BoundingBox box)
        {
            Vector3 inv_direction = new Vector3(1 / r.Direction.X, 1 / r.Direction.Y, 1 / r.Direction.Z);
            int[] sign = new int[3]{
                (inv_direction.X < 0)?1:0,
                (inv_direction.Y < 0)?1:0,
                (inv_direction.Z < 0)?1:0
        };


            Vector3[] parameters = new Vector3[2]
            {
                box.Min,
                box.Max
            };
            float tmin, tmax, tymin, tymax, tzmin, tzmax;
            float t0 = 0;
            float t1 = r.Length;

            tmin = (parameters[sign[0]].X - r.Start.X) * inv_direction.X;
            tmax = (parameters[1 - sign[0]].X - r.Start.X) * inv_direction.X;
            tymin = (parameters[sign[1]].Y - r.Start.Y) * inv_direction.Y;
            tymax = (parameters[1 - sign[1]].Y - r.Start.Y) * inv_direction.Y;
            if ((tmin > tymax) || (tymin > tmax))
                return false;
            if (tymin > tmin)
                tmin = tymin;
            if (tymax < tmax)
                tmax = tymax;
            tzmin = (parameters[sign[2]].Z - r.Start.Z) * inv_direction.Z;
            tzmax = (parameters[1 - sign[2]].Z - r.Start.Z) * inv_direction.Z;
            if ((tmin > tzmax) || (tzmin > tmax))
                return false;
            if (tzmin > tmin)
                tmin = tzmin;
            if (tzmax < tmax)
                tmax = tzmax;
            return ((tmin < t1) && (tmax > t0));
        }

        bool PointInBox(Vector3 v, BoundingBox bbox)
        {
            if (v.X < bbox.Min.X ||
                v.Y < bbox.Min.Y ||
                v.Z < bbox.Min.Z ||
                v.X > bbox.Max.X ||
                v.Y > bbox.Max.Y ||
                v.Z > bbox.Max.Z
                )
            {
                return false;
            }
            return true;
        }

        Vector3[] GetPoints(BoundingBox bbox)
        {
            return new Vector3[8]
            {
                bbox.Min,
                new Vector3(bbox.Min.X,bbox.Min.Y,bbox.Max.Z),
                new Vector3(bbox.Min.X,bbox.Max.Y,bbox.Min.Z),
                new Vector3(bbox.Max.X,bbox.Min.Y,bbox.Min.Z),
                bbox.Max,
                new Vector3(bbox.Max.X,bbox.Max.Y,bbox.Min.Z),
                new Vector3(bbox.Min.X,bbox.Max.Y,bbox.Max.Z),
                new Vector3(bbox.Max.X,bbox.Min.Y,bbox.Max.Z)
            };
        }
    }

    public class SphereCollisionInfo : CollisionInfo
    {
        public Sphere Sphere;

        public SphereCollisionInfo(Vector3 pos, float r)
        {
            Sphere = new Sphere(pos, r);
        }

        public override bool Check(CollisionInfo other)
        {
            return other.Check(this);
        }
        public override bool Check(RayCollisionInfo other)
        {
            return other.Check(this);
        }
        public override bool Check(SphereCollisionInfo other)
        {
            float dist = (Sphere.Center - other.Sphere.Center).GetMagnitude();
            return dist <= Sphere.Radius + other.Sphere.Radius;
        }

        public override bool Check(AABBCollisionInfo other)
        {
            return other.Check(this);
        }
        public override bool Check(BoxCollisionInfo other)
        {
            return other.Check(this);
        }
    }

    public class RayCollisionInfo : CollisionInfo
    {
        public Ray Ray;

        public RayCollisionInfo(Vector3 start, Vector3 end)
        {
            Ray = new Ray(start, end);
        }

        public override bool Check(CollisionInfo other)
        {
            return other.Check(this);
        }
        public override bool Check(SphereCollisionInfo other)
        {
            return Ray.Intersect(other.Sphere);
        }

        public override bool Check(AABBCollisionInfo other)
        {
            return other.Check(this);
        }
        public override bool Check(RayCollisionInfo other)
        {
            return false;
        }
        public override bool Check(BoxCollisionInfo other)
        {
            return other.Check(this);
        }
    }

    public class AABBCollisionInfo : CollisionInfo
    {
        public override bool Check(CollisionInfo other)
        {
            return other.Check(this);
        }
        public override bool Check(RayCollisionInfo other)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override bool Check(SphereCollisionInfo other)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public override bool Check(AABBCollisionInfo other)
        {
            throw new Exception("The method or operation is not implemented.");
        }
        public override bool Check(BoxCollisionInfo other)
        {
            return other.Check(this);
        }

        public Vector3 Center;
        public Vector3 Size;
    }

    public interface ICollisionMesh
    {
        void FillBegin();
        void Fill(Vector3 ta, Vector3 tb, Vector3 tc);
        void FillEnd();
    }


    public class HtmlTable
    {
        string GetClass()
        {
            if (Class == null)
                return "";
            else
                return " class=\"" + Class + "\"";
        }

        void Write(string s)
        {
            Builder.Append(s);
        }
        void WriteLine(string s)
        {
            Builder.AppendLine(s);
        }

        void Write()
        {
            WriteLine("<table" + GetClass() + ">");
            for (int i = 0; i < Rows.Count; ++i)
            {
                Write("<tr>");
                for (int j = 0; j < Rows[i].Length; ++j)
                {
                    WriteLine("<td>" + Rows[i][j].ToString() + "</td>");
                }
                Write("</tr>");
            }
            WriteLine("</table>");
        }
        public override string ToString()
        {
            Builder = new StringBuilder();
            Write();
            return Builder.ToString();
        }

        StringBuilder Builder;
        public string Class;
        public List<object[]> Rows = new List<object[]>();
    }

    public class WebPage
    {
        public virtual void Execute(IDictionary<string, string> Parameters, Stream Output, TextWriter Text, BinaryWriter Bin)
        {
        }
    }

    public class WebServer : IDisposable
    {
        public WebServer(int port, string name, string password)
        {
            Name = name;
            Password = password;
            listener = new TcpListener(port);
            listener.Start();
            listenthread = new Thread(new ThreadStart(Accept));
            listenthread.Start();
            Console.WriteLine("Webserver listening on port " + port);
            Port = port;
            FindPages();
        }

        void FindPages()
        {
            Type[] tl = Root.Instance.Factory.FindTypes(null, typeof(WebPage));

            foreach (Type t in tl)
            {
                FieldInfo f = t.GetField("Url", BindingFlags.Public | BindingFlags.Static);
                if (f != null)
                {
                    WebPage page = (WebPage)Activator.CreateInstance(t);
                    string url = (string)f.GetValue(null);
                    Console.WriteLine("page: " + url);
                    Pages[url] = page;
                }
            }
        }

        void Accept()
        {
            TcpClient c;
            try
            {
                while ((c = listener.AcceptTcpClient()) != null)
                {
                    //Thread t = new Thread(new ParameterizedThreadStart(Work));
                    //t.Start(c);
                    Work(c);
                }
            }
            catch (SocketException)
            {
            }
        }

        public void Shutdown()
        {
            Console.WriteLine("stopping threads.");
            if (listener != null)
            {
                listener.Stop();
                listener = null;
            }
            if (listenthread != null)
            {
                listenthread.Join();
                listenthread = null;
            }
        }

        public void Dispose()
        {
            Shutdown();
        }

        private string ToBase64(string str)
        {
            Encoding asciiEncoding = Encoding.ASCII;
            byte[] byteArray = new byte[asciiEncoding.GetByteCount(str)];
            byteArray = asciiEncoding.GetBytes(str);
            return Convert.ToBase64String(byteArray, 0, byteArray.Length);
        }

        public string Name;
        public string Password;

        bool CheckPermission(string auth)
        {
            if (auth.StartsWith("Basic "))
            {
                auth = auth.Substring(6);
                return auth == ToBase64(Name + ":" + Password);
            }
            else
                return false;
        }

        void Work(object p)
        {
            TcpClient client = (TcpClient)p;
            Stream stream = client.GetStream();
            StreamReader r = new StreamReader(stream);
            StreamWriter w = new StreamWriter(stream);
            string s = r.ReadLine();
            //Console.WriteLine(s);

            string[] split = s.Split(' ');

            string method = split[0];

            //Console.WriteLine("method: " + method);
            MemoryStream ms = new MemoryStream();
            StreamWriter msw = new StreamWriter(ms);


            Dictionary<string, string> header = new Dictionary<string, string>();
            client.Client.Blocking = false;
            try
            {
                string[] split2;
                while ((s = r.ReadLine()) != null)
                {
                    split2 = s.Split(':');
                    if (split2.Length == 2)
                        header[split2[0].Trim()] = split2[1].Trim();
                }
            }
            catch (IOException)
            {
            }


            if (method.ToLower() == "get")
            {
                string path = split[1];
                string version = split[2];
                Dictionary<string, string> param = new Dictionary<string, string>();

                if (path.IndexOf('?') >= 0)
                {
                    try
                    {
                        string paramstring = path.Split('?')[1];
                        path = path.Split('?')[0];
                        string[] parameters = paramstring.Split('&');
                        foreach (string kv in parameters)
                        {
                            string k = HttpUtility.UrlDecode(kv.Split('=')[0]);
                            string v = HttpUtility.UrlDecode(kv.Split('=')[1]);

                            param[k] = v;
                            //Console.WriteLine("key: " + k + " value: " + v);
                        }
                    }
                    catch (Exception)
                    {
                    }
                }

                if (!header.ContainsKey("Authorization") || !CheckPermission(header["Authorization"]))
                {
                    w.Write("HTTP/1.1" + " 401 Unauthorized" + "\r\n");
                    w.Write("WWW-Authenticate: Basic" + "\r\n");
                }
                else
                {
                    FileSystemNode root = Root.Instance.FileSystem.Get("web");
                    Stream code = null;
                    try
                    {
                        code = Root.Instance.FileSystem.Get(root, path).getStream();
                    }
                    catch (Exception)
                    {
                    }



                    if (code != null)
                    {
                        //Console.WriteLine("http ok!");

                        w.Write("HTTP/1.1" + " 200 OK" + "\r\n");
                        w.Write("Server: localhost\r\n");
                        w.Write("Content-Type: " + "text/html" + "\r\n");
                        w.Write("Accept-Ranges: bytes\r\n");
                        w.Write("\r\n\r\n");
                        //w.Write("Content-Length: " + data.Length.ToString() + "\r\n\r\n");

                        if (path.EndsWith(".boo"))
                        {
                            Root.Instance.Script.SetValue("web_writer", msw);
                            Root.Instance.Script.SetValue("web_url", path);
                            Root.Instance.Script.SetValue("web_get", param);
                            Root.Instance.Script.Execute(code);
                            msw.Flush();
                            string data = Encoding.ASCII.GetString(ms.GetBuffer());
                            w.Write(data);
                        }
                        else
                        {
                            byte[] buf = new byte[(int)code.Length];
                            code.Read(buf, 0, buf.Length);
                            stream.Write(buf, 0, buf.Length);
                            //Console.WriteLine("sent " + buf.Length + " bytes.");
                        }
                    }
                    else if (Pages.ContainsKey(path))
                    {
                        WebPage page = Pages[path];
                        BinaryWriter bw = new BinaryWriter(stream);
                        {
                            page.Execute(param, stream, w, bw);
                        }
                        bw.Flush();
                    }
                    else
                    {
                        w.Write("HTTP/1.1" + " 404 Not Found." + "\r\n");
                        w.Write("Server: localhost\r\n");
                        w.Write("\r\n");
                    }
                }
            }


            w.Flush();
            stream.Flush();
            client.Close();
        }
        TcpListener listener;
        Thread listenthread;
        public int Port;
        Dictionary<string, WebPage> Pages = new Dictionary<string, WebPage>();
    }

    public class CustomResourceLoader : IResourceLoader
    {
        public IResource Load(FileSystemNode n)
        {
            Stream s = n.getStream();
            if (s == null) throw new Exception("cant load " + n.ToString() + ": cant get stream.");

            BinaryReader r = new BinaryReader(s);
            Assembly a = Assembly.GetCallingAssembly();
            string name = r.ReadString();
            Type t = a.GetType(name);
            /*try
            {
                //o=(Object)a.CreateInstance(t.FullName);
                o=(Object)Activator.CreateInstance(t);
            }
            catch(Exception ex)
            {
                o=(Object)Activator.CreateInstance(t,new object[]{e});
            }*/
            //object o=Activator.CreateInstance(null,name);

            object o = (object)Activator.CreateInstance(t);
            if (o == null) throw new Exception("cant create " + name);
            ICustomResource cr = (ICustomResource)o;
            cr.Load(r);

            return null;
        }

        public Type LoadType
        {
            get { return typeof(IResource); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            if (n.info == null) return false;

            return n.info.Extension == "cheetah";
        }
    }

    public class ArchiveLoader : IResourceLoader
    {
        public IResource Load(FileSystemNode n)
        {
            IArchive a = new ZipArchive();
            a.Open(n.getStream());
            return a;
        }

        public Type LoadType
        {
            get { return typeof(IArchive); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            if (n.info == null) return false;

            return n.info.Extension == ".zip";
        }
    }

    public class SoundLoader : IResourceLoader
    {
        public IResource Load(FileSystemNode n)
        {
            if (Root.Instance.UserInterface==null)
            {
                return new Sound(null);
            }
            else
            {
                return Root.Instance.UserInterface.Audio.Load(n.getStream());
            }
        }

        public Type LoadType
        {
            get { return typeof(Sound); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            if (n.info == null) return false;

            return n.info.Extension.ToLower() == ".wav" || n.info.Extension.ToLower() == ".mp3" || n.info.Extension.ToLower() == ".ogg" || n.info.Extension.ToLower() == ".xm";
        }
    }

    public class ImageLoader : IResourceLoader
    {
        public IResource Load(FileSystemNode n)
        {
            //return new DotNetImageDecoder();
            IImageDecoder i;
//#if WINDOWS
            //if (n.GetName().ToLower().EndsWith(".bmp"))
                i = new DotNetImageDecoder();
            //else
//#endif
                //i=new SDLImageDecoder();
                //i=new DotNetImageDecoder();
            //    i = new DevIlImageDecoder();

            //IImageDecoder i=new LibPngImageDecoder();
            i.Load(n.getStream());
            //if(Root.Instance.UserInterface==null)
            //	return new DummyTexture();
            //Texture t=Root.Instance.UserInterface.getRenderer().CreateTexture(i.getRGBA(),i.getWidth(),i.getHeight(),i.hasAlpha());
            ImageResource img = new ImageResource(i.getWidth(), i.getHeight(), i.getRGBA(), i.hasAlpha());
            return img;
        }

        public Type LoadType
        {
            get { return typeof(ImageResource); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            if (n.info == null) return false;

            return n.info.Extension.ToLower() == ".bmp" || n.info.Extension.ToLower() == ".png" || n.info.Extension.ToLower() == ".jpg";
        }
    }

    public class AssemblyResource : IResource
    {
        public AssemblyResource(Assembly a)
        {
            Assembly = a;
        }

        public void Dispose()
        {
        }

        public Assembly Assembly;
    }

    public class CSharpLoader : IResourceLoader
    {
        public IResource Load(FileSystemNode n)
        {
            Compiler c = new Compiler();

            Assembly a = c.Compile(n.getStream());

            return new AssemblyResource(a);
        }

        public Type LoadType
        {
            get { return typeof(AssemblyResource); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            if (n.info == null) return false;

            return n.info.Extension.ToLower() == ".cs";
        }
    }

    public class AssemblyLoader : IResourceLoader
    {
        public IResource Load(FileSystemNode n)
        {
            Compiler c = new Compiler();
            BinaryReader r = new BinaryReader(n.getStream());
            byte[] image = r.ReadBytes((int)r.BaseStream.Length);

            Assembly a = Assembly.Load(image);

            return new AssemblyResource(a);
        }

        public Type LoadType
        {
            get { return typeof(AssemblyResource); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            if (n.info == null) return false;

            return n.info.Extension.ToLower() == ".dll";
        }
    }

    public class TextureLoader : IResourceLoader
    {
        protected Texture LoadCubeTexture(FileSystemNode n)
        {
            StreamReader r = new StreamReader(n.getStream());
            string line;

            string header = r.ReadLine().Trim();
            if (header != "CUBEMAPTEXT")
                return null;

            IImageDecoder xpos = null;
            IImageDecoder xneg = null;
            IImageDecoder ypos = null;
            IImageDecoder yneg = null;
            IImageDecoder zpos = null;
            IImageDecoder zneg = null;

            while ((line = r.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;

                if (line.StartsWith("+x:"))
                {
                    string filename = line.Split(new char[] { ':' })[1].Trim();
                    FileSystemNode n1 = (FileSystemNode)n.parent[filename];
                    Stream s = n1.getStream();
                    xpos = CreateDecoder(n1);
                    xpos.Load(s);
                    continue;
                }
                else if (line.StartsWith("-x:"))
                {
                    string filename = line.Split(new char[] { ':' })[1].Trim();
                    FileSystemNode n1 = (FileSystemNode)n.parent[filename];
                    Stream s = n1.getStream();
                    xneg = CreateDecoder(n1);
                    xneg.Load(s);
                    continue;
                }
                else if (line.StartsWith("+y:"))
                {
                    string filename = line.Split(new char[] { ':' })[1].Trim();
                    FileSystemNode n1 = (FileSystemNode)n.parent[filename];
                    Stream s = n1.getStream();
                    ypos = CreateDecoder(n1);
                    ypos.Load(s);
                    continue;
                }
                else if (line.StartsWith("-y:"))
                {
                    string filename = line.Split(new char[] { ':' })[1].Trim();
                    FileSystemNode n1 = (FileSystemNode)n.parent[filename];
                    Stream s = n1.getStream();
                    yneg = CreateDecoder(n1);
                    yneg.Load(s);
                    continue;
                }
                else if (line.StartsWith("+z:"))
                {
                    string filename = line.Split(new char[] { ':' })[1].Trim();
                    FileSystemNode n1 = (FileSystemNode)n.parent[filename];
                    Stream s = n1.getStream();
                    zpos = CreateDecoder(n1);
                    zpos.Load(s);
                    continue;
                }
                else if (line.StartsWith("-z:"))
                {
                    string filename = line.Split(new char[] { ':' })[1].Trim();
                    FileSystemNode n1 = (FileSystemNode)n.parent[filename];
                    Stream s = n1.getStream();
                    zneg = CreateDecoder(n1);
                    zneg.Load(s);
                    continue;
                }
            }

            if (xpos == null || xneg == null ||
                ypos == null || yneg == null ||
                zpos == null || zneg == null
                )
            {
                throw new Exception();
            }

            int w = xpos.getWidth();
            int h = xpos.getHeight();

            return new Texture(Root.Instance.UserInterface.Renderer.CreateCubeTexture(
                xpos.getRGBA(), xneg.getRGBA(), ypos.getRGBA(), yneg.getRGBA(), zpos.getRGBA(), zneg.getRGBA(), w, h));
        }

        TextureFormat ConvertFormat(DDS.TextureFormat f)
        {
            return (TextureFormat)f;
        }

        public Texture LoadDDS(Stream s)
        {
            DDS.DdsFile f = new DDS.DdsFile(s);
            Texture t;

            if (f.Format == DDS.TextureFormat.RGB || f.Format == DDS.TextureFormat.RGBA)
            {
                t = new Texture(Root.Instance.UserInterface.Renderer.CreateTexture(f.MipMaps[0], f.Header.Width, f.Header.Height, f.Format == DDS.TextureFormat.RGBA));
            }
            else
            {
                if (f.Header.IsCubeMap)
                {
                    t = new Texture(Root.Instance.UserInterface.Renderer.CreateCompressedCubeTexture(f.CubeMaps[0], f.CubeMaps[1], f.CubeMaps[2], f.CubeMaps[3], f.CubeMaps[4], f.CubeMaps[5], ConvertFormat(f.Format), f.Header.Width, f.Header.Height));

                }
                else
                {
                    t = new Texture(
                        Root.Instance.UserInterface.Renderer.CreateCompressedTexture(f.MipMaps, ConvertFormat(f.Format), f.Header.Width, f.Header.Height));
                }
            }
            return t;
        }

        public IResource Load(FileSystemNode n)
        {
            if (Root.Instance.UserInterface == null)
                return new DummyTexture();

            if (n.GetName().ToLower().EndsWith(".avi"))
            {
                VideoTexture t = new VideoTexture(n.getStream());
                return t;
            }
            else if (n.GetName().ToLower().EndsWith(".cubemap"))
            {
                return LoadCubeTexture(n);
            }
            else if (n.GetName().ToLower().EndsWith(".dds"))
            {
                return LoadDDS(n.getStream());
            }
            else
            {
                IImageDecoder i = CreateDecoder(n);

                //IImageDecoder i=new LibPngImageDecoder();
                i.Load(n.getStream());
                Texture t = new Texture(Root.Instance.UserInterface.Renderer.CreateTexture(i.getRGBA(), i.getWidth(), i.getHeight(), i.hasAlpha()));
                return t;
            }
        }

        protected IImageDecoder CreateDecoder(FileSystemNode n)
        {
            return new DotNetImageDecoder();
            IImageDecoder i;
//#if WINDOWS
            if (n.GetName().ToLower().EndsWith(".bmp"))
                i = new DotNetImageDecoder();
            else
//#endif
                //i=new SDLImageDecoder();
                //i=new DotNetImageDecoder();
                i = new DevIlImageDecoder();

            return i;
        }

        public Type LoadType
        {
            get { return typeof(Texture); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            if (n.info == null) return false;

            return n.info.Extension.ToLower() == ".dds" || n.info.Extension.ToLower() == ".cubemap" || n.info.Extension.ToLower() == ".dds" || n.info.Extension.ToLower() == ".tga" || n.info.Extension.ToLower() == ".bmp" || n.info.Extension.ToLower() == ".png" || n.info.Extension.ToLower() == ".jpg" || n.info.Extension.ToLower() == ".avi";
        }
    }
    /*
        public class ResourceMap : ISerializable
        {
            public struct Entry
            {
                public Entry(string path,Type type,IResource r)
                {
                    Path=path;
                    Type=type;
                    Resource=r;
                }
                public string Path;
                public Type Type;
                public IResource Resource;
            }
		
            public ResourceMap(Entry[] entries)
            {
                Entries=entries;
            }

            public ResourceMap(Factory f,Stream s,BinaryReader r)
            {
                DeSerialize(f,s,r);
            }

            public virtual void Serialize(Factory f,Stream s,BinaryWriter w)
            {
                w.Write(Entries.Length);
                foreach(Entry e in Entries)
                {
                    w.Write(e.Path);
                    w.Write(e.Type.FullName);
                    if(e.Resource!=null)
                    {
                        w.Write(true);
                        f.Serialize(s,(ISerializable)e.Resource);
                    }
                    else
                    {
                        w.Write(false);
                    }
                }
            }
		
            public void Register()
            {
                foreach(Entry e in Entries)
                {
                    if(e.Resource!=null)
                    {
                        FileSystemNode n=Root.Instance.FileSystem.CreatePath(null,e.Path);
                        Root.Instance.ResourceManager.Register(n,e.Resource,e.Type);
                    }
                    else
                        Root.Instance.ResourceManager.Load(Root.Instance.FileSystem.Get(e.Path),e.Type);
                }
            }

            public virtual void DeSerialize(Factory f,Stream s,BinaryReader r)
            {
                Entries=new Entry[r.ReadInt32()];
                for(int i=0;i<Entries.Length;++i)
                {
                    Entries[i].Path=r.ReadString();
                    Entries[i].Type=f.GetType(r.ReadString());
                    if(r.ReadBoolean())
                    {
                        Entries[i].Resource=(IResource)f.DeSerialize(s);
                    }
                }
            }

            Entry[] Entries;
        }
    */
    public class HeightMapLoader : IResourceLoader
    {
        public IResource Load(FileSystemNode n)
        {
            HeightMapImage hm = new HeightMapImage(Root.Instance.ResourceManager.LoadImage(n));

            return hm;
        }

        public Type LoadType
        {
            get { return typeof(IHeightMap); }
        }

        public bool CanLoad(FileSystemNode n)
        {
            if (n.info == null) return false;

            return n.info.Extension == ".bmp" || n.info.Extension == ".png" || n.info.Extension == ".jpg";
        }
    }

    public class ResourceManager : ITickable, IDisposable
    {
        public ResourceManager(FileSystem fs, Factory fac)
        {
            Fs = fs;
            resources = new Hashtable();
            loaders = new ArrayList();
            Type[] loadertypes = fac.FindTypes(new Type[] { typeof(IResourceLoader) }, null);
            foreach (Type t in loadertypes)
            {
                loaders.Add(fac.CreateInstance(t));
            }

            AddSearchPath(typeof(Mesh), "models");
            AddSearchPath(typeof(Texture), "textures");
            AddSearchPath(typeof(SkyBox), "objects");
            AddSearchPath(typeof(Terrain), "objects");
            AddSearchPath(typeof(IHeightMap), "textures");
            AddSearchPath(typeof(Shader), "shaders/glsl");
            AddSearchPath(typeof(Sound), "audio");
            AddSearchPath(typeof(Font), "fonts");
            AddSearchPath(".");
        }

        /*~ResourceManager()
        {
            Dispose();
        }*/

        public void Dispose()
        {
            if (!Disposed)
            {
                foreach (DictionaryEntry de1 in resources)
                {
                    foreach (DictionaryEntry de2 in (Hashtable)de1.Value)
                    {
                        if (de2.Value is IDisposable)
                            ((IDisposable)de2.Value).Dispose();
                    }
                }
                resources = null;
                Disposed = true;
            }
        }

        public FileSystemNode SearchFileNode(string _path)
        {
            FileSystemNode n = null;

            foreach (string s in path)
            {
                try
                {
                    n = Fs.Get(Fs.Get(s), _path);
                }
                catch (Exception)
                {
                    continue;
                }
                break;
            }
            if (n == null)
                throw new Exception("file not found: " + _path);

            return n;
        }

        private FileSystemNode SearchFileNode(string _path, Type t)
        {
            FileSystemNode n = null;
            string tp = null;
            if (typepath.ContainsKey(t))
                tp = typepath[t];

            foreach (string s in path)
            {
                try
                {
                    n = Fs.Get(Fs.Get(s), _path);
                }
                catch (Exception)
                {
                    if (tp == null)
                        continue;
                    try
                    {
                        n = Fs.Get(Fs.Get(Fs.Get(s), tp), _path);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                    break;
                }
                break;
            }
            if (n == null)
                throw new Exception("file not found: " + _path);

            return n;
        }

        public IResource Load(string _path)
        {
            FileSystemNode n = SearchFileNode(_path);

            return Load(n);
        }

        public IResource Load(string _path, Type t)
        {
            FileSystemNode n = SearchFileNode(_path, t);

            return Load(n, t);
        }

        public IResource Load(FileSystemNode n)
        {
            if (resources.ContainsKey(n))
            {
                Hashtable types = (Hashtable)resources[n];
                foreach (DictionaryEntry de in types)
                    return (IResource)de.Value;
            }
            foreach (IResourceLoader l in loaders)
            {
                if (l.CanLoad(n))
                {
                    Console.WriteLine("ResourceManager: Loading " + n.GetFullPath() + " with " + l.GetType().ToString() + ".");
                    IResource o = l.Load(n);
                    //Console.WriteLine("...Type is "+ o.GetType().ToString() +".");
                    if (o != null)
                    {
                        Hashtable types;
                        if (resources.ContainsKey(n))
                        {
                            types = (Hashtable)resources[n];
                        }
                        else
                        {
                            types = new Hashtable();
                            resources[n] = types;
                        }
                        types.Add(o.GetType(), o);
                        return o;
                    }
                }
            }
            throw new Exception("cant load " + n.ToString());
        }

        public IResource Load(FileSystemNode n, Type t)
        {
            if (resources.ContainsKey(n))
            {
                Hashtable types = (Hashtable)resources[n];
                if (types.ContainsKey(t))
                    return (IResource)types[t];
            }

            foreach (IResourceLoader l in loaders)
            {
                if (l.LoadType == t && l.CanLoad(n))
                {
                    Console.WriteLine("Loading " + n.GetFullPath() + " with " + l.GetType().Name + ".");
                    IResource o = l.Load(n);
                    if (o != null)
                    {
                        Hashtable types;
                        if (resources.ContainsKey(n))
                        {
                            types = (Hashtable)resources[n];
                        }
                        else
                        {
                            types = new Hashtable();
                            resources[n] = types;
                        }
                        types.Add(t, o);
                        return o;
                    }
                }
            }

            /*foreach(Type i in t.GetInterfaces())
            {
                if(i==typeof(ICustomResource))
                {
                }
            }*/

            /*if(Array.IndexOf(t.GetInterfaces(),typeof(ICustomResource))>=0)
            {
                ICustomResource r=(ICustomResource)Root.Instance.Factory.CreateInstance(t);
				
                r.Load(new BinaryReader(n.getStream()));
                resources.Add(n,r);
                return r;
            }*/

            throw new Exception("cant load " + n.ToString());
            //return null;
        }

        public void Register(FileSystemNode n, IResource r,Type t)
        {
            Hashtable types;
            if (resources.ContainsKey(n))
            {
                types = (Hashtable)resources[n];
            }
            else
            {
                types = new Hashtable();
                resources[n] = types;
            }
            types[t] = r;
        }

        public IResource Get(FileSystemNode n, Type t)
        {
            return (IResource)((Hashtable)resources[n])[t];
        }
        public Hashtable Get(FileSystemNode n)
        {
            return (Hashtable)resources[n];
        }
        public FileSystemNode Find(IResource r)
        {
            foreach (DictionaryEntry de in resources)
            {
                Hashtable types = (Hashtable)de.Value;
                foreach (DictionaryEntry de2 in types)
                {
                    if (de2.Value == r)
                        return (FileSystemNode)de.Key;
                }
            }
            throw new Exception("cant find " + r.ToString());
        }

        public bool IsLoaded(FileSystemNode n, Type t)
        {
            if (resources.ContainsKey(n))
            {
                Hashtable types = (Hashtable)resources[n];
                return types.ContainsKey(t);
            }
            return false;
        }

        public void UnloadAll()
        {
            List<FileSystemNode> all = new List<FileSystemNode>();
            foreach (DictionaryEntry de in resources)
            {
                all.Add((FileSystemNode)de.Key);
            }
            foreach (FileSystemNode n in all)
                Unload(n);
        }

        public void Unload(string name)
        {
            Unload(SearchFileNode(name));
        }

        public void Unload(string name, Type t)
        {
            Unload(SearchFileNode(name, t));
        }

        public void Unload(FileSystemNode n)
        {
            Hashtable ht = Get(n);
            Console.WriteLine("Unloading " + n.ToString());
            foreach (DictionaryEntry de in ht)
            {
                IResource r = (IResource)de.Value;
                Type t = (Type)de.Key;
                r.Dispose();
            }
            //r.Dispose();
            resources.Remove(n);
        }
        public void Unload(FileSystemNode n, Type t)
        {
            Console.WriteLine("Unloading " + n.ToString() + ", Type: " + t.ToString());
            IResource r = Get(n, t);
            r.Dispose();
            Get(n).Remove(t);
        }

        /*public void Unload(object o)
        {
			
        }*/

        public Texture LoadTexture(FileSystemNode n)
        {
            return (Texture)Load(n, typeof(Texture));
        }

        public IHeightMap LoadHeightMap(FileSystemNode n)
        {
            return (IHeightMap)Load(n, typeof(IHeightMap));
        }

        public IArchive LoadArchive(FileSystemNode n)
        {
            return (IArchive)Load(n, typeof(IArchive));
        }

        public Mesh LoadMesh(FileSystemNode n)
        {
            return (Mesh)Load(n, typeof(Mesh));
        }

        public ImageResource LoadImage(FileSystemNode n)
        {
            return (ImageResource)Load(n, typeof(ImageResource));
        }

        public Texture LoadTexture(string path)
        {
            return (Texture)Load(path, typeof(Texture));
        }

        public IHeightMap LoadHeightMap(string path)
        {
            return (IHeightMap)Load(path, typeof(IHeightMap));
        }

        public Sound LoadSound(string path)
        {
            return (Sound)Load(path, typeof(Sound));
        }

        public T Load<T>(string path)
        {
            return (T)Load(path, typeof(T));
        }

        public IArchive LoadArchive(string path)
        {
            return (IArchive)Load(path, typeof(IArchive));
        }

        public Font LoadFont(string path)
        {
            return (Font)Load(path, typeof(Font));
        }

        public Shader LoadShader(string path)
        {
            return (Shader)Load(path, typeof(Shader));
        }

        public Config LoadConfig(string path)
        {
            return (Config)Load(path, typeof(Config));
        }

        public Mesh LoadMesh(string path)
        {
            return (Mesh)Load(path, typeof(Mesh));
        }

        public ImageResource LoadImage(string path)
        {
            return (ImageResource)Load(path, typeof(ImageResource));
        }

        public AssemblyResource LoadCSharp(FileSystemNode n)
        {
            return (AssemblyResource)Load(n, typeof(AssemblyResource));
        }
        public AssemblyResource LoadCSharp(string path)
        {
            return (AssemblyResource)Load(path, typeof(AssemblyResource));
        }
        public AssemblyResource LoadAssembly(FileSystemNode n)
        {
            return (AssemblyResource)Load(n, typeof(AssemblyResource));
        }
        public AssemblyResource LoadAssembly(string path)
        {
            return (AssemblyResource)Load(path, typeof(AssemblyResource));
        }
        public void Tick(float dtime)
        {
            foreach (DictionaryEntry de1 in resources)
            {
                foreach (DictionaryEntry de2 in (Hashtable)de1.Value)
                {
                    if (de2.Value is ITickable)
                        ((ITickable)de2.Value).Tick(dtime);
                }
            }
        }


        public override string ToString()
        {
            string s = "";
            foreach (DictionaryEntry de1 in resources)
            {
                foreach (DictionaryEntry de2 in (Hashtable)de1.Value)
                {
                    FileSystemNode n = (FileSystemNode)de1.Key;
                    Type t = de2.Value.GetType();
                    s += t.ToString() + ": " + de2.Value.ToString() + "\n";
                }
            }
            return s;
        }
        public string[] GetAllNames()
        {
            string[] s = new string[resources.Count];
            int i = 0;
            foreach (DictionaryEntry de1 in resources)
            {
                FileSystemNode n = (FileSystemNode)de1.Key;
                s[i++] = n.ToString();
            }
            return s;
        }

        public void PrintAllNames()
        {
            string[] s = GetAllNames();
            foreach (string l in s)
                Console.WriteLine(l);
        }

        public void AddSearchPath(string dir)
        {
            path.Insert(0, dir);
            //path.Add(dir);
        }
        public void AddSearchPath(Type t, string dir)
        {
            //typepath.Insert(0, dir);
            typepath.Add(t, dir);
        }

        Hashtable resources;
        bool Disposed = false;
        ArrayList loaders;
        FileSystem Fs;

        List<string> path = new List<string>();
        Dictionary<Type, string> typepath = new Dictionary<Type, string>();
    }

    public class SaveContext
    {
        public SaveContext(Stream s)
        {
            Stream = s;
            Writer = new StreamWriter(s);
        }

        public void Save(ISaveable s, bool canbenull)
        {
            if (canbenull)
            {
                if (s == null)
                    Writer.Write(false);
                else
                {
                    Writer.Write(true);
                    s.Save(this);
                }
            }
        }

        public Stream Stream;
        public StreamWriter Writer;
    };

    public interface ISaveable
    {
        void Save(SaveContext sc);
    };

    public class FileSystemNode : Hashtable
    {
        //int nextindex=1;
        public FileSystemNode parent;

        public FileSystemNode()
        {
        }

        public FileSystemNode(FileInfo fi)
        {
            info = fi;
        }

        /*public int Add(object obj)
        {
            while(ContainsKey(nextindex))nextindex++;
            Add(nextindex,obj);
            return nextindex++;
        }*/

        public void Truncate()
        {
            Access = FileAccess.ReadWrite;
            info.Open(FileMode.Truncate, Access, FileShare.ReadWrite).Close();
        }

        public virtual FileSystemNode Create(string name)
        {
            if (ContainsKey(name))
            {
                //throw new Exception(this.dir.FullName+": file exists: " + name);
                FileSystemNode n1 = (FileSystemNode)this[name];
                Console.WriteLine("deleting file " + n1.info.FullName);
                n1.info.Delete();
                this.Remove(name);
            }

            FileSystemNode n = new FileSystemNode();
            Add(name, n);
            n.Access = FileAccess.ReadWrite;
            return n;
        }


        public virtual FileSystemNode CreateFile(string name)
        {
            if (dir == null)
                throw new Exception("cant create file: " + this.GetFullPath() + " is not a directory.");

            FileSystemNode n = Create(name);

            string filename = dir.FullName + Path.DirectorySeparatorChar + name;

            FileInfo fi = new FileInfo(filename);
            Console.WriteLine("creating file " + fi.FullName);
            fi.Create().Close();
            //Thread.Sleep(2000);
            //fi.();
            //fi.OpenRead();
            n.info = fi;

            return n;
        }

        public override void Add(object key, object value)
        {
            //if(value is FileSystemNode)
            //{
            ((FileSystemNode)value).parent = this;
            //}
            base.Add((string)key, value);
        }

        public override string ToString()
        {
            if (info != null)
                return info.FullName;
            return base.ToString();
        }

        public string GetName()
        {
            if (parent == null)
                return "";

            foreach (DictionaryEntry e in parent)
            {
                if (e.Value == this)
                    return (string)e.Key;
            }

            throw new Exception("failure in FileSystem tree.");
        }

        public string GetFullPath()
        {
            FileSystemNode n;
            //ArrayList l=new ArrayList();
            string s = "";

            for (n = this; n != null; n = n.parent)
            {
                s = n.GetName() + "/" + s;
            }

            s = s.TrimEnd(new char[] { '/', '\\' });

            return s;
        }

        public byte[] Md5
        {
            get
            {
                if (_Md5 == null)
                    _Md5 = (new MD5CryptoServiceProvider()).ComputeHash(getStream());
                return _Md5;
            }
        }

        public string Md5String
        {
            get
            {
                string s = "";
                const string seperator = "";
                for (int i = 0; i < Md5.Length; ++i)
                {
                    s += (i > 0 ? seperator : "") + Md5[i].ToString("X2");
                }
                return s;
            }
        }

        public virtual Stream getStream()
        {
            return info.Open(FileMode.Open, Access, FileShare.ReadWrite);
        }

        public int Size
        {
            get
            {
                if (info != null)
                    return (int)info.Length;
                else
                    return 0;
            }
        }

        public FileInfo info;
        public DirectoryInfo dir;
        protected byte[] _Md5;
        public FileAccess Access = FileAccess.Read;

    }

    public interface ISerializable
    {
        //void Serialize(Factory f,Stream s,BinaryWriter w);
        //void DeSerialize(Factory f,Stream s,BinaryReader r);
        void Serialize(SerializationContext context);
        void DeSerialize(DeSerializationContext context);
    }

    public class SerializationContext
    {
        public byte[] ToArray()
        {
            return Message.PeekDataBuffer();
        }

        public Lidgren.Network.NetOutgoingMessage GetMessage()
        {
            return Message;
        }

        public SerializationContext(Factory f, Lidgren.Network.NetOutgoingMessage m)
        {
            Factory = f;
            Message = m;
        }

        public void Serialize(ISerializable s)
        {
            Factory.Serialize(this,s);
        }
        public void Write(Vector3 v)
        {
            Message.Write(v.X);
            Message.Write(v.Y);
            Message.Write(v.Z);
        }
        public void Write(Quaternion q)
        {
            Message.Write(q.X);
            Message.Write(q.Y);
            Message.Write(q.Z);
            Message.Write(q.W);
        }
        public void WriteRangedSingle(float val, float min, float max,int bits)
        {
            Message.WriteRangedSingle(val, min, max, bits);
        }

        public void Write(int i, int bits)
        {
            Message.Write(i, bits);
        }
        public void Write(string s)
        {
            Message.Write(s);
        }
        public void Write(int i)
        {
            Message.Write(i);
        }
        public void Write(byte i)
        {
            Message.Write(i);
        }
        public void Write(short i)
        {
            Message.Write(i);
        }
        public void Write(float i)
        {
            Message.Write(i);
        }
        public void Write(bool i)
        {
            Message.Write(i);
        }
        public float Latency
        {
            get
            {
                if (Root.Instance.IsAuthoritive)
                {
                    return serverlatency;
                }
                else
                {
                    return clientlatency;
                }
            }
        }
        public Factory Factory;
        private Lidgren.Network.NetOutgoingMessage Message;
        //private Stream Stream;
        //private BinaryWriter Writer;
        private static float serverlatency = Root.Instance.ResourceManager.LoadConfig("config/global.config").GetFloat("server.sendlatency");
        private static float clientlatency = Root.Instance.ResourceManager.LoadConfig("config/global.config").GetFloat("client.sendlatency");
    }

    public class DeSerializationContext
    {
        public DeSerializationContext(Lidgren.Network.NetIncomingMessage msg)
        {

            Factory = Root.Instance.Factory;
            //Stream = s;
            //Reader = r;
            //Message = new Lidgren.Library.Network.NetMessage((int)(s.Length - s.Position));
            //Message.Write(r.ReadBytes((int)(s.Length - s.Position)));
            //System.Console.WriteLine("deserialization: " + s.Length);
            Message = msg;
        }

        public DeSerializationContext(Factory f, Stream s, BinaryReader r)
        {
            Factory = f;
            //Stream = s;
            //Reader = r;
            int len = (int)(s.Length - s.Position);
            Message = new Lidgren.Network.NetIncomingMessage(r.ReadBytes(len), len);
            //System.Console.WriteLine("deserialization: " + s.Length);
        }

        public Vector3 ReadVector3()
        {
            return new Vector3(Message.ReadSingle(), Message.ReadSingle(), Message.ReadSingle());
        }

        public float Latency
        {
            get
            {
                if (Root.Instance.IsAuthoritive)
                {
                    return serverlatency;
                }
                else
                {
                    return clientlatency;
                }
            }
        }

        public Factory Factory;

        //private Stream Stream;
        //private BinaryReader Reader;
        Lidgren.Network.NetIncomingMessage Message;

        public EntityFlags Flags = EntityFlags.None;
        private static float serverlatency = Root.Instance.ResourceManager.LoadConfig("config/global.config").GetFloat("server.recvlatency");
        private static float clientlatency = Root.Instance.ResourceManager.LoadConfig("config/global.config").GetFloat("client.recvlatency");

        public ISerializable DeSerialize()
        {
            return Factory.DeSerialize(this);
        }

        public string ReadString()
        {
            return Message.ReadString();
        }

        public int ReadInt32()
        {
            return Message.ReadInt32();
        }
        public int ReadInt(int bits)
        {
            return Message.ReadInt32(bits);
        }
        public short ReadInt16()
        {
            return Message.ReadInt16();
        }

        public float ReadSingle()
        {
            return Message.ReadSingle();
        }

        public byte ReadByte()
        {
            return Message.ReadByte();
        }

        public bool ReadBoolean()
        {
            return Message.ReadBoolean();
        }


        public Quaternion ReadQuaternion()
        {
            return new Quaternion(
                Message.ReadSingle(),
                Message.ReadSingle(),
                Message.ReadSingle(),
                Message.ReadSingle()
            );
        }
    }

    public class Factory
    {
        public const string CLASSFILE = "system/classes.txt";

        public Factory()
        {
            //UpdateTypeIds();
            
            Add(Assembly.GetExecutingAssembly());
            Add(Assembly.GetEntryAssembly());

            /*foreach(string fi in Directory.GetFiles(".","*.dll"))
            {
                try
                {
                    Assembly a=Assembly.LoadFrom(fi);
                    Assemblies.Add(a);
                }
                catch(Exception e)
                {
                }
            }*/

        }

        public Factory(Stream s)
        {
            LoadClassIds(s);
            Add(Assembly.GetExecutingAssembly());
            Add(Assembly.GetEntryAssembly());
            foreach (Assembly a in Root.Instance.Assemblies)
                Add(a);
        }

        private bool UpdateTypeIds(Assembly a)
        {
            bool modified = false;

            try
            {
                Type[] types = a.GetTypes();
                foreach (Type t in types)
                {
                    if (t.IsClass && t.IsPublic)
                    {
                        if (ClassNames.ContainsKey(t.FullName))
                        {
                            short id = (short)ClassNames[t.FullName];
                            //object o = ClassIds[id];

                            if (!ClassIds.ContainsKey(id))
                            {
                                ClassIds[id] = t;
                                //Console.WriteLine("type of new loaded assembly is already known: " + t.Name);
                            }
                            else
                            {
                            }
                        }
                        else
                        {
                            //Console.WriteLine("generating id for new type: " + t.Name);
                            while (ClassIds.ContainsKey(id))
                            {
                                id++;
                            }
                            ClassIds[id] = t;
                            ClassNames[t.Name] = ClassNames[t.FullName] = id++;
                            modified = true;
                            continue;
                        }
                    }
                }
            }
            catch (System.Reflection.ReflectionTypeLoadException e)
            {
                System.Console.WriteLine(e.Message);
                foreach (Exception e2 in e.LoaderExceptions)
                {
                    if(e2!=null)
                        System.Console.WriteLine(e2.ToString()+": "+e2.Message);
                }
                foreach (Type t in e.Types)
                {
                    if (t != null)
                        System.Console.WriteLine(t.ToString());
                }
                throw e;
            }

            return modified;
        }

        short id = 1;
        public void UpdateTypeIds()
        {
            FileSystem fs = Root.Instance.FileSystem;
            //string classesfile = "config/classes.types";

            FileSystemNode n=fs.Get(CLASSFILE);

            if (n!=null)
            {
                if (n.Size > 0)
                    LoadClassIds(n.getStream());
            }

            id = (short)(ClassIds.Count + 1);
            bool modified = false;

            foreach (Assembly a in Assemblies)
            {
                modified = UpdateTypeIds(a) || modified;
            }
            //Console.WriteLine("loaded " + (id - 1) + " classes.");

            //SaveClassIds("classes.types");
            if (modified)
                SaveClassIds(fs.CreateFile(CLASSFILE).getStream());
        }

        public object CreateInstance(string typename)
        {
            return CreateInstance(GetType(typename));
        }

        public object CreateInstance(string typename, object[] param)
        {
            return CreateInstance(GetType(typename), param);
        }

        /*public void LoadClassIds(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            LoadClassIds(fs);
            fs.Close();
        }*/

        public void LoadClassIds(Stream s)
        {
            StreamReader r = new StreamReader(s);
            string line;

            string header;
            try
            {
                header = r.ReadLine().Trim();
            }
            catch (NullReferenceException)
            {
                r.Close();
                //r.Dispose();
                s.Close();
                //s.Dispose();
                return;
            }

            if (header != "CLASSTEXT")
            {
                throw new Exception("");
            }

            ClassIds.Clear();
            ClassNames.Clear();

            while ((line = r.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;

                string[] split = line.Split(new char[] { ':' });
                string key = split[0].Trim();
                string val = split[1].Trim();
                short id = short.Parse(key);

                if (ClassIds.ContainsKey(id))
                    throw new Exception("duplicate id.");

                ClassNames[val] = id;
                //Console.WriteLine(val.GetHashCode().ToString());
                try
                {
                    Type t = GetType(val);
                    ClassIds[id] = t;

                }
                catch (Exception)
                {
                    //Console.WriteLine("problem: " + val + " not unknown.");
                    //ClassIds[id] = val;
                }
            }
            r.Close();
            s.Close();
        }

        /*public void SaveClassIds(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            SaveClassIds(fs);
            fs.Close();
        }*/

        public void SaveClassIds(Stream s)
        {
            StreamWriter w = new StreamWriter(s);
            w.WriteLine("CLASSTEXT");
            foreach (KeyValuePair<short,Type> de in ClassIds)
            {
                w.Write((short)de.Key);
                w.Write(": ");
                w.WriteLine(de.Value.FullName);
            }
            w.Flush();
            w.Close();
            s.Close();
        }

        public short GetClassId(Type t)
        {
            //HACK very slow!
            foreach (KeyValuePair<short,Type> de in ClassIds)
            {
                if (t.FullName == de.Value.FullName)
                    return de.Key;
            }
            throw new Exception("unknown type: " + t.ToString());
        }

        public short GetClassId(string typename)
        {
            return (short)ClassNames[typename];
        }

        public Type GetType(short classid)
        {
            Type t = (Type)ClassIds[classid];
            if (t == null)
                throw new CantFindTypeException("cant find type " + classid);
            return t;
        }

        public class CantFindTypeException : Exception
        {
            public CantFindTypeException(string text)
                : base(text)
            {
            }
        }

        public Type GetType(string typename)
        {
            try
            {
                if (!ClassNames.ContainsKey(typename))
                    throw new CantFindTypeException("cant find type(1) " + typename);
                short id = (short)ClassNames[typename];
                if (!ClassIds.ContainsKey(id))
                    throw new CantFindTypeException("cant find type(2) " + typename);
                /*if (ClassIds[id] is string)
                {
                    Console.WriteLine("wrong type: " + (string)ClassIds[id] + ", " + typename);
                }*/
                return ClassIds[id];
            }
            catch (CantFindTypeException e)
            {
                foreach (Assembly a in Assemblies)
                {
                    Type t = a.GetType(typename, false);
                    if (t != null)
                    {
                        return t;
                    }
                }
                throw e;
            }
        }

        public Type[] FindTypes(Type[] interfaces, Type type)
        {
            return FindTypes(interfaces, type, true);
        }

        public Type[] FindTypes(Type[] interfaces, Type type, bool allowabstract)
        {
            ArrayList l = new ArrayList();
            foreach (Assembly a in Assemblies)
            {
                Type[] t = FindTypes(a, interfaces, type, allowabstract);
                l.AddRange(t);
            }
            return (Type[])l.ToArray(typeof(Type));
        }

        public Type[] FindTypes(Assembly a, Type[] interfaces,Type type)
        {
            return FindTypes(a, interfaces, type, true);
        }

        public Type[] FindTypes(Assembly a, Type[] interfaces, Type type, bool allowabstract)
        {
            Type[] ts = a.GetExportedTypes();
            ArrayList l = new ArrayList();
            foreach (Type t in ts)
            {
                if (type == null || t.IsSubclassOf(type) || t == type)
                {
                    if (allowabstract || !t.IsAbstract)
                    {
                        Type[] i = t.GetInterfaces();
                        bool ok2 = true;
                        if (interfaces != null)
                        {
                            foreach (Type i2 in interfaces)
                            {
                                bool ok = false;
                                foreach (Type i1 in i)
                                {
                                    if (i2 == i1)
                                    {
                                        ok = true;
                                        break;
                                    }
                                }
                                if (!ok)
                                {
                                    ok2 = false;
                                    break;
                                }
                            }
                        }
                        if (ok2)
                        {
                            l.Add(t);
                        }
                    }
                }
            }
            return (Type[])l.ToArray(typeof(Type));
        }
        public object CreateInstance(Type t)
        {
            return CreateInstance(t, null);
        }

        public object CreateInstance(Type t, object[] param)
        {
            foreach (Assembly a in Assemblies)
            {
                try
                {
                    object o = a.CreateInstance(t.FullName, false, BindingFlags.CreateInstance, null, param, null, null);
                    if (o == null)
                        continue;
                    return o;
                    //return Activator.CreateInstance(a.FullName,t.FullName,false,BindingFlags.CreateInstance,null,param,null,null,null);
                }
                catch (MissingFieldException)
                {
                }
                catch (TargetInvocationException e)
                {
                    throw new Exception(e.InnerException.Message + e.InnerException.Source + e.InnerException.StackTrace, e.InnerException);
                }
            }
            throw new Exception("cant find type " + t.FullName);
        }

        public ISerializable DeSerialize(Stream s)
        {
            BinaryReader r = new BinaryReader(s);
            //HACK
            //string type=r.ReadString();
            short type = r.ReadInt16();

            return DeSerialize(s, r, GetType(type));
        }
        public ISerializable DeSerialize(DeSerializationContext c)
        {
            //BinaryReader r = new BinaryReader(s);
            //HACK
            //string type=r.ReadString();
            short type = c.ReadInt16();

            return DeSerialize(c, GetType(type));
        }
        public ISerializable DeSerialize(DeSerializationContext c, Type t)
        {
            return (ISerializable)CreateInstance(t, new object[] { c });
        }
        public ISerializable DeSerialize(Stream s, BinaryReader r, Type t)
        {
            return (ISerializable)CreateInstance(t, new object[] { new DeSerializationContext(this, s, r) });
        }

        /*public void Serialize(Stream s, ISerializable obj)
        {
            BinaryWriter w = new BinaryWriter(s);
            //string type=obj.GetType().FullName;
            //w.Write(type);

            //HACK
            short type = GetClassId(obj.GetType().FullName);
            w.Write(type);

            //int i = Root.Instance.TickCount();
            obj.Serialize(new SerializationContext(this, s, w));
            //i = Root.Instance.TickCount() - i;
            //if (i > 50)
            //    Console.WriteLine(i.ToString());
            w.Flush();
            s.Flush();
        }*/

        public void Serialize(SerializationContext context, ISerializable obj)
        {
            //BinaryWriter w = new BinaryWriter(s);
            //string type=obj.GetType().FullName;
            //w.Write(type);

            //HACK
            short type = GetClassId(obj.GetType().FullName);
            context.Write(type);

            //int i = Root.Instance.TickCount();
            obj.Serialize(context);
            //i = Root.Instance.TickCount() - i;
            //if (i > 50)
            //    Console.WriteLine(i.ToString());
            //w.Flush();
            //s.Flush();
        }
        public void Add(Assembly a)
        {
            if (Assemblies.Contains(a))
            {
                //throw new Exception("assembly already added.");
                Console.WriteLine("assembly already added: " + a.FullName);
                return;
            }

            Console.WriteLine("new assembly: " + a.FullName);

            Assemblies.Add(a);

            if (UpdateTypeIds(a))
            {
                //FileSystem fs = Root.Instance.FileSystem;

                //SaveClassIds(fs.CreateFile(CLASSFILE).getStream());
            }
        }
        private ArrayList Assemblies = new ArrayList();
        private Hashtable ClassNames = new Hashtable();
        //private Hashtable ClassIds = new Hashtable();
        private Dictionary<short, Type> ClassIds = new Dictionary<short, Type>();
    }

    public class Compiler
    {
        public Assembly Compile(string sourcecode)
        {
            CSharpCodeProvider provider = new CSharpCodeProvider();
            CompilerParameters parameters = new CompilerParameters(new string[] { "bin\\windows\\Cheetah.dll", "System.Xml.dll", "mscorlib.dll", "System.dll", "mods\\spacewar2006\\bin\\game.dll" });
            parameters.GenerateExecutable = false;

            //parameters.GenerateInMemory = true;
            //parameters.OutputAssembly = "mods\\spacewar2006\\.dll";
            CompilerResults result = provider.CompileAssemblyFromSource(parameters, sourcecode);

            foreach (CompilerError e in result.Errors)
                Console.WriteLine(e.ToString());
            return result.CompiledAssembly;
        }

        public Assembly Compile(Stream s)
        {
            StreamReader r = new StreamReader(s);
            return Compile(r.ReadToEnd());
        }
    }

    public class Flow
    {
        public virtual void Start()
        {
            Time = 0;
            Root.Instance.Quit = false;
        }

        public virtual void Tick(float dtime)
        {
            Time += dtime;

        }

        public virtual ISerializable Query()
        {
            return null;
        }

        public virtual void OnJoin(short clientid, string name)
        {
        }
        public virtual void OnLeave(short clientid, string name)
        {
        }
        public virtual void OnDraw()
        {
        }
        public virtual void OnEntityKill(Entity e)
        {
        }

        public virtual void OnEvent(EventReplicationInfo eventinfo)
        {
        }

        public virtual void Stop()
        {
            Finished = true;
            GC.Collect();
        }

        public virtual void OnKeyPress(global::OpenTK.Input.Key k)
        {
        }

        public float Time;
        public bool Finished = false;
        public int Return;

        public virtual void OnCollision(Entity e1, Entity e2)
        {
        }
    }

    public class InputEntity : Entity
    {
        public InputEntity()
        {
        }
    }

    public class PlayerEntity : Entity
    {
        public PlayerEntity(short clientid, string name)
        {
            ClientId = clientid;
            Name = name;
        }
        public override void OnDeserializeEntityData()
        {
            base.OnDeserializeEntityData();

            if (Root.Instance.IsAuthoritive && ServerIndex != 0)
            {
                Console.WriteLine("server takes control over " + ToString());
                OwnerNumber = 0;
            }
        }
        public PlayerEntity(DeSerializationContext context)
        {
            DeSerialize(context);
            //Console.WriteLine(Name + " joined the game.");
        }

        public override void Tick(float dTime)
        {
            base.Tick(dTime);
            /*if (Root.Instance.IsAuthoritive && ClientId != 0)
            {
                UdpServer.Slot s = ((UdpServer)Root.Instance.Connection).GetClient(ClientId);
                if (s == null)
                    Kill = true;
                else
                    RTT = (short)(s.AvgRTT * 1000);
            }*/


        }
        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);
            context.Write(ClientId);
            context.Write(Name);
            context.Write(RTT);
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);
            ClientId = context.ReadInt16();
            Name = context.ReadString();
            RTT = context.ReadInt16();
        }

        public override void OnKill()
        {
            base.OnKill();
            //Console.WriteLine(Name + " left the game.");
        }
        public string Name;
        public short ClientId;
        public short RTT;
    }

    public class EventReplicationInfo : ISerializable
    {
        public EventReplicationInfo(DeSerializationContext context)
        {
            DeSerialize(context);
        }

        public EventReplicationInfo(string functionname, Entity target)
        {
            FunctionName = functionname;
            Target = target;
        }
        public EventReplicationInfo(string functionname, Entity target, string[] param)
        {
            FunctionName = functionname;
            Target = target;
            Parameters = param;
        }

        public override string ToString()
        {
            string target;
            if (Target != null)
                target = Target.ToString();
            else
                target = "#unknown#";
            string param = "";
            if (Parameters != null)
            {
                for (int i = 0; i < Parameters.Length; ++i)
                {
                    param += Parameters[i];
                    if (i != Parameters.Length - 1)
                        param += ",";
                }
            }
            else
                param = "#none#";
            return "Event: " + target + "." + FunctionName + "(" + param + ")";
        }

        public void Raise()
        {
            if (Target != null)
            {
                Type t = Target.GetType();
                t.InvokeMember(FunctionName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod, null, Target, Parameters);
            }
        }

        #region ISerializable Members

        public void Serialize(SerializationContext context)
        {
            context.Write(FunctionName);

            if (Parameters == null)
                context.Write((byte)0);
            else
            {
                context.Write((byte)Parameters.Length);
                for (int i = 0; i < Parameters.Length; ++i)
                    context.Write(Parameters[i]);
            }

            if (Target != null)
            {
                if (Target.ServerIndex == 0 && Root.Instance.IsAuthoritive)
                    throw new Exception();
                context.Write(Target.ClientIndex);
                context.Write(Target.OwnerNumber);
                context.Write(Target.ServerIndex);
            }
            else
            {
                context.Write(-1);
                context.Write((short)-1);
                context.Write(1);
                System.Console.WriteLine("event: target unknown during serialization.");
            }
        }

        public void DeSerialize(DeSerializationContext context)
        {
            FunctionName = context.ReadString();

            int n = (int)context.ReadByte();
            if (n != 0)
            {
                Parameters = new string[n];
                for (int i = 0; i < n; ++i)
                {
                    Parameters[i] = context.ReadString();
                }
            }

            int clientindex = context.ReadInt32();
            short ownernumber = context.ReadInt16();
            int serverindex = context.ReadInt32();

            Target = Root.Instance.Scene.Find(ownernumber, clientindex, serverindex);
            if (Target == null)
            {
                System.Console.WriteLine("event: target unknown: " + serverindex + ", " + ownernumber + ", " + clientindex);
                //throw new Exception();
            }
        }

        #endregion

        public string FunctionName;
        public Entity Target;
        public string[] Parameters = null;
    }

    public class Entity : ISerializable, ITickable
    {
        public int ServerIndex;
        public short OwnerNumber;//ClientNumber, 0=Server
        public int ClientIndex;//ClientIndex@Owner
        public bool Dirty;
        public bool Override;
        public bool NoReplication = false;
        public Scene Scene;

        private bool kill = false;

        public bool Kill
        {
            get
            {
                return kill;
            }
            set
            {
                //Console.WriteLine("kill: " + value.ToString() + " " + ToString());
                if (!kill && value)
                {
                    Scene.MarkKill(this);
                }
                kill = value;
            }
        }

        public Entity(DeSerializationContext context)
        {
            DeSerialize(context);
        }

        public Entity()
        {
        }

        protected void ReplicateCall(string function, string[] args)
        {
            Root.Instance.EventSendQueue.Add(new EventReplicationInfo(function, this, args));

        }

        public virtual void OnDeserializeEntityData()
        {
        }

        public virtual void Timer(int seconds)
        {
        }
        public virtual void OnAdd(Scene s)
        {
        }
        public virtual void OnKill()
        {
        }

        public virtual void Tick(float dTime)
        {
        }
        /*
        public void Test()
        {
            Root.Instance.EventSendQueue.Add(new EventReplicationInfo("Test", this));
        }
        */
        public bool IsLocal
        {
            get
            {
                if (Root.Instance.Connection == null)
                {
                    return Root.Instance.Player == null;
                }
                return
                    (!Root.Instance.IsAuthoritive && OwnerNumber == ((UdpClient)Root.Instance.Connection).ClientNumber)
                    ||
                    (Root.Instance.IsAuthoritive && OwnerNumber == 0);
            }
        }

        public override string ToString()
        {
            return GetType().Name + ", O#" + OwnerNumber + " C#:" + ClientIndex + " S#:" + ServerIndex + (NoReplication ? " *" : "");
        }

        public virtual void Serialize(SerializationContext context)
        {
            context.Write(ServerIndex);
            context.Write(OwnerNumber);
            context.Write(ClientIndex);
        }

        public virtual void DeSerialize(DeSerializationContext context)
        {
            ServerIndex = context.ReadInt32();
            OwnerNumber = context.ReadInt16();
            ClientIndex = context.ReadInt32();

            OnDeserializeEntityData();
        }



    }

    public class ArchiveFileNode : FileSystemNode
    {
        public ArchiveFileNode(ArchiveEntry e)
        {
            entry = e;
        }

        public override Stream getStream()
        {
            return entry.GetStream();
        }

        protected ArchiveEntry entry;
    }

    public class ArchiveEntry
    {
        public ArchiveEntry(string n, IArchive a)
        {
            name = n;
            archive = a;
        }

        public Stream GetStream()
        {
            return archive.GetFileStream(name);
        }

        public string name;
        public IArchive archive;
    }

    public interface IArchive : IResource, IEnumerable
    {
        void Open(Stream s);
        Stream GetFileStream(string name);
    }


    public class ZipCompressor
    {
        public int Compress(byte[] input, int inputlength, byte[] output)
        {
            d.Reset();
            d.SetInput(input, 0, inputlength);
            d.Flush();
            int n = d.Deflate(output);
            //if (!d.)
            //   Console.WriteLine("ERROR");
            //Console.WriteLine(inputlength.ToString()+"->"+n.ToString());
            //Console.WriteLine(input[4].ToString() + input[5].ToString());
            return n;
        }
        public int Decompress(byte[] input, int inputlength, byte[] output)
        {
            i.Reset();
            i.SetInput(input, 0, inputlength);
            //i.Flush();
            int bufferlength = i.Inflate(output);
            //Console.WriteLine(inputlength.ToString() + "->" + bufferlength.ToString());
            //Console.WriteLine(output[4].ToString() + output[5].ToString());
            return bufferlength;
        }
        Deflater d = new Deflater();
        Inflater i = new Inflater();
        //byte[] buffer = new byte[1024 * 32];

        public static ZipCompressor Instance = new ZipCompressor();
    }

    public class DeltaCompressor
    {
        public int Compress(byte[] input, int inputlength, byte[] reference, int reflength,byte[] output)
        {
            int l = Delta(input, inputlength, reference, reflength, buffer);
            d.Reset();
            d.SetInput(buffer, 0, l);
            d.Flush();
            int n = d.Deflate(output);
            return n;
        }
        public int Decompress(byte[] input, int inputlength, byte[] reference, int reflength, byte[] output)
        {
            i.Reset();
            i.SetInput(input, 0, inputlength);
            //i.Flush();
            int bufferlength = i.Inflate(buffer);
            int l = Add(reference, reflength, buffer, bufferlength, output);
            return l;
        }

        protected int Add(byte[] a, int al, byte[] b, int bl, byte[] output)
        {
            int l = Math.Min(al, bl);
            int i;
            for (i = 0; i < l; ++i)
            {
                output[i] = (byte)(a[i] + b[i]);
            }
            l = Math.Max(al, bl);
            for (; i < l; ++i)
            {
                output[i] = (byte)((i < al ? a[i] : (byte)0) + (i < bl ? b[i] : (byte)0));
            }
            return l;
        }
        protected int Delta(byte[] a, int al,byte[] b, int bl,byte[] output)
        {
            int l = Math.Min(al, bl);
            int i;
            for (i = 0; i < l; ++i)
            {
                output[i] = (byte)(a[i] - b[i]);
            }
            l = Math.Max(al, bl);
            for (; i < l; ++i)
            {
                output[i] = (byte)((i < al ? a[i] : (byte)0) - (i < bl ? b[i] : (byte)0));
            }
            return l;
        }

        Deflater d = new Deflater();
        Inflater i = new Inflater();
        byte[] buffer = new byte[1024 * 32];
    }

    public class Compressor
    {
        public static byte[] Compress(byte[] data)
        {
            return data;
            /*
			//def.WaitOne();
			d.Reset();
			d.SetInput(data);
			d.Flush();
			int n=d.Deflate(buffer);
			if(n==0)
				throw new Exception("compression failed.");

			byte[] c=new byte[n];
			for(int j=0;j<n;++j)
				c[j]=buffer[j];
			//def.ReleaseMutex();
			return c;
             * */
        }
        public static byte[] DeCompress(byte[] data)
        {
            return data;
            //inf.WaitOne();
            /*i.Reset();
            i.SetInput(data);
            //i.Flush();
            int n=i.Inflate(buffer);
            if(n==0)//||!i.IsFinished)
                throw new Exception("decompression failed.");

            byte[] c=new byte[n];
            for(int j=0;j<n;++j)
                c[j]=buffer[j];
            //inf.ReleaseMutex();
            return c;*/
        }

        static Deflater d = new Deflater();
        static Inflater i = new Inflater();
        static byte[] buffer = new byte[1024 * 1024];
        //static Mutex inf=new Mutex();
        //static Mutex def=new Mutex();
    }

    public class Console
    {
        public delegate void ConsoleCallback(string line);
        public static event ConsoleCallback ConsoleEvent;
        /*public static void Write(string text)
        {
            CurrentLine += text;
            if(Root.Instance is Server)
            {
                System.Console.Write("Server: "+text);
            }
            else
            {
                if(Root.Instance.Gui!=null)
                    Root.Instance.Gui.Console.log.Append(text);
                else
                    System.Console.Write("Client: "+text);
            }
        }*/
        public static void WriteLine(string text)
        {
            if(ConsoleEvent!=null)
                ConsoleEvent(text);
            History.Enqueue(text);
            while (History.Count > 256)
                History.Dequeue();

            if (Root.Instance == null)
            {
                System.Console.WriteLine(text);
                return;
            }

            if (Root.Instance.IsAuthoritive)
            {
                System.Console.WriteLine("Server: " + text);
            }
            else
            {
                if (Root.Instance != null && Root.Instance.Gui != null && Root.Instance.Gui.Console!=null)
                    Root.Instance.Gui.Console.log.AppendLine(text);
                //else
                System.Console.WriteLine("Client: " + text);
            }
        }

        public static Queue<string> History = new Queue<string>();
        //string CurrentLine = "";
    }

    //freespace 2 archive
    public class VpArchive : IArchive
    {
        public class Enumerator : IEnumerator
        {
            public Enumerator(VpArchive a)
            {
            }

            public object Current
            {
                get
                {
                    return null;
                }
            }

            public void Reset()
            {
            }

            public bool MoveNext()
            {
                return true;
            }

            protected VpArchive archive;
        }

        struct header
        {
            public string signature;   //"VPVP"
            public int version;         //"2"
            public int diroffset;       //from beginning of file
            public int direntries;      //number of files
        }

        struct direntry
        {
            public int offset;          //from beginning of file
            public int size;
            public string filename;   //Null-terminated string
            public int timestamp;       //The time the file was last modified in seconds since 1.1.1970 0:00
            // Same as from calling findfirst/findnext file using any C compiler.
        }

        void ReadHeader()
        {
            Header.signature = "";
            for (int i = 0; i < 4; ++i)
                Header.signature += Reader.ReadChar();
            if (Header.signature != "VPVP")
                throw new Exception("sig invalid.");
            Header.version = Reader.ReadInt32();
            Header.diroffset = Reader.ReadInt32();
            Header.direntries = Reader.ReadInt32();
        }

        void ReadEntries()
        {
            Data.Seek(Header.diroffset, SeekOrigin.Begin);
            Entries = new direntry[Header.direntries];
            for (int i = 0; i < Header.direntries; ++i)
            {
                Entries[i].offset = Reader.ReadInt32();
                Entries[i].size = Reader.ReadInt32();
                Entries[i].filename = "";
                bool stringend = false;
                for (int j = 0; j < 32; ++j)
                {
                    if (!stringend && Reader.PeekChar() != 0)
                        Entries[i].filename += Reader.ReadChar();
                    else
                    {
                        Reader.ReadChar();
                        stringend = true;
                    }
                }
                Entries[i].timestamp = Reader.ReadInt32();
            }
        }

        public void Open(Stream s)
        {
            Data = s;
            Reader = new BinaryReader(s);
            ReadHeader();
            ReadEntries();
        }

        public Stream GetFileStream(string name)
        {
            throw new Exception("The method or operation is not implemented.");
        }


        public void Dispose()
        {
        }


        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        direntry[] Entries;
        header Header;
        Stream Data;
        BinaryReader Reader;
    }

    public class ZipArchive : IArchive
    {
        public class Enumerator : IEnumerator
        {
            public Enumerator(ZipArchive a)
            {
                archive = a;
                enumerator = a.zfile.GetEnumerator();
            }

            public object Current
            {
                get
                {
                    ZipEntry e = (ZipEntry)enumerator.Current;
                    return new ArchiveEntry(e.Name, archive);
                }
            }

            public void Reset()
            {
                enumerator.Reset();
                ZipEntry e = (ZipEntry)enumerator.Current;
                while (e.IsDirectory)
                {
                    enumerator.MoveNext();
                    e = (ZipEntry)enumerator.Current;
                }
            }

            public bool MoveNext()
            {
                if (!enumerator.MoveNext())
                    return false;
                ZipEntry e = (ZipEntry)enumerator.Current;
                while (e.IsDirectory)
                {
                    if (!enumerator.MoveNext())
                        return false;
                    e = (ZipEntry)enumerator.Current;
                }
                return true;
            }

            protected ZipArchive archive;
            protected IEnumerator enumerator;
        }

        public void Open(Stream s)
        {
            zfile = new ZipFile(s);
        }

        public Stream GetFileStream(string name)
        {
            return zfile.GetInputStream(zfile.GetEntry(name));
        }

        public IEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        public void Dispose()
        {
            zfile.Close();
            zfile = null;
        }

        ZipFile zfile;
    }

    public static class Utility
    {
        public static Ray GetMouseVector()
        {
            IRenderer r = Root.Instance.UserInterface.Renderer;
            r.SetCamera(Root.Instance.Scene.camera);
            //float[] modelview=new float[16];
            //float[] projection=new float[16];

            float x = Root.Instance.UserInterface.Mouse.GetPosition(0);
            float y = r.Size.Y - Root.Instance.UserInterface.Mouse.GetPosition(1);

            x *= (float)r.RenderSize.X / (float)r.WindowSize.X;
            y *= (float)r.RenderSize.Y / (float)r.WindowSize.Y;

            Vector3 v1 = new Vector3(r.UnProject(new float[] { x, y, 1 }, null, null, null));
            Vector3 v2 = new Vector3(r.UnProject(new float[] { x, y, 100000 }, null, null, null));

           
            return new Ray(v1, v2);
        }
    }
    public class FileSystem : FileSystemNode
    {
        public FileSystem(string home)
        {
            ListDirectory(dir = new DirectoryInfo(home), this);
            ListDirectory(new DirectoryInfo("."), this);
            //Add("Virtual",new FileSystemNode());
            UpdateFileIds();
            //SaveFileIds();
        }

        private void SaveFileIds()
        {
            Stream s = this.CreateFile("files.id").getStream();
            StreamWriter sw = new StreamWriter(s);
            foreach (KeyValuePair<int, string> kv in FileIds)
            {
                sw.WriteLine(kv.Key + ": " + kv.Value);
            }
            sw.Close();
            s.Close();

        }

        public void ListDirectory(DirectoryInfo dir, FileSystemNode n)
        {
            DirectoryInfo[] dir2 = dir.GetDirectories();
            for (int i = 0; i < dir2.Length; ++i)
            {
                string name = dir2[i].Name;
                if (!n.ContainsKey(name))
                {
                    FileSystemNode n2 = new FileSystemNode();
                    n2.dir = dir2[i];
                    n.Add(name, n2);
                }
                ListDirectory(dir2[i], ((FileSystemNode)n[name]));
            }

            FileInfo[] files = dir.GetFiles();
            for (int i = 0; i < files.Length; ++i)
            {
                string name = files[i].Name;
                if (!n.ContainsKey(name))
                {
                    FileSystemNode f = new FileSystemNode(files[i]);
                    n.Add(name, f);
                }
            }
        }

        public FileSystemNode CreatePath(FileSystemNode root, string path)
        {
            if (root == null)
                root = this;

            path = path.Trim(new char[] { ' ', '/', '\\' });
            if (path.IndexOf("//") >= 0 || path.IndexOf("\\\\") >= 0)
                throw new Exception("invalid url: " + path);

            string[] a = path.Split(new char[] { '/', '\\' });
            for (int i = 0; i < a.Length; ++i)
            {
                string s = a[i];
                if (root.ContainsKey(s))
                {
                    root = (FileSystemNode)root[s];
                }
                else
                {
                    root = root.Create(s);
                }
            }
            return root;
        }


        public FileSystemNode Get(FileSystemNode root, string path)
        {
            return Get(root, path, true);
        }
        public FileSystemNode Get(FileSystemNode root, string path, bool nocase)
        {
            if (root == null)
                root = this;

            path = path.Trim(new char[] { ' ', '/', '\\' });
            if (path.IndexOf("//") >= 0 || path.IndexOf("\\\\") >= 0)
                throw new Exception("invalid url: " + path);

            string[] a = path.Split(new char[] { '/', '\\' });
            for (int i = 0; i < a.Length; ++i)
            {
                string s = a[i];
                if (root.ContainsKey(s))
                {
                    root = (FileSystemNode)root[s];
                }
                else if (s == ".")
                {
                    //nothing
                }
                else if (nocase)
                {
                    //System.Console.WriteLine("nocase: " + s);
                    bool found = false;
                    foreach (DictionaryEntry de in root)
                    {
                        string key = (string)de.Key;
                        if (key.ToLower() == s.ToLower())
                        {
                            root = (FileSystemNode)de.Value;
                            found = true;
                            break;
                        }
                    }
                    if(!found)
                        throw new Exception("url not found(nocase): " + path);
                }
                else
                {
                    throw new Exception("url not found: " + path);
                }
            }
            return root;
        }

        private void EnumFileIds(Dictionary<int, string> list, FileSystemNode n, ref int id)
        {
            list.Add(id++, n.GetFullPath());
            foreach (DictionaryEntry de in n)
            {
                EnumFileIds(list, (FileSystemNode)de.Value, ref id);
            }
        }

        Dictionary<int, string> FileIds = new Dictionary<int, string>();

        public void UpdateFileIds()
        {
            int id = 1;

            EnumFileIds(FileIds, this, ref id);
        }

        public FileSystemNode Get(string path)
        {
            return Get(null, path);
        }

        public void Add(FileSystemNode root, string path,FileSystemNode n)
        {
            if (root == null)
                root = this;

            path = path.Trim(new char[] { ' ', '/', '\\' });
            if (path.IndexOf("//") >= 0 || path.IndexOf("\\") >= 0)
                throw new Exception("invalid url.");

            string[] a = path.Split(new char[] { '/', '\\' });
            for (int i = 0; i < a.Length; ++i)
            {
                string s = a[i];
                if (root.ContainsKey(s))
                {
                    if (i == a.Length - 1)
                        throw new Exception("node already exists.");
                }
                else
                {
                    if (i == a.Length - 1)
                        root.Add(s, n);
                    else
                        root.Add(s, new FileSystemNode());
                }
                root = (FileSystemNode)root[s];
            }
            if (root != n)
                throw new Exception("error in FileSystem.Add.");
        }

        public void Mount(string archive, string target)
        {
            Mount(Get(null, archive), Get(null, target));
        }

        public void Mount(FileSystemNode archive, FileSystemNode target)
        {
            IArchive a = Root.Instance.ResourceManager.LoadArchive(archive);
            foreach (ArchiveEntry e in a)
            {
                Add(target, e.name, new ArchiveFileNode(e));
            }
            //target.mounted=a;
        }
    }

    public class Global
    {
        public static void ConsoleWriteLine(string line)
        {
            //Root.getInstance().console.WriteLine(line);
            Console.WriteLine(line);
        }
    }

    public interface IConsole : IDisposable
    {
        void WriteLine(string text);
        void Write(string text);
        void SetInputHandler(ConsoleInputDelegate f);
    }

    public delegate void ConsoleInputDelegate(string input);

    /*	public class TextConsole : IConsole
        {
            public TextConsole()
            {
                //consolethread=new Thread(new ThreadStart(ThreadStart));
                //consolethread.Start();
            }

            protected void ThreadStart()
            {
                while(!stop)
                {
                    //Console.Write("cheetah["+((FileSystemNode)Root.getInstance().current).GetFullPath()+"]>");
                    Console.Write("cheetah>");
                    string line=Console.ReadLine();
                    if(input!=null)
                        input(line);
                }
            }

            public void Dispose()
            {
                stop=true;
                //consolethread.Abort();
                //consolethread.Join();
                //consolethread=null;
            }

            public void WriteLine(string text)
            {
                Console.WriteLine("");
                Console.WriteLine(text);
            }

            public void Write(string text)
            {
                Console.Write(text);
            }

            public void SetInputHandler(ConsoleInputDelegate f)
            {
                input=f;
            }

            ConsoleInputDelegate input;
            Thread consolethread;
            bool stop;
        }

        public class Script
        {
            public Script()
            {
                Lua=new Lua();
            }

            Lua Lua;
        }
    */
    public class DemoPlayer : ITickable
    {
        struct Frame
        {
            public float Time;
            public byte[] Data;
        }

        public DemoPlayer(Stream s)
        {
            DemoFile = s;
            DemoReader = new BinaryReader(DemoFile);

            ReadHeader();
            ReadAllPackets();
            ReadNextPacket();

            StartTime = Frames[0].Time;
            Time = StartTime;
        }

        public DemoPlayer(FileSystemNode n)
            :this(n.getStream())
        {
        }

        public DemoPlayer(string filename)
            :this(Root.Instance.FileSystem.Get(filename).getStream())
        {
            //DemoFile = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
            //DemoFile = ;

        }

        protected void ReadHeader()
        {
            TickRate = DemoReader.ReadInt32();
            TimeBased = DemoReader.ReadBoolean();
        }

        public void Tick(float dtime)
        {
            while (NextTime <= Time)
            {
                ReadNextPacket();
            }

            Time += dtime;
        }

        public byte[] ReadPacket()
        {
            if (NextPacketRead)
                return null;
            else
            {
                NextPacketRead = true;
                return NextPacket;
            }
        }

        protected void ReadAllPackets()
        {
            //int c = 0;
            float t;
            int size;
            byte[] p;

            try
            {
                while (true)
                {
                    t = (float)DemoReader.ReadInt32() / 1000;
                    size = DemoReader.ReadInt32();
                    p = new byte[size];
                    if (DemoFile.Read(p, 0, size) != size)
                        break;

                    Frame f;
                    f.Time = t;
                    f.Data = p;
                    Frames.Add(f);
                }
            }
            catch (IOException)
            {
            }
            Console.WriteLine("demo has " + Frames.Count + " frames.");
        }

        protected void ReadNextPacket()
        {
            try
            {
                NextTime = Frames[CurrentFrame].Time;
                //int size = DemoReader.ReadInt32();
                NextPacket = Frames[CurrentFrame].Data;
                CurrentFrame++;
                NextPacketRead = false;
            }
            catch (Exception)
            {
                //Console.WriteLine("demo ended, looping...");
                Root.Instance.Scene.Clear();

                CurrentFrame = 0;

                //ReadHeader();
                ReadNextPacket();

                //StartTime = NextTime;
                Time = StartTime;

            }
        }

        public void GoTo(float time)
        {
            time = Math.Min(time, Length);
            for (int i = 0; i < Frames.Count; ++i)
            {
                if (Frames[i].Time >= time + StartTime)
                {
                    NextPacketRead = false;
                    CurrentFrame = i;
                    Time = NextTime = Frames[i].Time;

                    Console.WriteLine("goto frame " + CurrentFrame + "/" + Frames.Count + ", time: " + (Frames[i].Time - StartTime) + "/" + Length);
                    return;
                }
            }
            throw new Exception();
        }

        public float CurrentTime
        {
            get
            {
                return Time - StartTime;
            }
        }

        public float Length
        {
            get
            {
                return Frames[Frames.Count - 1].Time;
            }
        }

        public int FrameCount
        {
            get
            {
                return Frames.Count;
            }
        }
        List<Frame> Frames = new List<Frame>();
        int CurrentFrame = 0;

        Stream DemoFile;
        BinaryReader DemoReader;
        int TickRate;
        bool TimeBased;
        float Time = 0;

        byte[] NextPacket;
        bool NextPacketRead = false;
        float NextTime;
        float StartTime;
    }

    public class DemoRecorder
    {
        public DemoRecorder(Stream s, bool timebased, int tickrate)
        {
            DemoFile = s;
            DemoWriter = new BinaryWriter(DemoFile);
            TimeBased = timebased;
            TickRate = tickrate;

            WriteHeader();
        }

        public DemoRecorder(string filename, bool timebased,int tickrate)
            :this(new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.Read),timebased,tickrate)
        {
        }

        protected void WriteHeader()
        {
            DemoWriter.Write(TickRate);
            DemoWriter.Write(TimeBased);
        }

        public void WritePacket(int time, byte[] data, int size)
        {
            DemoWriter.Write(time);
            DemoWriter.Write(size);
            DemoFile.Write(data, 0, size);
        }

        Stream DemoFile;
        BinaryWriter DemoWriter;
        int TickRate;
        bool TimeBased;
    }

    public class InGameConsole : Window
    {
        public InGameConsole(Gui gui)
        {
            System.Drawing.Point s;
            if (Root.Instance.UserInterface != null)
                s = Root.Instance.UserInterface.Renderer.Size;
            else
                s = new System.Drawing.Point(1000, 1000);

            Size = new Vector2(s.X, s.Y / 2);
            float f = s.Y / 2 - gui.DefaultFont.size;
            log = new TextBox(0, 0, s.X, f);
            cmdline = new TextBox(0, f, s.X, s.Y / 2 - f);

            Transparent = true;
            log.Color = new Color4f(0.5f, 0.5f, 0, 0.5f);
            cmdline.Color = new Color4f(0.0f, 0.5f, 0, 0.5f) ;

            log.ReadOnly = true;
            cmdline.MultiLine = false;
            log.MultiLine = true;

            Add(log);
            Add(cmdline);

            Fade = -1;
        }

        public InGameConsole(float x, float y,float w,float h)
            : base(x, y,w,h)
        {
            Fade = -1;

            System.Drawing.Point s = Root.Instance.UserInterface.Renderer.Size;
            //float f = s.Y / 2 - gui.DefaultFont.size;
            log = new TextBox(0, 0, w, h - Root.Instance.Gui.DefaultFont.size);
            cmdline = new TextBox(0, h - Root.Instance.Gui.DefaultFont.size, w, Root.Instance.Gui.DefaultFont.size);
            Transparent = true;

            log.ReadOnly = true;
            cmdline.MultiLine = false;
            log.MultiLine = true;

            log.FocusColor = log.NormalColor = log.Color = new Color4f(0.5f, 0.5f, 0, 0.0f);

            //cmdline.Color = new Color4f(0.0f, 0.5f, 0, 0.5f);
            color.a = 0.0f;

            Add(log);
            Add(cmdline);
        }

        public override void OnResize()
        {
            base.OnResize();

            if (log != null)
            {
                log.Size = new Vector2(size.x, size.y - Root.Instance.Gui.DefaultFont.size);
                cmdline.Size = new Vector2(size.x, Root.Instance.Gui.DefaultFont.size);
                cmdline.Position = new Vector2(0, size.y - Root.Instance.Gui.DefaultFont.size);
            }
        }
        public override void OnChildKeyDown(Window w, global::OpenTK.Input.Key key)
        {
            base.OnChildKeyDown(w, key);
            if (key == global::OpenTK.Input.Key.Enter)
            {
                if (cmdline.GetLine(0).Length > 0)
                {
                    log.AppendLine(cmdline.GetLine(0));
                    Execute((string)cmdline.GetLine(0));
                }
                cmdline.Clear();
            }
        }

        public override void OnKeyPress(char c)
        {
            base.OnKeyPress(c);

            cmdline.Append(c.ToString());
        }

        public override void OnKeyDown(global::OpenTK.Input.Key key)
        {
            base.OnKeyDown(key);

            //throw new Exception(key.Code.ToString());
            //int keycode=(int)key;
            /*if (IsPrintable(key))
            {
                cmdline.Append(key.GetString());
            }
            else */if (key == global::OpenTK.Input.Key.Enter)
            {
                if (cmdline.GetLine(0).Length > 0)
                {
                    log.AppendLine(cmdline.GetLine(0));
                    Execute((string)cmdline.GetLine(0));
                }
                cmdline.Clear();
            }
            else if (key == global::OpenTK.Input.Key.BackSpace)
            {
                cmdline.OnKeyDown(key);
            }
            else if (key == global::OpenTK.Input.Key.PageUp)
            {
                log.Scroll(-1);
            }
            else if (key == global::OpenTK.Input.Key.PageDown)
            {
                log.Scroll(1);
            }
            //	log.OnKeyDown(key);
        }

        public void WriteLine(string text)
        {
            log.AppendLine(text);
        }

        public virtual void Execute(string cmd)
        {
            try
            {
                Root.Instance.Script.Execute(cmd);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType().ToString() + " at " + e.Source + ":");
                Console.WriteLine(e.Message);
            }
        }
        public bool enabled = false;
        public TextBox log;
        TextBox cmdline;
    }

    public class RemoteConsole
    {
    }

    public class TimedEvent : ITickable
    {
        public TimedEvent(float rate)
        {
            Rate = rate;
        }
        public TimedEvent(float rate, TimerDelegate f)
        {
            Rate = rate;
            Function = f;
        }

        public float Interval;
        public float LastExecution;
        public float Rate
        {
            get { return 1 / Interval; }
            set { Interval = 1 / value; }
        }
        public virtual void Raise()
        {
            if (Function != null)
                Function();
        }
        public delegate void TimerDelegate();
        public event TimerDelegate Function;

        #region ITickable Members

        public void Tick(float dtime)
        {
            if (Root.Instance.Time - LastExecution >= Interval)
            {
                LastExecution = Root.Instance.Time;
                Raise();
            }
        }

        #endregion
    }

    public interface IInterpreter
    {
        void Execute(string code);
        void Execute(Stream code);
        void SetValue(string name, object v);
        void Reference(Assembly a);
    }

    /*public abstract class Interpreter : IInterpreter
    {
        public void Execute(string code)
        {
        }
    }*/

    public class ConfigLoader : IResourceLoader
    {

        public IResource Load(FileSystemNode n)
        {
            StreamReader r = new StreamReader(n.getStream());
            string line;

            Config c = new Config(n);

            string header = r.ReadLine().Trim();
            if (header != "CONFIGTEXT")
                return null;

            while ((line = r.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#"))
                    continue;

                string[] split = line.Split(new char[] { ':' });
                string key = split[0].Trim();
                string val = split[1].Trim();

                c.Set(key, val);
            }

            c.Changed = false;
            return c;
        }

        public Type LoadType
        {
            get { return typeof(Config); }
        }


        public bool CanLoad(FileSystemNode n)
        {
            return n.info != null && n.info.Extension.ToLower() == ".config";
        }

    }

    public class Config : IResource
    {
        public Config(FileSystemNode n)
        {
            File = n;
        }

        public void Dispose()
        {
            if (Changed)
                Save();
        }

        public bool Exists(string key)
        {
            return Table.ContainsKey(key);
        }

        public bool GetBool(string key)
        {
            return bool.Parse((string)Lookup(key));
        }

        public string GetString(string key)
        {
            return (string)Lookup(key);
        }

        public int GetInteger(string key)
        {
            return int.Parse((string)Lookup(key));
        }

        public float GetFloat(string key)
        {
            return float.Parse((string)Lookup(key));
        }

        public void Set(string key, string value)
        {
            Changed = true;
            Table[key] = value;
        }
        public void Set(string key, bool value)
        {
            Set(key, value ? "true" : "false");
        }
        public void Save()
        {
            List<string> lines = new List<string>();
            foreach (DictionaryEntry de in Table)
            {
                lines.Add((string)de.Key + ": " + de.Value.ToString());
            }

            string[] array = lines.ToArray();
            Array.Sort(array);

            File.Truncate();
            Stream s = File.getStream();
            StreamWriter w = new StreamWriter(s);

            w.WriteLine("CONFIGTEXT");

            for (int i = 0; i < array.Length; ++i)
            {
                w.WriteLine(array[i]);
            }

            w.Close();
            s.Close();
            Changed = false;
        }

        protected object Lookup(string key)
        {
            if (Table.ContainsKey(key))
                return Table[key];
            else
                throw new Exception("config: cant find " + key);
        }

        public Hashtable Table = new Hashtable();
        protected FileSystemNode File;
        public bool Changed = false;
    }

    /*   public class Configurator
       {
           public void Register(object o)
           {
               Objects.Add(o);
           }

           public string[] GetConfigs()
           {
               ArrayList al = new ArrayList();
               foreach (object o in Objects)
               {
                   Type t = o.GetType();
                   foreach (PropertyInfo pi in t.GetProperties())
                   {
                       foreach(Attribute a in pi.GetCustomAttributes(true))
                       {
                           if (a is ConfigurableAttribute)
                           {
                               ConfigurableAttribute ca = (ConfigurableAttribute)a;
                               al.Add(t.FullName+":"+pi.Name);
                           }
                       }
                   }
               }
               string[] s = new string[al.Count];
               al.CopyTo(s);
               return s;
           }


           ArrayList Objects = new ArrayList();
       }

       public class ConfigurableAttribute : Attribute
       {
           public ConfigurableAttribute(string[] path)
           {
               Path = path;
           }

           public string[] Path;
       }
   */
    public class BooInterpreter : IInterpreter
    {
        public void SetValue(string name, object v)
        {
            boo.SetValue(name, v);
        }

        public BooInterpreter()
        {
            boo = new Boo.Lang.Interpreter.InteractiveInterpreter();

            boo.load(Root.Instance.GetAssemblyPath("Cheetah.dll"));
            boo.load(Root.Instance.GetAssemblyPath("Spacewar2006.exe"));


            Execute(new FileStream("scripts" + Path.DirectorySeparatorChar + "init.boo", FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        }
        public void Reference(Assembly a)
        {
            boo.References.Add(a);
        }

        #region IInterpreter Members

        public void Execute(string code)
        {
            Boo.Lang.Compiler.CompilerContext c = boo.Eval(code);
            foreach (Boo.Lang.Compiler.CompilerError e in c.Errors)
            {
                //e.
                Console.WriteLine(e.ToString());
            }
        }

        public void Execute(Stream code)
        {
            byte[] buffer = new byte[code.Length];
            code.Read(buffer, 0, (int)code.Length);
            string s = System.Text.Encoding.ASCII.GetString(buffer, 0, (int)code.Length);
            Execute(s);
        }

        #endregion

        public Boo.Lang.Interpreter.InteractiveInterpreter boo;
    }

    /*    public class LuaInterpreter : IInterpreter
        {
            public LuaInterpreter()
            {
                lua=new Lua();
                lua.OpenBaseLib();
                lua.OpenMathLib();
                lua.OpenStringLib();
                lua.OpenTableLib();
                lua.OpenIOLib();

                lua.DoFile("scripts/init.lua");
            }

            public void Execute(string code)
            {
                //try
                //{
                    lua.DoString(code);

            }
            public void Execute(Stream code)
            {
                byte[] buffer=new byte[code.Length];
                code.Read(buffer,0,(int)code.Length);
                string s=System.Text.Encoding.ASCII.GetString(buffer,0,(int)code.Length);
                Execute(s);
            }

            public Lua lua;
        }
    */


    public class IrcBot : ITickable, IDisposable
    {
        public IrcBot(string host, int port,string nick,string realname,string[] channels)
        {
            irc = new IrcClient();
            irc.OnChannelMessage += new IrcEventHandler(irc_OnChannelMessage);
            irc.Connect(host, port);
            irc.Login(nick, realname);
            irc.OnRawMessage += new IrcEventHandler(irc_OnRawMessage);
            foreach (string c in channels)
                irc.RfcJoin(c);
            irc.OnQuit += new QuitEventHandler(irc_OnQuit);
        }

        void irc_OnQuit(object sender, QuitEventArgs e)
        {
            irc.Disconnect();
            irc = null;
        }
        void irc_OnChannelMessage(object sender, IrcEventArgs e)
        {
            OnChannelMessage(e.Data.Channel, e.Data.Nick, e.Data.Message);
        }

        protected virtual void OnChannelMessage(string channel, string nick,string message)
        {
        }
        void irc_OnRawMessage(object sender, IrcEventArgs e)
        {
            Cheetah.Console.WriteLine(e.Data.RawMessage);
        }
        public void Say(string channel, string message)
        {
            irc.RfcPrivmsg(channel, message);
        }

        public virtual void Tick(float dtime)
        {
            irc.Listen(false);
        }


        IrcClient irc;

        #region IDisposable Members

        public void Dispose()
        {
            if (irc != null)
            {
                if (irc.IsConnected)
                {
                    irc.RfcQuit();
                }
                else
                {
                    irc = null;
                }
                //HACK
                //Thread.Sleep(1000);
                //irc.Disconnect();
                //irc = null;
            }
        }

        #endregion
    }

    public class IrcWindow : Window
    {
        public IrcWindow()
            : base(new Layout(2, 2))
        {
            cmd = new TextBox(false);
            Add(cmd, 0, 1);
            log = new TextBox(true);
            Add(log, 0, 0);
            send = new Button(OnSendButtonPressed, "Send");
            Add(send, 1, 1);

            Layout.Heights[1] = 24;
            Layout.Heights[0] = 500 - 24;
            Layout.Widths[0] = 700;
            Layout.Widths[1] = 100;

            irc = new IrcClient();
            irc.OnChannelMessage += new IrcEventHandler(irc_OnChannelMessage);
            irc.OnRawMessage += new IrcEventHandler(irc_OnRawMessage);
            irc.OnConnecting += new EventHandler(irc_OnConnecting);
            irc.Connect("irc.quakenet.org", 6667);
            irc.Login("cody__", "cody");
            irc.RfcJoin("#meinungsverstaerker");
        }

        public void Send(string text)
        {
            irc.RfcPrivmsg("#meinungsverstaerker", text);
            log.AppendLine("<" + irc.Nickname + "> " + text);
            cmd.Clear();
        }

        public void OnSendButtonPressed(Button source, int button, float x, float y)
        {
            Send(cmd.GetLine(0));
        }
        void irc_OnConnecting(object sender, EventArgs e)
        {
            //log.AppendLine("connecting.");
        }

        void irc_OnRawMessage(object sender, IrcEventArgs e)
        {
            Cheetah.Console.WriteLine(e.Data.RawMessage);
        }

        void irc_OnChannelMessage(object sender, IrcEventArgs e)
        {
            log.AppendLine("<" + e.Data.Nick + "> " + e.Data.Message);
        }

        public override void OnKeyDown(global::OpenTK.Input.Key key)
        {
            if (key == global::OpenTK.Input.Key.Enter)
            {
                Send(cmd.GetLine(0));
            }
            else
                base.OnKeyDown(key);
        }
        public override void Tick(float dtime)
        {
            base.Tick(dtime);
            irc.Listen(false);
        }

        IrcClient irc;
        TextBox log;
        TextBox cmd;
        Button send;
    }

    public sealed class Root : IDisposable
    {
        public List<Assembly> Assemblies = new List<Assembly>();

        [DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWow64Process(
            [In] IntPtr hProcess,
            [Out] out bool wow64Process
        );

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool InternalCheckIsWow64()
        {
            if ((Environment.OSVersion.Version.Major == 5 && Environment.OSVersion.Version.Minor >= 1) ||
                Environment.OSVersion.Version.Major >= 6)
            {
                using (Process p = Process.GetCurrentProcess())
                {
                    bool retVal;
                    if (!IsWow64Process(p.Handle, out retVal))
                    {
                        return false;
                    }
                    return retVal;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool Is64BitProcess
        {
            get
            {
                return IntPtr.Size == 8;
            }
        }

        public static bool Is64BitOS
        {
            get
            {
                try
                {
                    return Is64BitProcess || InternalCheckIsWow64();
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public void ClientClient(string[] args, IUserInterface userinterface)
        {
            Authoritive = false;

            Config c = (Config)ResourceManager.Load("config/global.config", typeof(Config));

            if (userinterface != null)
            {
                int width = c.GetInteger("video.width");
                int height = c.GetInteger("video.height");
                bool fullscreen = c.GetBool("video.fullscreen");
                bool audio = c.GetBool("audio.enable");


                UserInterface = userinterface;
                UserInterface.Create(fullscreen, width, height, audio);
                ShaderManager = new ShaderManager(UserInterface.Renderer);
                ClientPostProcessor = new PostProcess(width, height, UserInterface.Renderer);
            }

            Gui = new Gui();

            float updaterate = c.GetFloat("client.updaterate");
            ClientSendEvent = new TimedEvent(updaterate);
        }
        
        public void ClientClient(string[] args)
		{
            int i = Array.IndexOf<string>(args, "--ui");
            if (i != -1)
            {
                string ui = args[i + 1];
                switch (ui)
                {
                    case "none":
                        ClientClient(args, null);
                        break;
                    default:
                        throw new Exception("unknown UI: " + ui);
                }
            }
            else
            {
                ClientClient(args, new Cheetah.OpenTK.UserInterface());
            }
		}
        public void ClientConnect(string host)
		{
            string playername=ResourceManager.LoadConfig("config/global.config").GetString("player.name");
            int defaultport = ResourceManager.LoadConfig("config/global.config").GetInteger("client.defaultport");

            Console.WriteLine("connecting to " + host);
            int port = defaultport;
            string protocol="spacewar://";
            if (host.StartsWith(protocol,true,null))
            {
                host=host.Substring(protocol.Length);
            }
            if (host.Contains("/"))
            {
                host = host.Substring(0, host.IndexOf('/'));
            }

            if (host.Contains(":"))
            {
                string[] split = host.Split(':');
                host = split[0];
                port = int.Parse(split[1]);
            }

            Connection = new UdpClient(host, port, playername,ClientPassword);
        }

		public void ClientDisconnect()
		{
            if (Connection != null && Connection is UdpClient)
			{
				((UdpClient)Connection).Disconnect();
				Connection=null;
			}
		}

		public void ClientRun(Flow f)
		{
			CurrentFlow=f;
			f.Start();
			ClientLoop();
			f.Stop();
			CurrentFlow=null;
		}

		public void ClientLoop()
		{
			while((UserInterface==null || !UserInterface.wantsQuit()) && !Quit&&(CurrentFlow==null||!CurrentFlow.Finished))
			{
				Update(false);
			}
		}
		
		public void ClientSendUpdates()
		{
            Events e = GenerateEventPacket();
            if (e != null)
            {
                ((UdpClient)Connection).Send(e);
            }
            if (Time - ClientSendEvent.LastExecution >= ClientSendEvent.Interval)
			{
                while (Time - ClientSendEvent.LastExecution > ClientSendEvent.Interval)
                    ClientSendEvent.LastExecution += ClientSendEvent.Interval;
				State s=GenerateState(0);
				((UdpClient)Connection).Send(s);
			}
        }
		
		public bool ClientCategorizeReceivedEntity(int serverindex,int clientindex,short ownernumber,short sendernumber)
		{
			return true;
		}
		
		public bool ClientCategorizeSendEntity(int serverindex,int clientindex,short ownernumber,short sendernumber)
		{
			return ownernumber==sendernumber;
		}

		public void ClientOnMouseDown(int button,int x,int y)
		{
			Gui.OnMouseDown(button,x,y);
		}

		public void ClientOnMouseMove(int x,int y)
		{
			Gui.OnMouseMove(x,y);
		}

        public void ClientOnKeyPress(char key)
        {
            Gui.OnKeyPress(key);
        }

        public void ClientOnKeyDown(global::OpenTK.Input.Key key)
		{
            if (key == global::OpenTK.Input.Key.F1)
			{
                string name = string.Format("screen-{0}{1,2}{2,2}{3,2}{4,2}{5,2}.tga",
                    DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                name = name.Replace(' ', '0');

                Screenshot(name);
				/*Image img=Root.Instance.UserInterface.Renderer.Screenshot();
				int id;
				File.Delete(filename);
				Tao.DevIl.Il.ilGenImages(1,out id);
				Tao.DevIl.Il.ilBindImage(id);
				Tao.DevIl.Il.ilTexImage(img.Width,img.Height,1,3,Tao.DevIl.Il.IL_RGB,Tao.DevIl.Il.IL_UNSIGNED_BYTE,img.Data);
				Tao.DevIl.Il.ilSave(Tao.DevIl.Il.IL_PNG, filename);
				Tao.DevIl.Il.ilDeleteImages(1,ref id);*/
			}
			else
			{
				if(Root.Instance.CurrentFlow!=null)
					Root.Instance.CurrentFlow.OnKeyPress(key);
				Gui.OnKeyDown(key);
			}
		}

        public void ClientOnStateReceive(Lidgren.Network.NetIncomingMessage msg)
		{
            DeSerializationContext context = new DeSerializationContext(msg);

            if(Connection!=null)
                Scene.ClientStateReceive(context,((UdpClient)Connection).ClientNumber);
            else
                Scene.ClientStateReceive(context, 0);
        }

        public bool Authoritive=false;
        public bool IsAuthoritive
        {
            get
            {
                return Authoritive;
            }
        }

        /*public Lidgren.Library.Network.NetStatistics Delta(Lidgren.Library.Network.NetStatistics before, Lidgren.Library.Network.NetStatistics after)
        {
            if (before == null)
                return after;

            Lidgren.Library.Network.NetStatistics delta = new Lidgren.Library.Network.NetStatistics(null);
        }*/

        public Version AssemblyVersion
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        public int Version
        {
            get
            {
                return AssemblyVersion.Minor;
            }
        }

		public void UpdateInfo(float dtime,int cycles)
		{
			ClientFps=(int)((float)cycles/dtime);
			ClientMem=(int)GC.GetTotalMemory(false);
            if (Connection != null)
            {
                ConnectionStatistics stat = Connection.Statistics;
                DeltaConnStats = stat - LastConnStats;
                LastConnStats = stat;
            }
        }

        public PostProcess ClientPostProcessor;

		public void ClientTick(float dtime)
		{
            if(UserInterface!=null)
			    UserInterface.ProcessEvents();

            if(ClientPostProcessor!=null)
                ClientPostProcessor.Enable(UserInterface.Renderer);

            if (UserInterface != null)
            {
                UserInterface.Renderer.SetMode(RenderMode.Draw3D);
                Scene.Draw(UserInterface.Renderer);

                UserInterface.Tick(dtime);
            }

            //for (int i = 0; i < TickMultiplier; ++i)
            {
                Tick(dtime,false);///(float)TickMultiplier);
            }

            if (EventSendQueue != null && EventSendQueue.Count > 0 && IsAuthoritive)
            {
                EventSendQueue.Clear();
            }

            if (ClientPostProcessor != null)
                ClientPostProcessor.Render();

            if (UserInterface != null)
            {
                UserInterface.Renderer.SetMode(RenderMode.Draw2D);
                Gui.Draw(UserInterface.Renderer);
            }

            if (CurrentFlow != null)
                CurrentFlow.OnDraw();


            if (UserInterface != null)
			{
                string gc="";

                string sound = " sounds: " + Scene.NumSounds;

                if (IsUnix)
                {
                    gc = "NYI";
                }
                else
                {
                    for (int i = 0; i <= GC.MaxGeneration; ++i)
                    {
                        gc += GC.CollectionCount(i) + ((i == GC.MaxGeneration) ? "" : "/");
                    }
                }

                Gui.DefaultFont.Draw(UserInterface.Renderer, "fps: " + ClientFps.ToString() + " mem: " + (ClientMem / 1024).ToString() + "k" +
                    " gc: "+gc+
					" video: "+(((OpenGL)UserInterface.Renderer).BufferMemory/1024).ToString()+"k vis: "+Scene.VisibleNodes.ToString()+
                    sound +
                    " lights: "+Scene.lightcount+
                    ((((int)(Time*2))%2==0 && Player!=null)?" [playing demo]":""),0,0);
			
			
				if(Connection!=null)
				{
                    Gui.DefaultFont.Draw(UserInterface.Renderer, DeltaConnStats.ToString(), 0, 20);
                }
            }


            if (UserInterface != null)
                UserInterface.Flip();

            if (ClientRecordVideo > 0)
            {
                //Image img=UserInterface.Renderer.Screenshot2();

                //int id;
                //Directory.CreateDirectory("video");
                string filename = string.Format("video{0,6}.tga", ClientRecordVideo);
                filename=filename.Replace(' ', '0');

                
                /*Tao.DevIl.Il.ilGenImages(1, out id);
                Tao.DevIl.Il.ilBindImage(id);
                Tao.DevIl.Il.ilTexImage(img.Width, img.Height, 1, 3, Tao.DevIl.Il.IL_RGB, Tao.DevIl.Il.IL_UNSIGNED_BYTE, img.Data);
                Tao.DevIl.Il.ilSave(Tao.DevIl.Il.IL_BMP, filename);
                Tao.DevIl.Il.ilDeleteImages(1, ref id);
                */
                /*System.Drawing.Bitmap b = new System.Drawing.Bitmap(img.Width, img.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                for(int y=0;y<img.Height;++y)
                    for (int x = 0; x < img.Width; ++x)
                    {
                        //System.Drawing.Color c= new System.Drawing.Color();
                        b.SetPixel(x, y, img.GetPixel(x,y));
                    }*/
                //img.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
                
                //TgaImageSaver tga=new TgaImageSaver();
                //tga.Save(img,new FileStream(filename,FileMode.Create,FileAccess.Write));
                Screenshot(filename);
                ClientRecordVideo++;
            }


			frame++;
		}

        public void Screenshot(string filename)
        {
            Image img = UserInterface.Renderer.Screenshot2();

            FileSystemNode n = FileSystem.Get("screens");
            TgaImageSaver tga = new TgaImageSaver();
            tga.Save(img, new FileStream(n.dir.FullName+Path.DirectorySeparatorChar+filename, FileMode.Create, FileAccess.Write));
        }

		public TimedEvent ClientSendEvent;
		public int ClientFps;
		public int ClientMem;
        public int ClientRecordVideo = -1;
        public int ClientTickMultiplier = 10;
        public ConnectionStatistics LastConnStats;
        public ConnectionStatistics DeltaConnStats;
        public string ClientPassword;




        /// <summary>
        /// Server
        /// 
        /// </summary>

        
		public void ServerServer(string[] args)
		{
            Authoritive = true;
            //InfoInterval = 5;
            ServerLoadConfig();
            Config c=ResourceManager.LoadConfig("config/global.config");
            bool webserver = c.GetBool("web.enable");
            if (webserver)
            {
                int port = c.GetInteger("web.port");
                ServerWeb = new WebServer(port,c.GetString("web.name"),c.GetString("web.password"));
            }
            
		}

        void ServerLoadConfig()
        {
            Config c=ResourceManager.LoadConfig("config/global.config");
            ServerMaxPlayers = c.GetInteger("server.maxplayers");
            ServerName = c.GetString("server.name");
            ServerTickrate = c.GetFloat("server.tickrate");
            ServerUpdateDivisor = c.GetInteger("server.updatedivisor");
            ServerAdmin = c.GetString("server.admin");
            ServerAdminMail = c.GetString("server.adminmail");
        }

		public void ServerSendUpdates()
		{
            Events e = GenerateEventPacket();
            if (e != null)
            {
                //Console.WriteLine(e.ToString());
                ((UdpServer)Connection).Send(e);
            }

            if (ClientSendEvent == null)
            {
                if (Cycle % ServerUpdateDivisor == 0)
                {
                    State s = GenerateState(0);
                    ((UdpServer)Connection).Send(s);
                }
            }
            else
            {
                //non-dedicated server
                if (Time - ClientSendEvent.LastExecution >= ClientSendEvent.Interval)
                {
                    while (Time - ClientSendEvent.LastExecution > ClientSendEvent.Interval)
                        ClientSendEvent.LastExecution += ClientSendEvent.Interval;
                    State s = GenerateState(0);
                    ((UdpServer)Connection).Send(s);
                }
            }
		}

		public void ServerLoop()
		{
            sleeptime = 1000.0f / ServerTickrate;
            int last = TickCount();
            while (!Quit && (CurrentFlow == null || !CurrentFlow.Finished))
			{
                ServerConsoleMutex.WaitOne();
                string line;
                while (ServerConsoleQueue.Count>0)
                {
                    Script.Execute(ServerConsoleQueue.Dequeue());
                }
                ServerConsoleMutex.ReleaseMutex();

                int t = TickCount();
                float delta = t-last;
                last = t;

                if (ServerTickrate != 0)
                {
                    float want = 1000.0f / ServerTickrate;
                    float rate = (float)delta / 1000.0f * 3.0f;
                    if (delta > want)
                        sleeptime -= rate;
                    else
                        sleeptime += rate;
                    sleeptime = Math.Max(1, sleeptime);

                    Update(true);

                    if (sleeptime > 1)
                        Thread.Sleep((int)sleeptime);
                }
                else
                {
                    Update(true);
                }
                //Thread.Sleep(40);
            }
		}
        float sleeptime;

		public void ServerTick(float dtime)
		{
			Tick(dtime,true);

            frame++;
		}
		
		public bool ServerCategorizeReceivedEntity(int serverindex,int clientindex,short ownernumber,short sendernumber)
		{
			if(serverindex==0)
			{
				//noch nicht in der serverliste
				if(ownernumber==sendernumber)
				{
					return true;
				}
			}
			else
			{
				if(ownernumber==sendernumber)
				{
					return true;
				}
			}
			return false;
		}
		
		public bool ServerCategorizeSendEntity(int serverindex,int clientindex,short ownernumber,short sendernumber)
		{
			return true;
		}

        public void ServerOnStateReceive(Lidgren.Network.NetIncomingMessage msg, IPEndPoint ip)
        {
            short client = ((UdpServer)Connection).GetClientNumber(ip);
            DeSerializationContext context = new DeSerializationContext(msg);

            Scene.ServerStateReceive(context, client);
		}

		public void ServerOnConnect(short clientid,string name,IPEndPoint client)
		{
            CurrentFlow.OnJoin(clientid, name);
			//Lidgren.Library.Network.NetServer s=((UdpServer)Connection).server;
			//Lidgren.Library.Network.NetConnection c=((UdpServer)Connection).
        }

        public void ServerOnDisconnect(short clientid, string name, IPEndPoint client)
        {
            if(CurrentFlow!=null)
                CurrentFlow.OnLeave(clientid, name);
            if(Scene!=null)
                Scene.KillEntitiesOfClient(clientid);
        }

        public Thread ServerConsoleThread;
        public Queue<string> ServerConsoleQueue = new Queue<string>();
        public Mutex ServerConsoleMutex = new Mutex();

        public void ServerRun(bool dedicated)
		{
            int port = ((Config)ResourceManager.Load("config/global.config")).GetInteger("server.port");

            string password=null;
            if(ResourceManager.LoadConfig("config/global.config").Exists("server.password"))
                password=ResourceManager.LoadConfig("config/global.config").GetString("server.password");
            System.Console.WriteLine("port: " + port);
			
            if(Connection==null)
                Connection=new UdpServer(port,16,password);

            //ServerConsoleThread = new Thread(ServerConsoleThreadStart);
            //ServerConsoleThread.Start();

            if (dedicated)
                ServerLoop();
            else
                ClientLoop();
		}

        protected void ServerConsoleThreadStart()
        {
            string line;
            while ((line = System.Console.ReadLine()) != null)
            {
                ServerConsoleMutex.WaitOne();
                ServerConsoleQueue.Enqueue(line);
                ServerConsoleMutex.ReleaseMutex();
            }
        }

        public UdpServer ServerConnection
        {
            get
            {
                return ((UdpServer)Connection);
            }
        }

		public void ServerStop()
		{
			Quit=true;
            if (ServerWeb != null)
            {
                ServerWeb.Shutdown();
                ServerWeb = null;
            }
            if (Connection != null)
            {
                Connection.Disconnect();
                Connection = null;
            }

            if (ServerConsoleThread != null)
            {
                ServerConsoleThread.Abort();
                //ServerConsoleThread.
                ServerConsoleThread = null;
            }

            ClientSendEvent = null;
		}



		public float ServerTickrate=40;
        public int ServerUpdateDivisor = 1;
		public WebServer ServerWeb;
        public string ServerName;
        public string ServerAdmin;
        public string ServerAdminMail;
        public int ServerMaxPlayers;


////////////////////////////////////////////


        public static Root Instance
        {
            get
            {
                return singleton;
            }
        }/*
        public string GetModAssemblyPath(string assembly)
        {
            return "mods" + Path.DirectorySeparatorChar + Mod + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + assembly;
        }*/

        public string GetAssemblyPath(string assembly)
        {
            /*if (IsWindows)
                return "bin\\windows\\" + assembly;
            else
                return "bin/linux/" + assembly;*/
            FileInfo fi = new FileInfo(Assembly.GetExecutingAssembly().Location);
            return fi.DirectoryName+Path.DirectorySeparatorChar+assembly;
        }

        public Root(string[] args,bool authoritive)
        {
            Authoritive = authoritive;
            Args = args;
            singleton = this;

            Console.WriteLine("Process: " + (Is64BitProcess ? "64" : "32") + " Bit");
            Console.WriteLine("OS: " + (Is64BitOS ? "64" : "32") + " Bit");


			string home;
			int i=Array.IndexOf<string>(args, "-home");
			if(i!=-1)
			{
				home = args[i+1];
			}
			else
			{
            	home = IsWindows ? "Spacewar2006-User" : ".spacewar2006";
            	home = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + Path.DirectorySeparatorChar + home;
			}
            
			if (!Directory.Exists(home))
            {
                Console.WriteLine("creating user directory: " + home);
                try
                {
                    Directory.CreateDirectory(home);
                    Directory.CreateDirectory(home + Path.DirectorySeparatorChar + "config");
                    Directory.CreateDirectory(home + Path.DirectorySeparatorChar + "screens");
                    Directory.CreateDirectory(home + Path.DirectorySeparatorChar + "demos");
                    Directory.CreateDirectory(home + Path.DirectorySeparatorChar + "system");
                    File.Copy("config" + Path.DirectorySeparatorChar + "global.config", home + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "global.config");
                    //File.Copy("config" + Path.DirectorySeparatorChar + "controls.xml", home + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "controls.xml");
                    //File.Copy("config" + Path.DirectorySeparatorChar + "servers.config", home + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "servers.config");
                    //File.Copy("config" + Path.DirectorySeparatorChar + "classes.types", home + Path.DirectorySeparatorChar + "config" + Path.DirectorySeparatorChar + "classes.types");
                }
                catch (Exception)
                {
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
            FileSystem = new FileSystem(home);
            Factory = new Factory();
            ResourceManager = new ResourceManager(FileSystem, Factory);

            //FileSystem.Get("screens").CreateFile("test.bmp");

            /*
            Mod = ((Config)ResourceManager.Load("config/global.config", typeof(Config))).GetString("mod");
            Console.WriteLine("launching mod: " + Mod);
            ResourceManager.AddSearchPath("mods/" + Mod);

            Assembly a = ModAssembly = Assembly.LoadFrom(GetModAssemblyPath("game.dll"));

            Factory.UpdateTypeIds();

            Factory.Add(a);
            */

            Scene = new Scene();
            Script = new BooInterpreter();


            TickStart = TickCount();


        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Console.WriteLine("Resolving: " + args.Name);
            return null;
            /*if (args.Name.StartsWith("game,"))
            {
                return ModAssembly;
            }
            else*/
            {
                //string path=Directory.GetCurrentDirectory()+Path.DirectorySeparatorChar+GetAssemblyPath(args.Name.Split(',')[0]+".dll");
                string path = GetAssemblyPath(args.Name.Split(',')[0] + ".dll");
                Console.WriteLine("trying to load assembly " + path);
                try
                {
                    Assembly a = Assembly.LoadFile(path);
                    Factory.Add(a);
                    return a;
                }
                catch(Exception e)
                {
                    try
                    {
                        string path2 = GetAssemblyPath(args.Name.Split(',')[0] + ".exe");
                        Assembly a2 = Assembly.LoadFile(path2);
                        return a2;
                    }
                    catch (Exception e2)
                    {
                        return null;
                    }
                }
            }

            return null;
        }



        /*public virtual void DoEvents()
        {
        }
        */
        public bool IsWindows
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Win32NT
                || Environment.OSVersion.Platform == PlatformID.Win32S
                || Environment.OSVersion.Platform == PlatformID.WinCE
                || Environment.OSVersion.Platform == PlatformID.Win32Windows;
            }
        }
        public bool IsUnix
        {
            get
            {
                return Environment.OSVersion.Platform == PlatformID.Unix;
            }
        }

        public Events GenerateEventPacket()
        {
            if (EventSendQueue.Count > 0)
            {
                Events e = new Events(EventSendQueue);
                EventSendQueue = new List<EventReplicationInfo>();
                return e;
            }
            else
            {
                return null;
            }
        }

        /*public virtual void ReceiveUpdates()
        {
        }*/


        public State GenerateState(int targetclientnumber)
        {
            State s = new State(Scene);
            //s.Time = Time;
            return s;
        }

        //public virtual void ApplyState(int sourceclientnumber,State s,Hashtable clientlist,Hashtable serverlist)
        //{
        //}

        /*	public virtual void OnStateDataReceive(Stream s,BinaryReader r)
            {
                Scene.DeSerialize(Factory,s,r);
            }
        */
       /* public virtual void OnStateReceive(Stream s, BinaryReader r,System.Net.IPEndPoint ep)
        {
            //Scene.DeSerialize(Factory,s,r);
        }
        */
        public enum ReceiveRule
        {
            UpdateExisting,
            CreateNew,
            Ignore,
        }

        public enum SendRule
        {
            Send,
            Ignore
        }
/*
        public virtual bool CategorizeReceivedEntity(int serverindex, int clientindex,short ownernumber,short sendernumber)
        {
            return false;
        }
        */
        public bool CategorizeSendEntity(int serverindex, int clientindex, short ownernumber, short sendernumber)
        {
            if (Authoritive)
                return ServerCategorizeSendEntity(serverindex, clientindex, ownernumber, sendernumber);
            else
                return ClientCategorizeSendEntity(serverindex, clientindex, ownernumber, sendernumber);
        }

        public void OnDatagramReceive(Lidgren.Network.NetIncomingMessage msg, IPEndPoint sender)
        {
            if (IsAuthoritive)
            {
                switch (msg.MessageType)
                {
                    case Lidgren.Network.NetIncomingMessageType.DiscoveryRequest:
                        return;
                }
            }
            //try
            {
                //Stream m1 = msg.GetStream();
                /*if (m1.Length < 2)
                {
                    System.Console.WriteLine("received short packet, dropping.");
                    return;
                }*/
                //BinaryReader r = new BinaryReader(m1);
                //string typename=r.ReadString();
                //Type type=Factory.GetType(typename);
                //HACK
                short typeid = msg.ReadInt16();
                Type type;
                try
                {
                    type = Factory.GetType(typeid);
                }
                catch
                {
                    System.Console.WriteLine("cant find type, packet dropped.");
                    return;

                }
                //Stream m = msg.GetStream();

                if (type == typeof(State))
                {
                    //if (msg.Connected)
                    {
                        if(IsAuthoritive)
                            ServerOnStateReceive(msg, sender);
                        else
                            ClientOnStateReceive(msg);
                    }
                    //object o=Factory.DeSerialize(m);
                    //Packet p=(Packet)o;
                    return;
                }
                msg.Position = 0;
                if (type == typeof(Events))
                {
                    //if (msg.Connected)
                    {
                        Events e = (Events)Factory.DeSerialize(new DeSerializationContext(msg));
                        //System.Console.WriteLine(e.ToString());
                        if (EventSendQueue.Count != 0)
                            throw new Exception("");
                        if (Connection is UdpServer)
                        {
                            ((UdpServer)Connection).SendNot(e, sender);
                        }
                        e.RaiseAll();
                        EventSendQueue.Clear();
                    }
                }
                else if (type == typeof(Command))
                {
                    Command e = (Command)Factory.DeSerialize(new DeSerializationContext(msg));
                    e.Execute();
                }
                else
                {
                    Console.WriteLine("b0rk packet received.");
                    //object o=Factory.DeSerialize(m);
                    //Packet p=(Packet)o;
                    //Console.WriteLine("Packet Received: "+p.GetType().ToString());
                }
            }
            /*catch (Exception e)
            {
                Console.WriteLine("exception raised while receiving. packet dumped.");
                FileStream f = new FileStream("packet.dump", FileMode.Create, FileAccess.Write);
                byte[] data = msg.GetData();
                f.Write(data, 0, msg.Length);
                f.Close();
                throw e;
            }*/
        }

        public void Tick(float dtime,bool server)
        {
            //Timer1s.Tick(dtime);

            ResourceManager.Tick(dtime);
            foreach (object obj in LocalObjects)
            {
                if (obj is ITickable)
                {
                    ((ITickable)obj).Tick(dtime);
                }
            }

            if (Gui != null)
                Gui.Tick(dtime);
            Scene.Tick(dtime);

            if (CurrentFlow != null)
                CurrentFlow.Tick(dtime);

            if (Connection != null)
            {
                if (Connection is ITickable)
                    ((ITickable)Connection).Tick(dtime);

                if (Connection is UdpServer)
                    ServerSendUpdates();
                else
                    ClientSendUpdates();

                //SendUpdates();

                Lidgren.Network.NetIncomingMessage msg;
                IPEndPoint ip;
                while ((msg = Connection.Receive(out ip)) != null)
                {
                    //if(Server
                    OnDatagramReceive(msg,ip);
                }
            }

            if (Player != null)
            {
                Player.Tick(dtime);

                byte[] packet;
                while ((packet = Player.ReadPacket()) != null)
                {
                    Lidgren.Network.NetIncomingMessage m = new Lidgren.Network.NetIncomingMessage(packet,packet.Length);
                    //m.Write(packet);
                    OnDatagramReceive(m,null);
                }
            }

        }

        public override string ToString()
        {
            return Scene.ToString();
        }

        public int TickCount()
        {
            //the windows timer suxxx
            return Environment.TickCount;
            /*if (IsWindows)
            {
                return Sdl.SDL_GetTicks();
            }
            else
            {
                return Environment.TickCount;
            }*/
        }

        public void Update(bool server)
        {
            int now = TickCount();
            if (LastTickCount == 0)
            {
                LastTickCount = now;
            }
            

            int dt = now - LastTickCount;
            if (dt > 1000 && ClientRecordVideo < 0)
            {
                //<1fps!
                Console.WriteLine("warning: <1FPS!");
                dt = 1000;
            }

            if (dt <= 5)
            {
                Thread.Sleep(1);
                return;
            }


            float fdt = (float)dt / 1000;
            TickDelta = fdt;

            if (LockTimeDelta > 0.0f)
            {
                fdt = LockTimeDelta;
            }

            Time += fdt;

            InfoCounter += fdt;
            InfoCycles++;
            if (InfoCounter >= InfoInterval && InfoInterval > 0)
            {
                UpdateInfo(InfoCounter, InfoCycles);
                InfoCounter = 0;
                InfoCycles = 0;
            }

            if (server)
                ServerTick(fdt);
            else
                ClientTick(fdt);

            LastTickCount = now;
            Cycle++;
        }

        public float TickDelta = 0;

        ~Root()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                if (UserInterface != null)
                    UserInterface.Dispose();
                ResourceManager.Dispose();
                ResourceManager = null;
                Disposed = true;
            }
        }

        public List<EventReplicationInfo> EventSendQueue = new List<EventReplicationInfo>();
        public IUserInterface UserInterface;
        public ResourceManager ResourceManager;
        public ShaderManager ShaderManager;
        public FileSystem FileSystem;
        public Scene Scene;
        public Gui Gui;
        //public object current;
        public int frame;
        public Factory Factory;
        public IConnection Connection;
        public ArrayList LocalObjects = new ArrayList();
        public Flow CurrentFlow;
        //public ResourceMap Resources;
        public IInterpreter Script;
        public bool Quit = false;
        public float LockTimeDelta = -1.0f;

        protected int Cycle = 1;
        public int NextIndex = 1;
        protected int LastTickCount = 0;
        public float InfoInterval = 1;
        public float InfoCounter;
        public int InfoCycles;
        public float Time = 0;
        public int TickStart;
        bool Disposed;


        //[ThreadStatic]
        protected static Root singleton;

        public string[] Args;

        public DemoRecorder Recorder = null;
        public DemoPlayer Player = null;

        public Mod Mod;
    }

    public abstract class Mod
    {
        public abstract int Version
        {
            get;
        }

        public abstract Version AssemblyVersion
        {
            get;
        }
        public abstract string GameString
        {
            get;
        }

    }
}
