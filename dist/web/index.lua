function echo(string)
	web.writer:Write(string)
end

echo("<html><title>cheetah webadmin[tm]</title><body>")
echo("<form>")

table.foreach(web.get,function(k,v) echo(k);echo("=");echo(v);echo("<br>"); end)
echo("<br>")

if web.get.exit then
	r:Stop()
end

echo("enter text:")
echo("<input type=text name=test1>")
echo("<input type=text name=test2>")
echo("<br>")
echo("<input type=submit>")
echo("<input type=submit name=exit value=exit>")

echo("</form>")
echo("</body></html>")
