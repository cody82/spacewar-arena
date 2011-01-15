using System;
using System.Threading;
using System.Reflection;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Net;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using SpaceWar2006.Controls;
using SpaceWar2006.GameObjects;
using Cheetah;
using OpenTK;

namespace SpaceWar2006.Ai
{

    public class SpaceShipBotControl : SpaceShipControlBase
    {
        public SpaceShipBotControl(SpaceShip s)
            : base(s)
        {
            ChangeToDefaultTask();
        }

        protected override SpaceShipControlInput GetControls()
        {
            if (CurrentTask != null)
            {
                return CurrentTask.GetConstrols();
            }
            else
            {
                return null;
            }
        }

        public override void Tick(float dtime)
        {
            if (CurrentTask != null)
            {
                Time += dtime;
                CurrentTask.Tick(dtime);
                if (CurrentTask.Done || (MaxTime > 0 && Time >= MaxTime))
                {
                    if (CurrentTask.NextTask != null)
                    {
                        ChangeTask(CurrentTask.NextTask);
                    }
                    else
                    {
                        ChangeToDefaultTask();
                    }
                }
            }
            if (CurrentTask == null && Todo.Count > 0)
            {
                ChangeTask(Todo.Dequeue());
            }

            base.Tick(dtime);
        }

        public void ChangeToDefaultTask()
        {
            ChangeTask(new Ai.Search(Target));
        }
        public void ChangeTask(Ai.Task t)
        {
            Time = 0;
            CurrentTask = t;
            Cheetah.Console.WriteLine("bot task: " + t.GetType().Name);
        }

        Ai.Task CurrentTask;
        float Time;
        float MaxTime = 0;
        Queue<Ai.Task> Todo = new Queue<Ai.Task>();
    }

    public abstract class Task : ITickable
    {
        public Task(SpaceShip owner)
        {
            Owner = owner;
        }

        /*public virtual AiTask NextTask()
        {
            return null;
        }*/

        public virtual void Tick(float dtime)
        {
            Age += dtime;
        }

        public virtual SpaceShipControlInput GetConstrols()
        {
            return null;
        }

        public float Age = 0;
        public SpaceShip Owner;
        public bool Done;
        public Task NextTask;
    }


    public class Attack : Task
    {
        public Attack(SpaceShip owner, Actor target)
            : base(owner)
        {
            Target = target;
        }

        public override SpaceShipControlInput GetConstrols()
        {
            SpaceShipControlInput input = new SpaceShipControlInput();

            input.LookAt = Target.AbsolutePosition;
            input.Thrust = 1;

            float dist = Owner.Distance(Target);

            Vector3 dir = Target.AbsolutePosition - Owner.AbsolutePosition;
            if (dir.LengthSquared > 0)
            {
                dir.Normalize();
                if ((float)Math.Acos(Vector3.Dot(dir, Owner.Direction)) < (float)Math.PI / 18 && dist < 2000)
                    input.Fire1 = true;
            }

            if (dist < 1000)
            {
                input.Strafe = 1;
                strafetime += Root.Instance.TickDelta;
                if (strafetime > 10)
                {
                    Done = true;
                    NextTask = new Retreat(Owner, Target);
                }
            }
            else
            {
                strafetime = 0;
            }

            if (dist < Owner.Radius * 3)
            {
                Done = true;
                NextTask = new Retreat(Owner, Target);
            }
            return input;
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            if (Target.Kill)
            {
                Done = true;
                NextTask = new Search(Owner);
            }
        }

        float strafetime = 0;

        Actor Target;
    }

    public class Stop : Task
    {
        public Stop(SpaceShip owner)
            : base(owner)
        {
        }
    }

    public class Wait : Task
    {
        public Wait(SpaceShip owner)
            : base(owner)
        {
        }
    }

    public class Search : Task
    {
        public Search(SpaceShip owner)
            : base(owner)
        {
        }

        public override SpaceShipControlInput GetConstrols()
        {
            SpaceShipControlInput input = new SpaceShipControlInput();


            return input;
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            ICollection<Actor> list = Owner.Computer.Scan();

            float dist = float.MaxValue;
            SpaceShip target = null;
            foreach (Actor a in list)
            {
                if (a is SpaceShip && a.Distance(Owner) < dist)
                {
                    SpaceShip s = (SpaceShip)a;
                    Player p1 = Owner.GetPlayer();
                    Player p2 = s.GetPlayer();
                    if (p1 == null || p2 == null || p1.Team == -1 || p2.Team == -1 || p1.Team != p2.Team)
                    {
                        target = (SpaceShip)a;
                    }
                }
            }

            if (target != null)
            {
                Done = true;
                if (Owner.Distance(target) > 1000)
                    NextTask = new FlySmart(Owner, target, 800);
                else
                    NextTask = new Attack(Owner, target);
            }
        }
    }

