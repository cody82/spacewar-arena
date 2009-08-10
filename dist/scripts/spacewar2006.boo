def view(mesh):
	(Cheetah.Root.Instance.CurrentFlow as SpaceWar2006.Flows.Viewer).ChangeMesh(mesh)
def anim(n):
	(Cheetah.Root.Instance.CurrentFlow as SpaceWar2006.Flows.Viewer).ChangeAnim(n)

def nextgame(rule,map,bots):
	server=Cheetah.Root.Instance.CurrentFlow as SpaceWar2006.Flows.GameServer
	server.NextBotCount=bots
	server.NextMap=map as SpaceWar2006.GameObjects.Map
	server.NextRule=rule as SpaceWar2006.Rules.GameRule

def restart():
	server=Cheetah.Root.Instance.CurrentFlow as SpaceWar2006.Flows.GameServer
	server.Restart()

def test():
	nextgame(SpaceWar2006.Rules.DeathMatch(10,600),SpaceWar2006.Maps.TestSector(),2)
	Cheetah.Console.WriteLine("new rules set.")

Cheetah.Console.WriteLine("welcome to SpaceWar2006[tm]")