load_assembly("Mscorlib")
load_assembly("Cheetah")
--print("bla")
--Access=import_type("Cheetah.Access")
--a=Access()
--Root=a:GetRoot()
Root=import_type("Cheetah.Root")
Instance=Root.Instance
Console=import_type("Cheetah.Console")
--Instance:Loop()
r=Instance

--Console.WriteLine(tostring(Instance.Fps))
--print(tostring(Instance.Fps))

function password(pw)
r.Password=pw
end

function print(text)
	Console:WriteLine(text)
end

function print_fps()
	print(tostring(fps()))
end

function fps()
	return Instance.Fps
end

function print_cam_position()
	local cam=Instance.Scene.camera
	local p=cam.Position
	print(p:ToString())
end

function start_record()
	r.LockTimeDelta=0.04
	r.RecordVideo=1
end

function stop_record()
	r.LockTimeDelta=0.04
	r.RecordVideo=-1
end

function print_list()
	print(r.Scene:ToString())
end

function print_count()
	print(r.Scene.ServerList.Count)
end

function lsres()
	r.ResourceManager:PrintAllNames()
end

function lstex()
print(r.UserInterface.Renderer.Textures.Count)
end

function lsbuf()
print(r.UserInterface.Renderer.Buffers.Count)
end