    public class Follow : Task
    {
        public Follow(SpaceShip owner, Actor target)
            : base(owner)
        {
            Target = target;
        }

        Actor Target;
    }

    public class Retreat : Task
    {
        public Retreat(SpaceShip owner, Actor target)
            : base(owner)
        {
            Target = target;
        }

        public override SpaceShipControlInput GetConstrols()
        {
            SpaceShipControlInput input = new SpaceShipControlInput();
            input.LookAt = 2 * Owner.AbsolutePosition - Target.AbsolutePosition;
            input.Thrust = 1;

            return input;
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            if (Owner.Distance(Target) >= 2000 || Age > 5)
            {
                Done = true;
            }
        }
        Actor Target;
    }
    public class Patrol : Task
    {
        public Patrol(SpaceShip owner, Node[] waypoints)
            : base(owner)
        {
            Waypoints = waypoints;
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);
            Plan();
        }
        void Plan()
        {
            //NextTask = new FlyTo(Owner, Waypoints[NextWaypoint].AbsolutePosition);
            NextTask = new FlySmart(Owner, Waypoints[NextWaypoint], -1);
            NextTask.NextTask = this;
            NextWaypoint = (NextWaypoint + 1) % Waypoints.Length;
            Done = true;
            //Cheetah.Console.WriteLine("xxx");
        }

