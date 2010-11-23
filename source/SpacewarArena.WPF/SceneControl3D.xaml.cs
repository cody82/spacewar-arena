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
using System.Windows.Media.Media3D;
using Material = System.Windows.Media.Media3D.Material;

namespace SpacewarArena.WPF
{
    /// <summary>
    /// Interaktionslogik für SceneControl3D.xaml
    /// </summary>
    public partial class SceneControl3D : UserControl
    {
        public SceneControl3D()
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

                this.viewport.Children.Clear();
                entities = new Dictionary<Entity, ModelVisual3D>();

                scene = value;
                scene.SpawnEvent += scene_SpawnEvent;
                scene.RemoveEvent += scene_RemoveEvent;
                foreach (Entity e in scene.FindEntitiesByType<Node>())
                {
                    AddEntity(e);
                }
            }
        }

        Dictionary<Entity, ModelVisual3D> entities;

        private void AddEntity(Entity e)
        {
            Node n=e as Node;
            if(n==null)
                return;

            MeshGeometry3D triangleMesh = new MeshGeometry3D();
            Point3D point0 = new Point3D(0, 0, 0);
            Point3D point1 = new Point3D(5, 0, 0);
            Point3D point2 = new Point3D(0, 0, 5);
            triangleMesh.Positions.Add(point0);
            triangleMesh.Positions.Add(point1);
            triangleMesh.Positions.Add(point2);
            triangleMesh.TriangleIndices.Add(0);
            triangleMesh.TriangleIndices.Add(2);
            triangleMesh.TriangleIndices.Add(1);
            Vector3D normal = new Vector3D(0, 1, 0);
            triangleMesh.Normals.Add(normal);
            triangleMesh.Normals.Add(normal);
            triangleMesh.Normals.Add(normal);
            Material material = new DiffuseMaterial(
                new SolidColorBrush(Colors.DarkKhaki));
            GeometryModel3D triangleModel = new GeometryModel3D(
                triangleMesh, material);
            ModelVisual3D model = new ModelVisual3D();
            model.Content = triangleModel;
            this.viewport.Children.Add(model);
            entities[e] = model;

        }

        void scene_RemoveEvent(Entity e)
        {
            ModelVisual3D model = entities[e];
            this.viewport.Children.Remove(model);
            entities.Remove(e);
        }

        void scene_SpawnEvent(Entity e)
        {
            AddEntity(e);
        }
    }
}
