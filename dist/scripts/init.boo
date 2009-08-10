import Cheetah

def startrecord():
	Root.Instance.LockTimeDelta=0.04
	Root.Instance.ClientRecordVideo=1

def stoprecord():
	Root.Instance.LockTimeDelta=-1
	Root.Instance.ClientRecordVideo=-1

def videorec():
	if Root.Instance.ClientRecordVideo >= 1:
		stoprecord()
	else:
		startrecord()

def pp_wobble():
	Root.Instance.ClientPostProcessor.Passes.Add(Ripple())

def pp_shift():
	Root.Instance.ClientPostProcessor.Passes.Add(Shift())

def pp_smooth():
	Root.Instance.ClientPostProcessor.Passes.Add(Smooth())


def pp_clear():
	Root.Instance.ClientPostProcessor.Passes.Clear()



def lsres():
	Root.Instance.ResourceManager.PrintAllNames()

def spawn(classname):
	e=Root.Instance.Factory.CreateInstance(classname as string) as Node
	Root.Instance.Scene.Spawn(e)

def cmd(command):
	c=Command(command as string)
	(Root.Instance.Connection as UdpClient).Send(c)

def demorec(name):
	Root.Instance.Recorder=DemoRecorder("demos/"+(name as string)+".demo",true,20);

def demostop():
	Root.Instance.Recorder=null;

def fov(deg):
	Root.Instance.Scene.camera.Fov=deg

Console.WriteLine("version "+Handshake.ThisVersion.ToString())