        int NextWaypoint;
        Node[] Waypoints;
    }

    public class AStarMap
    {
        /// <summary>
        /// A node class for doing pathfinding on a 2-dimensional map
        /// </summary>
        public class AStarNode2D : AStarNode
        {
            #region Properties

            /// <summary>
            /// The X-coordinate of the node
            /// </summary>
            public int X
            {
                get
                {
                    return FX;
                }
            }
            private int FX;

            /// <summary>
            /// The Y-coordinate of the node
            /// </summary>
            public int Y
            {
                get
                {
                    return FY;
                }
            }
            private int FY;

            #endregion

            AStarMap Map;
            #region Constructors

            /// <summary>
            /// Constructor for a node in a 2-dimensional map
            /// </summary>
            /// <param name="AParent">Parent of the node</param>
            /// <param name="AGoalNode">Goal node</param>
            /// <param name="ACost">Accumulative cost</param>
            /// <param name="AX">X-coordinate</param>
            /// <param name="AY">Y-coordinate</param>
            public AStarNode2D(AStarNode AParent, AStarNode AGoalNode, double ACost, int AX, int AY,AStarMap map)
                : base(AParent, AGoalNode, ACost)
            {
                FX = AX;
                FY = AY;
                Map = map;
            }

            #endregion

            #region Private Methods

            /// <summary>
            /// Adds a successor to a list if it is not impassible or the parent node
            /// </summary>
            /// <param name="ASuccessors">List of successors</param>
            /// <param name="AX">X-coordinate</param>
            /// <param name="AY">Y-coordinate</param>
            private void AddSuccessor(ArrayList ASuccessors, int AX, int AY)
            {
                int CurrentCost = Map.GetMap(AX, AY);
                if (CurrentCost == -1)
                {
                    return;
                }
                AStarNode2D NewNode = new AStarNode2D(this, GoalNode, Cost + CurrentCost, AX, AY, Map);
                if (NewNode.IsSameState(Parent))
                {
                    return;
                }
                ASuccessors.Add(NewNode);
            }

            #endregion

            #region Overidden Methods

            /// <summary>
            /// Determines wheather the current node is the same state as the on passed.
            /// </summary>
            /// <param name="ANode">AStarNode to compare the current node to</param>
            /// <returns>Returns true if they are the same state</returns>
            public override bool IsSameState(AStarNode ANode)
            {
                if (ANode == null)
                {
                    return false;
                }
                return ((((AStarNode2D)ANode).X == FX) &&
                    (((AStarNode2D)ANode).Y == FY));
            }

            /// <summary>
            /// Calculates the estimated cost for the remaining trip to the goal.
            /// </summary>
            public override void Calculate()
            {
                if (GoalNode != null)
                {
                    double xd = FX - ((AStarNode2D)GoalNode).X;
                    double yd = FY - ((AStarNode2D)GoalNode).Y;
                    // "Euclidean distance" - Used when search can move at any angle.
                    //GoalEstimate = Math.Sqrt((xd*xd) + (yd*yd));
                    // "Manhattan Distance" - Used when search can only move vertically and 
                    // horizontally.
                    //GoalEstimate = Math.Abs(xd) + Math.Abs(yd); 
                    // "Diagonal Distance" - Used when the search can move in 8 directions.
                    GoalEstimate = Math.Max(Math.Abs(xd), Math.Abs(yd));
                }
                else
                {
                    GoalEstimate = 0;
                }
            }

            /// <summary>
            /// Gets all successors nodes from the current node and adds them to the successor list
            /// </summary>
            /// <param name="ASuccessors">List in which the successors will be added</param>
            public override void GetSuccessors(ArrayList ASuccessors)
            {
                ASuccessors.Clear();
                AddSuccessor(ASuccessors, FX - 1, FY);
                AddSuccessor(ASuccessors, FX - 1, FY - 1);
                AddSuccessor(ASuccessors, FX, FY - 1);
                AddSuccessor(ASuccessors, FX + 1, FY - 1);
                AddSuccessor(ASuccessors, FX + 1, FY);
                AddSuccessor(ASuccessors, FX + 1, FY + 1);
                AddSuccessor(ASuccessors, FX, FY + 1);
                AddSuccessor(ASuccessors, FX - 1, FY + 1);
            }

            /// <summary>
            /// Prints information about the current node
            /// </summary>
            public override void PrintNodeInfo()
            {
                System.Console.WriteLine("X:\t{0}\tY:\t{1}\tCost:\t{2}\tEst:\t{3}\tTotal:\t{4}", FX, FY, Cost, GoalEstimate, TotalCost);
            }

            #endregion
        }

        public AStarMap(float cellsize, Node owner)
        {
            CellSize = cellsize;
            Owner = owner;
        }

        public void Print(int xfrom,int yfrom,int xto,int yto)
        {
            for (int y = yfrom; y < yto; ++y)
            {
                for (int x = xfrom; x < xto; ++x)
                {
                    System.Console.Write(GetMap(x, y) == 1 ? "." : "x");
                }
                System.Console.WriteLine("");
            }
        }

        public Point GetNearestFreePosition(Point p)
        {
            if(GetMap(p.X,p.Y)!=-1)
                return p;

            int r = 1;

            for (r = 1; r < 100; ++r)
            {
                for (int x = -r; x <= r; x+=2*r)
                {
                    for (int y = -r; y <= r; y++)
                    {
                        if (GetMap(p.X+x, p.Y+y) != -1)
                            return p;
                    }
                }
                for (int y = -r; y <= r; y += 2 * r)
                {
                    for (int x = -r; x <= r; x++)
                    {
                        if (GetMap(p.X + x, p.Y + y) != -1)
                            return p;
                    }
                }
            }
            throw new Exception();
        }

        float CellSize;
        Node Owner;
        public int GetMap(int x, int y)
        {
            //return 1;
            //System.Console.WriteLine(x.ToString() + " " + y.ToString());
            float mapsize = 5000.0f / CellSize;
            if (Math.Abs(x) > mapsize || Math.Abs(y) > mapsize)
            {
                return -1;
            }

            foreach (CollisionInfo ci in GetObstacles())
            {
                if (ci.Check(new SphereCollisionInfo(new Vector3((float)x * CellSize + 0.5f * CellSize, 0, (float)y * CellSize + 0.5f * CellSize), new Vector3(CellSize * 0.5f, CellSize * 0.5f, 0).Length)))
                {
                    //throw new Exception();
                    return -1;
                }
            }
            //throw new Exception();
            return 1;
        }

        public Point GetCell(Vector3 pos)
        {
            return new Point((int)(pos.X / CellSize), (int)(pos.Z / CellSize));
        }
        public Vector3 GetPosition(int x, int y)
        {
            return new Vector3(x * CellSize + 0.5f * CellSize, 0, y * CellSize + 0.5f * CellSize);
        }

        CollisionInfo[] GetObstacles()
        {
            //return Root.Instance.Scene.FindEntitiesByType<Node>();
            
            List<CollisionInfo> list = new List<CollisionInfo>(); 
            foreach (Node n in Root.Instance.Scene.FindEntitiesByType<Node>())
            {
                if (!n.CanCollide(Owner) || n == Owner || n is SpaceWar2006.Weapons.Projectile || n is SpaceWar2006.Effects.EclipticNode || n is SpaceWar2006.GameObjects.SpawnPoint || n is SpaceWar2006.GameObjects.Flag || n is SpaceWar2006.GameObjects.SpaceShip)
                    continue;

                CollisionInfo ci = n.GetCollisionInfo();
                if (ci != null)
                {
                    //System.Console.WriteLine(n.ToString());
                    list.Add(ci);
                }
            }
            return list.ToArray();
        }
    }

    public class FlySmart : FlyTo
    {
        public FlySmart(SpaceShip owner, Node target, float dist)
            :base(owner,target.AbsolutePosition)
        {
            map = new AStarMap(400, owner);
            Dist = dist;
            TargetNode = target;

            astar = new AStar();

            //map.Print(-22,-22,22,22);


            Point p1 = map.GetCell(target.AbsolutePosition);
            Point p2 = map.GetCell(owner.AbsolutePosition);

            p1 = map.GetNearestFreePosition(p1);
            p2 = map.GetNearestFreePosition(p2);

            if (p1 == p2)
                System.Console.WriteLine("start=end!");

            AStarMap.AStarNode2D GoalNode = new AStarMap.AStarNode2D(null, null, 0, p1.X, p1.Y, map);
            AStarMap.AStarNode2D StartNode = new AStarMap.AStarNode2D(null, null, 0, p2.X, p2.Y, map);
            StartNode.GoalNode = GoalNode;
            astar.FindPath(StartNode, GoalNode);
            if (astar.Solution.Count == 0)
            {
                System.Console.WriteLine("no solution! from " + p2 + " to " + p1);
                Done = true;
            }
            PrintSolution(astar.Solution);
            currentnode = 0;
        }
        float Dist;
        Node TargetNode;

        public override SpaceShipControlInput GetConstrols()
        {
            SpaceShipControlInput i = base.GetConstrols();
            if (Done)
            {
                Done = false;
                currentnode++;
                if (currentnode >= astar.Solution.Count)
                {
                    currentnode--;
                    Done = true;
                }
            }
            return i;
        }
        int currentnode;
        AStar astar;
        /// <summary>
        /// Prints the solution
        /// </summary>
        /// <param name="ASolution">The list that holds the solution</param>
        public void PrintSolution(ArrayList ASolution)
        {
            for (int j = -20; j <21; j++)
            {
                for (int i = -20; i < 21; i++)
                {
                    bool solution = false;
                    foreach (AStarMap.AStarNode2D n in ASolution)
                    {
                        AStarMap.AStarNode2D tmp = new AStarMap.AStarNode2D(null, null, 0, i, j,map);
                        solution = n.IsSameState(tmp);
                        if (solution)
                            break;
                    }
                    if (solution)
                        System.Console.Write("S ");
                    else
                        if (map.GetMap(i, j) == -1)
                            System.Console.Write("X ");
                        else
                            System.Console.Write(". ");
                }
                System.Console.WriteLine("");
            }
        }
        AStarMap map;

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            if (Owner.Distance(TargetNode) < Dist)
            {
                Done = true;
                return;

            }
            Point p2 = map.GetCell(Owner.AbsolutePosition);

            for (int i = 0; i < astar.Solution.Count;++i )
            {
                AStarMap.AStarNode2D node = (AStarMap.AStarNode2D)astar.Solution[i];
                Point p1 = new Point(node.X, node.Y);

                if (p2 == p1)
                {
                    if (i == astar.Solution.Count - 1)
                    {
                        Done = true;
                        System.Console.WriteLine("arrived at last node");
                    }
                    else if(i>currentnode-1)
                    {
                        currentnode=i;
                        System.Console.WriteLine("next node: " + ++currentnode);
                    }
                }
            }
        }

        Vector3 NextTarget
        {
            get
            {
                if (Done)
                    return Owner.AbsolutePosition;

                AStarMap.AStarNode2D node = (AStarMap.AStarNode2D)astar.Solution[currentnode];
                return map.GetPosition(node.X, node.Y);
            }
        }
        //List<Vector3> Checkpoints = new List<Vector3>();

        IList<Node> GetObstacles()
        {
            return Root.Instance.Scene.FindEntitiesByType<Node>();
            /*
            List<CollisionInfo> list = new List<CollisionInfo>(); 
            foreach (Node n in Root.Instance.Scene.FindEntitiesByType<Node>())
            {
                CollisionInfo ci = n.GetCollisionInfo();
                if (ci != null)
                    list.Add(ci);
            }
            return list.ToArray();*/
        }

        protected override Vector3 Target
        {
            get
            {
                return NextTarget;
            }
            set
            {
                target = value;
            }
        }

        float Safety = 100;
    }

    public class FlyTo : Task
    {
        public FlyTo(SpaceShip owner, Vector3 target)
            : base(owner)
        {
            this.target = target;
        }


        public override SpaceShipControlInput GetConstrols()
        {
            SpaceShipControlInput input = new SpaceShipControlInput();

            input.LookAt = Target;

            Vector3 dir = Target - Owner.AbsolutePosition;
            float dist = dir.Length;
            if (dist > lastdist)
            {
                stop = true;
                lastdist = float.PositiveInfinity;
            }

            if (Owner.Speed.Length < 1)
                stop = false;

            input.Strafe = stop?0:Math.Sign(Owner.GetCosDirection(Target));

            input.Thrust = stop?0:Math.Min((dist*dist)/(500.0f*500.0f),1.0f);

            if (dist < 200)
                Done = true;

            return input;
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);


        }
        protected Vector3 target;
        bool stop = false;
        float lastdist = float.PositiveInfinity;

        protected virtual Vector3 Target
        {
            get
            {
                return target;
            }
            set
            {
                target = value;
            }
        }
    }
}