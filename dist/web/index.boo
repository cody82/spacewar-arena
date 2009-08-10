import SpaceWar2006
import Cheetah

def echo(t):
	web_writer.Write(t)

def gamestat():
	gs=Root.Instance.Scene.FindEntitiesByType(typeof(GameObjects.Player))
	list=[["name","frags","deaths"]]
	for player in gs:
		list.Add([player.Name,player.Frags,player.Deaths])
	return list

def gameinfo():
	table={}
	table["maxclients"]=Root.Instance.ServerConnection.ConnectedClients
	table["numclients"]=Root.Instance.ServerConnection.MaxClients
	return table

def printhash(hash):
	echo("<table class=\"main\">")
	for item in hash:
		echo("<tr><td class=\"main\">")
		echo(item.Key)
		echo("</td><td class=\"main\">")
		echo(item.Value)
		echo("</td></tr>")
	echo("</table>")

def printlist(list):
	echo("<table class=\"main\">")
	for row in list:
		echo("<tr>")
		for cell in row:
			echo("<td class=\"main\">")
			echo(cell.ToString())
			echo("</td>")
		echo("</tr>")
	echo("</table>")


echo("<html><title>cheetah webadmin[tm]</title>")
echo("<head><link rel=\"stylesheet\" type=\"text/css\" href=\"style.css\"></head>")
echo("<body class=\"main\">")
echo("<form>")

//if web_get.ContainsKey("exit"):
//	Cheetah.Root.Instance.Quit=true

echo("<tt>")
s1=Root.Instance.Scene.ToString()
s1=s1.Replace("\n","<br>")
echo(s1)
echo("<br>")
echo("</tt>")

printlist(gamestat())

printhash(gameinfo())

if web_get.ContainsKey("cmd"):
	Root.Instance.Script.Execute(web_get["commandline"])


echo("console:")
echo("<input type=text name=commandline>")
echo("<input type=submit name=cmd value=cmd>")

echo("</form>")
echo("</body></html>")
