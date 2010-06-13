check:
	mdtool --help
	zip -v
	fakeroot -v
	dpkg-deb --version
	hg --version
	#awk --version

compile: check
	mdtool build -c:Release source/Spacewar2006/Spacewar2006.sln

deb: compile
	rm -f spacewar2006-1.1-2.deb
	rm -rf debian/opt/spacewar2006/*
	rm -rf debian/opt
	mkdir debian/opt
	mkdir debian/opt/spacewar2006
	cp -r dist/* debian/opt/spacewar2006
	cp source/Spacewar2006/bin/Release/* debian/opt/spacewar2006/bin/

	fakeroot dpkg-deb --build debian spacewar2006-1.1-2.deb

	rm -rf debian/opt/spacewar2006/*

zip: compile
	rm -rf release
	rm -rf spacewar-arena
	mkdir release
	mkdir spacewar-arena
	cp -r dist/* spacewar-arena
	cp source/Spacewar2006/bin/Release/* spacewar-arena/bin/
	zip -r9 release/spacewar-arena-`date +%Y%m%d`-`hg identify|awk '{print $1}'`.zip spacewar-arena
	rm -rf spacewar-arena

upload:
	#scp spacewar-arena-20100613-c452d2f7d430+.zip cody82,spacewar2006@frs.sourceforge.net:/home/frs/project/s/sp/spacewar2006/
	rsync -e ssh -rv --delete release/* cody82,spacewar2006@frs.sourceforge.net:/home/frs/project/s/sp/spacewar2006/