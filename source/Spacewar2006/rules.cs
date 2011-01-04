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

using SpaceWar2006.GameObjects;
using SpaceWar2006.Weapons;
using Cheetah;
using Cheetah.Graphics;

namespace SpaceWar2006.Rules
{

    public class CaptureTheFlag : TeamDeathMatch
    {
        public CaptureTheFlag(Team[] teams, int capturelimit, float timelimit)
            : base(teams, 0, timelimit)
        {
            CaptureLimit = capturelimit;
        }

        public CaptureTheFlag(DeSerializationContext context)
            : base(context)
        {
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);

            CaptureLimit = context.ReadByte();
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);

            context.Write((byte)CaptureLimit);
        }

        protected override Team CreateTeam()
        {
            return new CtfTeam();
        }

        public override GameRule New()
        {
            Team[] newteams = new Team[Teams.Length];
            for (int i = 0; i < newteams.Length; ++i)
            {
                newteams[i] = CreateTeam();
                newteams[i].Index = Teams[i].Index;
                newteams[i].Name = Teams[i].Name;
            }

            return new CaptureTheFlag(newteams, CaptureLimit, TimeLimit);
        }
        public override string ToString()
        {
            string time = ", Time: " + ((int)TimeElapsed).ToString();
            string frags = "";
            if (CaptureLimit > 0)
                frags = ", CaptureLimit: " + CaptureLimit.ToString();
            if (TimeLimit > 0)
                time += "/" + (int)TimeLimit;

            return "CaptureTheFlag" + frags + time;
        }
        public void TakeFlag(Flag f, SpaceShip ship)
        {
            Player p = ship.GetPlayer();
            Announce(p.Name + " has taken the " + Team.ColorNames[f.Team] + " flag!");
        }

        public void DropFlag(Flag f, SpaceShip ship)
        {
            Player p = ship.GetPlayer();
            Announce(p.Name + " dropped the " + Team.ColorNames[f.Team] + " flag!");
        }

        public void ReturnFlag(Flag f, SpaceShip ship)
        {
            if (ship != null)
            {
                Player p = ship.GetPlayer();
                Announce(p.Name + " returned the " + Team.ColorNames[f.Team] + " flag!");
            }
            else
            {
                Announce("The " + Team.ColorNames[f.Team] + " flag was returned.");
            }
        }

        public void CaptureFlag(Flag f, SpaceShip ship)
        {
            Player p = ship.GetPlayer();
            Announce(p.Name + " captured the " + Team.ColorNames[f.Team] + " flag!");
            int cap = ++((CtfTeam)Teams[p.Team]).Captures;
            if (cap >= CaptureLimit && CaptureLimit > 0)
            {
                Announce(Team.ColorNames[p.Team] + " wins the match!");
                EndGame();
            }
        }

        int CaptureLimit;
    }

    public interface IRuleCreator
    {
        Rules.GameRule CreateRule();
    }
    public abstract class GameRule : Entity
    {
        public delegate void AnnounceDelegate(string text);
        public event AnnounceDelegate AnnounceEvent;

        public virtual void Announce(string text)
        {
            //Cheetah.Console.WriteLine(text);
            if (AnnounceEvent != null)
                AnnounceEvent(text);
            if(Root.Instance.IsAuthoritive)
                ReplicateCall("AnnounceReplication", new string[] { text });
        }

        /*public abstract void Reset()
        {
        }*/

        public abstract GameRule New();

        public override void OnAdd(Scene s)
        {
            base.OnAdd(s);
            s.SpawnEvent += new Scene.SpawnDelegate(SpawnEvent);
            s.RemoveEvent += new Scene.SpawnDelegate(RemoveEvent);
        }

        void RemoveEvent(Entity e)
        {
            if (Root.Instance.IsAuthoritive)
            {
                if (e is Player)
                {
                    Announce(((Player)e).Name + " left the game.");
                }
            }
        }
        void SpawnEvent(Entity e)
        {
            if (Root.Instance.IsAuthoritive)
            {
                if (e is Player)
                {
                    Announce(((Player)e).Name + " entered the game.");
                }
            }
        }

        public void AnnounceReplication(string text)
        {
            if (AnnounceEvent != null)
                AnnounceEvent(text);
        }

        public GameRule()
        {
            TimeLimit = 0;
        }

        public GameRule(float timelimit)
        {
            TimeLimit = timelimit;
        }

        public GameRule(DeSerializationContext context)
            : base(context)
        {
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);

            TimeElapsed = context.ReadSingle();
            TimeLimit = context.ReadSingle();
            GameOver = context.ReadBoolean();
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);

            context.Write(TimeElapsed);
            context.Write(TimeLimit);
            context.Write(GameOver);
        }

        public override void Tick(float dtime)
        {
            TimeElapsed += dtime;

            if (TimeElapsed >= TimeLimit && TimeLimit > 0.0f && !GameOver)
            {
                TimeLimitHit();
            }

            if (GameOver)
            {
                CurrentWaitTime += dtime;
                if (!Quitted && CurrentWaitTime >= PostGameWaitTime)
                {
                    Quit();
                    Quitted = true;
                }
            }
        }

        public void Quit()
        {
            Flow f = Root.Instance.CurrentFlow;
            //Root.Instance.CurrentFlow = null;
            f.Finished = true;
            f.Stop();
        }

        protected virtual void TimeLimitHit()
        {
            Announce("Time limit hit.");
            EndGame();
        }

        protected virtual void EndGame()
        {
            GameOver = true;
            Announce("Game has ended.");
        }

        public virtual void ActorDestroy(Actor killer, Actor victim, Projectile p)
        {

        }

        public float TimeRemaining
        {
            get
            {
                return TimeLimit - TimeElapsed;
            }
        }

        public virtual Player CreatePlayer(short clientid, string name)
        {
            return new Player(clientid, name);
        }
        public float TimeLimit;
        public float TimeElapsed = 0;
        public bool GameOver = false;
        public float PostGameWaitTime = 10;
        public float CurrentWaitTime = 0;
        public bool Quitted = false;
    }

    public abstract class LastManStanding : GameRule
    {
    }


    public class DeathMatch : GameRule
    {

        public DeathMatch(DeSerializationContext context)
            : base(context)
        {
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);

            FragLimit = context.ReadInt16();
        }
        public override GameRule New()
        {
            return new DeathMatch(FragLimit, TimeLimit);
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);

            context.Write((short)FragLimit);
        }

        public override string ToString()
        {
            string time = ", Time: " + ((int)TimeElapsed).ToString();
            string frags = "";
            if (FragLimit > 0)
                frags = ", Fraglimit: " + FragLimit.ToString();
            if (TimeLimit > 0)
                time += "/" + (int)TimeLimit;

            return "DeathMatch" + frags + time;
        }

        public DeathMatch(int fraglimit)
        {
            FragLimit = fraglimit;
        }
        public DeathMatch()
        {
            FragLimit = 0;
        }
        public DeathMatch(int fraglimit, float timelimit)
            : base(timelimit)
        {
            FragLimit = fraglimit;
        }

        public int FragLimit;

        protected virtual void Suicide(Player b00n)
        {
            b00n.Frags--;
            b00n.Deaths++;
            Announce(b00n.Name + " killed his own dumb self.");
        }

        protected virtual void Frag(Player killer, Player victim, Projectile p)
        {
            killer.Frags++;
            victim.Deaths++;

            Announce(killer.Name + " killed " + victim.Name + " with " + p.GetType().Name);

            if (FragLimit > 0 && killer.Frags >= FragLimit)
            {
                FragLimitHit(killer);
            }
        }

        protected virtual void FragLimitHit(Player winner)
        {
            Announce("Frag limit hit by " + winner.Name + ".");
            EndGame();
        }

        protected virtual void PlayerKill(Player killer, Player victim, Projectile p)
        {
            if (victim != null)
            {
                if (killer == null)
                {
                    Suicide(victim);
                }
                else
                {
                    Frag(killer, victim, p);
                }
            }
        }

        public override void ActorDestroy(Actor killer, Actor victim, Projectile p)
        {
            System.Console.WriteLine("actor destroyed.");
            if (!GameOver)
                PlayerKill(killer != null ? killer.Owner as Player : null, victim.Owner as Player, p);
        }
    }
    public enum Status
    {
        Done, Pending, Failed
    }

    public class Mission : GameRule
    {
        public Mission(DeSerializationContext context)
            :this()
        {
            DeSerialize(context);
        }
        public Mission()
        {
            Teams = new Team[]
            {
                new Team(0,"Red"),
                new Team(1,"Green")
            };
        }
        protected override void TimeLimitHit()
        {
            base.TimeLimitHit();
            for (int i = 0; i < Missions.Length; ++i)
            {
                SingleMission sm =Missions[i];
                foreach (Objective o in sm.PrimaryObjectives)
                {
                    EscortObjective es = o as EscortObjective;
                    if (es != null)
                    {
                        es.NoTreats = true;
                    }
                }

                /*sm.Tick(0.001f);
                if (sm.CurrentStatus == Status.Done)
                {
                    MissionCompleted(i);
                }*/
            }

        }
        protected virtual Team CreateTeam()
        {
            return new Team();
        }
        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);

            Missions = new SingleMission[context.ReadByte()];
            for (int i = 0; i < Missions.Length; ++i)
            {
                Missions[i] = new SingleMission(context);
            }

            int l = context.ReadByte();
            Teams = new Team[l];
            for (int i = 0; i < l; ++i)
            {
                Teams[i] = CreateTeam();
                Teams[i].DeSerialize(context);
                Teams[i].Index = i;
            }

        }
        public override GameRule New()
        {
            return new Mission();
        }
        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            if (!Root.Instance.IsAuthoritive)
                return;

            for(int i=0;i<Missions.Length;++i)
            {
                SingleMission m = Missions[i];
                Status current = m.CurrentStatus;
                m.Tick(dtime,this);
                if (m.CurrentStatus == Status.Done && current!=Status.Done)
                {
                    MissionCompleted(i);
                }
            }
        }
        protected void MissionCompleted(int i)
        {
            Announce("Team " + i.ToString() + " completed its mission.");
            if(!GameOver)
                EndGame();
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);

            context.Write((byte)Missions.Length);
            for (int i = 0; i < Missions.Length; ++i)
            {
                Missions[i].Serialize(context);
            }

            context.Write((byte)Teams.Length);
            for (int i = 0; i < Teams.Length; ++i)
            {
                Teams[i].Serialize(context);
            }

        }
        public SingleMission[] Missions;//array index=team
        public Team[] Teams;
    }

    public class SingleMission : ISerializable
    {
        public SingleMission(DeSerializationContext context)
        {
            DeSerialize(context);
        }
        public SingleMission()
        {
        }
        public SingleMission(Objective[] objectives)
        {
            PrimaryObjectives = objectives;
        }

        public Status CheckStatus(Mission m)
        {
            foreach (Objective o in PrimaryObjectives)
            {
                switch (o.CheckStatus(m))
                {
                    case Status.Failed:
                        return Status.Failed;
                }
            }
            foreach (Objective o in PrimaryObjectives)
            {
                switch (o.CheckStatus(m))
                {
                    case Status.Pending:
                        return Status.Pending;
                }
            }
            return Status.Done;
        }

        public void Tick(float dtime, Mission m)
        {

            foreach (Objective o in PrimaryObjectives)
            {
                o.Tick(dtime,m);
            }
            /*foreach (Objective o in SecondaryObjectives)
            {
                o.Tick(dtime);
            }*/

            Status newstatus = CheckStatus(m);
            if (newstatus != CurrentStatus)
            {
                Cheetah.Console.WriteLine(ToString() + " status changed to " + newstatus + ".");
                CurrentStatus = newstatus;
            }
        }
        public override string ToString()
        {
            string s = "Mission: ";
            foreach (Objective o in PrimaryObjectives)
            {
                s += o.ToString();
            }
            /*s += "Secondary: ";
            foreach (Objective o in SecondaryObjectives)
            {
                s += o.ToString();
            }*/
            return s;
        }

        public void DeSerialize(DeSerializationContext context)
        {
            //base.DeSerialize(context);

            CurrentStatus = (Status)context.ReadByte();

            PrimaryObjectives = new Objective[context.ReadByte()];
            for (int i = 0; i < PrimaryObjectives.Length; ++i)
            {
                PrimaryObjectives[i]=(Objective)context.DeSerialize();
            }
        }

        public void Serialize(SerializationContext context)
        {
            //base.Serialize(context);

            context.Write((byte)CurrentStatus);

            context.Write((byte)PrimaryObjectives.Length);
            for (int i = 0; i < PrimaryObjectives.Length; ++i)
            {
                context.Factory.Serialize(context, PrimaryObjectives[i]);
            }
        }

        public Objective[] PrimaryObjectives;
        //public Objective[] SecondaryObjectives;
        //public Objective[] BonusObjectives;
        public Status CurrentStatus = Status.Pending;
    }


    public abstract class Objective : ISerializable
    {
        /*public enum Type
        {
            Primary, Secondary, Bonus
        }*/
        public Objective(string text)
        {
            Text = text;
        }
        public Objective(DeSerializationContext context)
        {
            DeSerialize(context);
        }
        public virtual Status CheckStatus(Mission m)
        {
            return Status.Pending;
        }

        public bool CheckDependencies()
        {
            if (Dependencies == null)
                return true;
            foreach (Objective o in Dependencies)
            {
                if (o.CurrentStatus != Status.Done)
                    return false;
            }
            return true;
        }

        public virtual void DeSerialize(DeSerializationContext context)
        {
            Text = context.ReadString();
            CurrentStatus = (Status)context.ReadByte();
        }

        public virtual void Serialize(SerializationContext context)
        {
            context.Write(Text);
            context.Write((byte)CurrentStatus);
        }

        public virtual void Tick(float dtime, Mission m)
        {
            Status newstatus = CheckStatus(m);
            if (newstatus != CurrentStatus && CheckDependencies())
            {
                Cheetah.Console.WriteLine(ToString() + " status changed to " + newstatus.ToString() + ".");
                CurrentStatus = newstatus;
            }
        }

        public override string ToString()
        {
            return Text;
        }
        public Status CurrentStatus = Status.Pending;
        public string Text;
        public Objective[] Dependencies;
    }

    public class DestroyObjective : Objective
    {
        public DestroyObjective(string text,Actor[] targets)
            :base(text)
        {
            Targets = targets;
        }

        public DestroyObjective(DeSerializationContext context)
            :base(context)
        {
        }

        public override Status CheckStatus(Mission m)
        {
            if (Root.Instance.IsAuthoritive)
            {
                foreach (Actor a in Targets)
                    if (!a.Kill)
                        return m.GameOver?Status.Failed:Status.Pending;
                return Status.Done;
            }
            else
                return CurrentStatus;
        }
        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);
        }

        public Actor[] Targets;
    }

    public class CaptureObjective : Objective
    {
        public CaptureObjective(string text, Actor[] targets)
            : base(text)
        {
            Targets = targets;
        }

        public CaptureObjective(DeSerializationContext context)
            : base(context)
        {
        }

        public override Status CheckStatus(Mission m)
        {
            foreach (Actor a in Targets)
            {
                if (a.Kill)
                    return Status.Failed;
            }
            foreach (SpaceShip a in Targets)
            {
                if (!a.ControlsJamed)
                    return m.GameOver?Status.Failed:Status.Pending;
            }

            return Status.Done;
        }
        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);
        }

        public Actor[] Targets;
    }


    public class RendevouzObjective : Objective
    {
        public RendevouzObjective(string text, Actor[] targets)
            : base(text)
        {
            Targets = targets;
        }

        public override Status CheckStatus(Mission m)
        {
            if (Root.Instance.IsAuthoritive)
            {
                foreach (Actor a in Targets)
                    if (a.Kill)
                        return Status.Failed;

                if (Targets[0].Distance(Targets[1]) < 1000)
                    return Status.Done;

                return Status.Pending;
            }
            else
                return CurrentStatus;
        }
        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);
        }

        public Actor[] Targets;
    }

    public class EscortObjective : Objective
    {
        public EscortObjective(string text,Actor[] targets)
            :base(text)
        {
            Targets = targets;
        }
        public EscortObjective(DeSerializationContext context)
            : base(context)
        {
        }
        public override Status CheckStatus(Mission m)
        {
            if (Root.Instance.IsAuthoritive)
            {
                foreach (Actor a in Targets)
                    if (a.Kill)
                        return Status.Failed;
                if (NoTreats)
                    return Status.Done;
                return m.GameOver?Status.Done:Status.Pending;
            }
            else
                return CurrentStatus;
        }
        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);
        }

        public Actor[] Targets;
        public bool NoTreats = false;
    }

    public class Race : GameRule
    {
        public Race(DeSerializationContext context)
            : base(context)
        {
            /*IList<CheckPoint> l = Root.Instance.Scene.FindEntitiesByType<CheckPoint>();
            CheckPoints = new CheckPoint[l.Count];
            l.CopyTo(CheckPoints, 0);*/
        }
        public Race(int laps)
        {
            /*IList<CheckPoint> l = Root.Instance.Scene.FindEntitiesByType<CheckPoint>();
            CheckPoints = new CheckPoint[l.Count];
            l.CopyTo(CheckPoints, 0);*/
            Laps = laps;
        }
        public override GameRule New()
        {
            return new Race(Laps);
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);

            Laps = (int)context.ReadByte();
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);
            context.Write((byte)Laps);
        }

        public int Laps;

        public int Lap(int checks)
        {
            return Math.Max(0, checks / CheckPoints.Length);
        }
        public int CheckPointIndex(int checks)
        {
            return checks % CheckPoints.Length;
        }
        public CheckPoint CheckPoint(int checks)
        {
            return CheckPoints[CheckPointIndex(checks)];
        }
        public void Reach(RacePlayer player, CheckPoint check)
        {
            if (check.CheckPointIndex == CheckPointIndex(player.Checks + 1))
            {
                //richtiger checkpoint, ganz klasse
                player.Checks++;
                Announce(player.Name + " reached " + player.Checks + ".");

                if (Lap(player.Checks) >= Laps && Laps>0)
                {
                    Finish(player);
                }
            }
        }
        public CheckPoint GetNextCheckPoint(RacePlayer player)
        {
            CheckPoint[] list = CheckPoints;
            foreach (CheckPoint p in list)
            {
                if (p.CheckPointIndex == CheckPointIndex(player.Checks + 1))
                {
                    return p;
                }
            }
            return null;
        }

        protected void Finish(RacePlayer player)
        {
            Announce(player.Name + " finished.");
            EndGame();
        }

        public override string ToString()
        {
            return "Race";
        }

        public override Player CreatePlayer(short clientid, string name)
        {
            return new RacePlayer(clientid, name);
        }

        public CheckPoint[] CheckPoints
        {
            get
            {
                IList<CheckPoint> l = Root.Instance.Scene.FindEntitiesByType<CheckPoint>();
                CheckPoint[] cp = new CheckPoint[l.Count];
                l.CopyTo(cp, 0);
                return cp;
            }
        }
    }

    public class Domination : GameRule
    {
        public Domination()
        {
            Teams = new Team[] { new Team(0, "t1"), new Team(1, "t2") };
        }
        public Domination(Team[] teams, int scoreintervall, int scorelimit, float timelimit)
        {
            ScoreIntervall = scoreintervall;
            TeamScoreLimit = scorelimit;
            Teams = teams;
            TimeLimit = timelimit;
        }
        public Domination(int scoreintervall, int scorelimit)
            : this()
        {
            ScoreIntervall = scoreintervall;
            TeamScoreLimit = scorelimit;

        }
        public override GameRule New()
        {
            return new Domination(ScoreIntervall,TeamScoreLimit);
        }

        public Domination(DeSerializationContext context)
            : this()
        {
            DeSerialize(context);
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);

            ScoreIntervall = (int)context.ReadByte();
            TeamScoreLimit = (int)context.ReadInt16();
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);
            context.Write((byte)ScoreIntervall);
            context.Write((short)TeamScoreLimit);
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            t += dtime;
            while (t >= 1)
            {
                seconds++;
                t -= 1;

                Player p1 = Root.Instance.Scene.FindEntityByType<Player>();
                if (p1 != null)
                    p1.Team = 1;

                if (seconds % ScoreIntervall == 0)
                {
                    IList<DominationPoint> points = Root.Instance.Scene.FindEntitiesByType<DominationPoint>();
                    foreach (DominationPoint p in points)
                    {
                        if (p.Team >= 0)
                        {
                            Teams[p.Team].Score++;
                        }
                    }
                }
            }

        }

        public Team[] Teams;
        public int ScoreIntervall;
        public int TeamScoreLimit;
        int seconds;
        float t = 0;
    }

    public class KingOfTheHill : DeathMatch
    {

        public KingOfTheHill(DeSerializationContext context)
            : base(context)
        {
        }
        public KingOfTheHill()
        {
        }
        public KingOfTheHill(int fraglimit, int timelimit)
            :base(fraglimit,timelimit)
        {
        }

        public override GameRule New()
        {
            return new KingOfTheHill();
        }

        protected override void Frag(Player killer, Player victim, Projectile p)
        {
            if (killer != King && victim != King)
            {
                killer.Frags--;
            }

            base.Frag(killer, victim, p);

            if (victim == King)
            {
                ChangeKing(killer);
            }
        }

        protected void ChangeKing(Player king)
        {
            King = king;
            Announce(King.Name + " is the new king of the hill.");
        }

        public override void Tick(float dtime)
        {
            base.Tick(dtime);

            if (King == null || King.Kill)
            {
                Player p = Root.Instance.Scene.FindEntityByType<Player>();
                if (p != null)
                {
                    ChangeKing(p);
                }
            }
        }

        public Player King;
    }

    public class TeamDeathMatch : DeathMatch
    {
        public TeamDeathMatch(Team[] teams, int teamscorelimit, float timelimit)
            : base(0, timelimit)
        {
            Teams = teams;
            TeamScoreLimit = teamscorelimit;
        }

        public TeamDeathMatch(DeSerializationContext context)
            : base(context)
        {
        }

        public void SetPlayerTeam(string playername, int team)
        {
            IList<Player> players = Root.Instance.Scene.FindEntitiesByType<Player>();
            foreach (Player p in players)
            {
                if (p.Name == playername)
                {
                    p.Team = team;
                    break;
                }
            }
        }

        protected virtual Team CreateTeam()
        {
            return new Team();
        }

        public override void DeSerialize(DeSerializationContext context)
        {
            base.DeSerialize(context);

            TeamScoreLimit = context.ReadInt16();

            int l = context.ReadByte();
            Teams = new Team[l];
            for (int i = 0; i < l; ++i)
            {
                Teams[i] = CreateTeam();
                Teams[i].DeSerialize(context);
                Teams[i].Index = i;
                //Teams[i].Score = context.ReadInt16();
            }
        }

        public override void Serialize(SerializationContext context)
        {
            base.Serialize(context);

            context.Write((short)TeamScoreLimit);

            context.Write((byte)Teams.Length);
            for (int i = 0; i < Teams.Length; ++i)
            {
                //context.Write(Teams[i].Name);
                //context.Write((short)Teams[i].Score);
                Teams[i].Serialize(context);
            }
        }

        protected override void Suicide(Player b00n)
        {
            base.Suicide(b00n);
            if (b00n.Team >= 0)
                Teams[b00n.Team].Score--;
        }

        protected void TeamKill(Player killer, Player victim, Projectile p)
        {
            Announce(killer.Name + " teamkilled " + victim.Name + " with " + p.GetType().Name);
            Teams[killer.Team].Score--;
        }

        protected override void Frag(Player killer, Player victim, Projectile p)
        {
            if (killer.Team >= 0)
            {
                if (killer.Team != victim.Team)
                {
                    base.Frag(killer, victim, p);
                    Teams[killer.Team].Score++;

                    if (TeamScoreLimit >= 0 && Teams[killer.Team].Score >= TeamScoreLimit)
                    {
                        TeamScoreLimitHit(killer);
                    }
                }
                else
                {
                    TeamKill(killer, victim, p);
                }
            }
            else
                base.Frag(killer, victim, p);
        }

        protected virtual void TeamScoreLimitHit(Player winner)
        {
            EndGame();
        }
        public override GameRule New()
        {
            Team[] newteams = new Team[Teams.Length];
            for (int i = 0; i < newteams.Length; ++i)
                newteams[i] = new Team(Teams[i].Index, Teams[i].Name);

            return new TeamDeathMatch(newteams,TeamScoreLimit,TimeLimit);
        }
        public override string ToString()
        {
            string time = ", Time: " + ((int)TimeElapsed).ToString();
            string frags = "";
            if (TeamScoreLimit > 0)
                frags = ", TeamScoreLimit: " + TeamScoreLimit.ToString();
            if (TimeLimit > 0)
                time += "/" + (int)TimeLimit;

            return "TeamDeathMatch" + frags + time;
        }
        public Team[] Teams;
        public int TeamScoreLimit;
    }

    public abstract class RamboMatch : KingOfTheHill
    {
    }
}
