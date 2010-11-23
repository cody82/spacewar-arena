using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Cheetah;
using Cheetah.Graphics;

namespace SpacewarArena.WPF
{
    /// <summary>
    /// Interaktionslogik für SceneControl.xaml
    /// </summary>
    public partial class SceneControl : UserControl
    {
        public SceneControl()
        {
            InitializeComponent();
        }


        protected Scene scene;
        public Scene Scene
        {
            get { return scene; }
            set
            {
                if (scene != null)
                {
                    scene.SpawnEvent -= scene_SpawnEvent;
                    scene.RemoveEvent -= scene_RemoveEvent;
                }

                scene = value;
                scene.SpawnEvent += scene_SpawnEvent;
                scene.RemoveEvent += scene_RemoveEvent;
                foreach (Entity e in scene.FindEntitiesByType<Node>())
                {
                    AddEntity(e);
                }
            }
        }

        private void AddEntity(Entity e)
        {
            Ellipse visual = new Ellipse();

            Node n=e as Node;
            if(n==null)
                return;

            visual.Width = visual.Height = n.RenderRadius*2;
            visual.
        }

        void scene_RemoveEvent(Entity e)
        {
            throw new NotImplementedException();
        }

        void scene_SpawnEvent(Entity e)
        {
            throw new NotImplementedException();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            Brush b = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            Pen p=new Pen(b,2);
            drawingContext.DrawLine(p, new Point(0, 0), new Point(100, 100));

        }
    }
}
