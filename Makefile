compile:
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
	rm -f spacewar-arena.zip
	rm -rf spacewar-arena
	mkdir spacewar-arena
	cp -r dist/* spacewar-arena
	cp source/Spacewar2006/bin/Release/* spacewar-arena/bin/
	zip -r9 spacewar-arena-`date +%Y%m%d`.zip spacewar-arena
	rm -rf spacewar-arena
