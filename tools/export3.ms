struct vertex(pos,normal,tex,tangent,binormal)
struct meshmaterial(meshname,materialname)

--struct material(diffuse,ambient,specular,glossiness)

tangents=true

fn calc_tangents vertices numvertices triangles numtriangles=
(
	for t = 1 to numtriangles do
	(
		triangle=triangles[t]
		i1=triangle.x
		i2=triangle.y
		i3=triangle.z
		
		v1=vertices[i1].pos
		v2=vertices[i2].pos
		v3=vertices[i3].pos
		
		w1=vertices[i1].tex
		w2=vertices[i2].tex
		w3=vertices[i3].tex
		
		x1=v2.x-v1.x
		x2=v3.x-v1.x
		y1=v2.y-v1.y
		y2=v3.y-v1.y
		z1=v2.z-v1.z
		z2=v3.z-v1.z
	
		s1=w2.x-w1.x
		s2=w3.x-w1.x
		t1=w2.y-w1.y
		t2=w3.y-w1.y
		
		r=1/(s1*t2-s2*t1)
		
		sdir=[(t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r,(t2 * z1 - t1 * z2) * r]
		tdir=[(s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r,(s1 * z2 - s2 * z1) * r]
		
		vertices[i1].binormal=vertices[i1].binormal+sdir
		vertices[i2].binormal=vertices[i2].binormal+sdir
		vertices[i3].binormal=vertices[i3].binormal+sdir
		
		vertices[i1].tangent=vertices[i1].tangent+tdir
		vertices[i2].tangent=vertices[i2].tangent+tdir
		vertices[i3].tangent=vertices[i3].tangent+tdir
	)
	
	for v = 1 to numvertices do
	(
		n=vertices[v].normal
		t=vertices[v].binormal
		t2=vertices[v].tangent
		
		vertices[v].binormal=normalize (t-n*(dot n t))
		vertices[v].tangent=normalize (t2-n*(dot n t2))

	)
	
)

fn write_vertex v file=
(
format "%,%,%\r\n" v.pos v.normal v.tex to:file
)

fn compare_vertex v1 v2 =
(
v1.pos==v2.pos and v1.tex==v2.tex and v1.normal==v2.normal
)

fn get_index vtx=
(
for i=1 to vertices.count do
(
            if (compare_vertex vtx (vertices[i])) do
                    (return i)
    )
    return -1
)

flip=true

fn convert_point3 v=
(
    if flip then (return [v.x,v.z,-v.y])
    else (return v)
)

fn convert_quat q=
(
    aa=q as angleaxis
    aa.angle=-aa.angle
    q=aa as quat
    if flip then (return quat q.x q.z -q.y q.w)
    else (return q)
)

fn export_node node out_file=
(
    format "NODETEXT0001\r\n" to:out_file
    format "position: %\r\n" (convert_point3 node.position) to:out_file

    q=convert_quat node.rotation
    format "rotation: [%,%,%,%]\r\n" q.x q.y q.z q.w to:out_file
)

fn export_light light out_file=
(
    format "LIGHTTEXT\r\n" to:out_file
    format "position: %\r\n" (convert_point3 light.position) to:out_file
    format "diffuse: [%,%,%]\r\n" (light.rgb.red/255) (light.rgb.green/255) (light.rgb.blue/255) to:out_file
    format "ambient: [%,%,%]\r\n" 0 0 0 to:out_file
    format "specular: [%,%,%]\r\n" (light.rgb.red/255) (light.rgb.green/255) (light.rgb.blue/255) to:out_file

)

fn export_camera cam out_file=
(
    format "CAMERATEXT\r\n" to:out_file
    format "position: %\r\n" (convert_point3 cam.position) to:out_file

    --aa=cam.rotation as angleaxis
    --aa.angle=-aa.angle
    --q=((angleaxis 180 [0,1,0]) as quat)*cam.rotation
    in coordsys local
    (rotate cam (eulerangles 0 180 0))
    q=convert_quat cam.rotation
    in coordsys local
    (rotate cam (eulerangles 0 180 0))
    --q=((angleaxis 180 [0,1,0]) as quat)*q
    --format "rotation: [%,%,%,%]\r\n" q.x q.y q.z q.w to:out_file
    --format "rotation: [%,%,%,%]\r\n" (aa.axis.y) (aa.axis.z) (aa.axis.x) (aa.angle) to:out_file
    --format "target: %\r\n" (cam.target.position.y) (cam.target.position.z) (cam.target.position.x) to:out_file

)

fn export_material mat out_file=
(
    --matstruct = material mat.diffuse mat.ambient mat.specular mat.glossiness
    format "MATERIALTEXT\r\n" to:out_file
    if classof mat == standardMaterial do
    (
    format "diffuse: [%,%,%]\r\n" (mat.diffuse.red/255) (mat.diffuse.green/255) (mat.diffuse.blue/255) to:out_file
    format "ambient: [%,%,%]\r\n" (mat.ambient.red/255) (mat.ambient.green/255) (mat.ambient.blue/255) to:out_file
    format "specular: [%,%,%]\r\n" (mat.specular.red/255) (mat.specular.green/255) (mat.specular.blue/255) to:out_file
    format "glossiness: %\r\n" (mat.glossiness as float) to:out_file
    if mat.diffuseMap != undefined do
            (format "diffusemap: %\r\n" (filenameFromPath mat.diffuseMap.filename) to:out_file)

    if mat.reflectionMap != undefined do
            (format "environmentmap: %\r\n" (filenameFromPath mat.reflectionMap.filename) to:out_file)
    if mat.bumpMap != undefined do
            (format "bumpmap: %\r\n" (filenameFromPath mat.bumpMap.filename) to:out_file)


    )
)

fn export_mesh tmesh mat out_file=
(

numfaces=(getNumFaces tmesh)
vertices=#()
indices=#()

for f = 1 to numfaces do
(

face=getFace tmesh f
pos1 = getVert tmesh (face.x)
pos2 = getVert tmesh (face.y)
pos3 = getVert tmesh (face.z)

tex1=[0,0,0]
tex2=[0,0,0]
tex3=[0,0,0]

if getNumTVerts tmesh > 0 do
(
tface=getTVFace tmesh f

tex1 = getTVert tmesh (tface.x)
tex2 = getTVert tmesh (tface.y)
tex3 = getTVert tmesh (tface.z)

tex1=[tex1.x,1-tex1.y,tex1.z]
tex2=[tex2.x,1-tex2.y,tex2.z]
tex3=[tex3.x,1-tex3.y,tex3.z]
)


--normals = meshop.getFaceRNormals tmesh f
normal1 = getNormal tmesh (face.x)
normal2 = getNormal tmesh (face.y)
normal3 = getNormal tmesh (face.z)
--normal1 = normals[1]
--normal2 = normals[2]
--normal3 = normals[3]

pos1=convert_point3 pos1
pos2=convert_point3 pos2
pos3=convert_point3 pos3

normal1=convert_point3 normal1
normal2=convert_point3 normal2
normal3=convert_point3 normal3

--if flip do
--(
--pos1=[pos1.y,pos1.z,pos1.x]
--pos2=[pos2.y,pos2.z,pos2.x]
--pos3=[pos3.y,pos3.z,pos3.x]
--normal1=[normal1.y,normal1.z,normal1.x]
--normal2=[normal2.y,normal2.z,normal2.x]
--normal3=[normal3.y,normal3.z,normal3.x]
--)

normal1=normalize normal1
normal2=normalize normal2
normal3=normalize normal3

vtx1 = vertex pos1 normal1 tex1 [0,0,0] [0,0,0]
vtx2 = vertex pos2 normal2 tex2 [0,0,0] [0,0,0]
vtx3 = vertex pos3 normal3 tex3 [0,0,0] [0,0,0]

append indices face
vertices[face.x]= vtx1
vertices[face.y]= vtx2
vertices[face.z]= vtx3
)

if tangents then
(
	calc_tangents vertices vertices.count indices numfaces
)

format "SUBMESHTEXT0001\r\n" to:out_file
--format "material: %\r\n" mat.name to:out_file
if tangents then
(format "vertex format: position:3,normal:3,texture0:3,binormal:3,tangent:3\r\n" to:out_file)
else
(format "vertex format: position:3,normal:3,texture0:3\r\n" to:out_file)

format "vertex count: %\r\n" (getNumVerts tmesh) to:out_file

vtx=vertex [0,0,0] [0,0,0] [0,0,0] [0,0,0] [0,0,0]

nonref=0
for i = 1 to vertices.count do
(
	if vertices[i]==undefined then
	(
		if tangents then
		(
			format "% % % % %\r\n" vtx.pos vtx.normal vtx.tex vtx.binormal vtx.tangent to:out_file;nonref=nonref+1
		)
		else
		(
			format "% % %\r\n" vtx.pos vtx.normal vtx.tex to:out_file;nonref=nonref+1
		)
	)
	else
	(
		if tangents then
			(format "% % % % %\r\n" vertices[i].pos vertices[i].normal vertices[i].tex vertices[i].binormal vertices[i].tangent to:out_file)
		else
			(format "% % %\r\n" vertices[i].pos vertices[i].normal vertices[i].tex to:out_file)
	)
)
format "#nonreferenced vertices: %\r\n" nonref to:out_file
format "triangle count: %\r\n" indices.count to:out_file
for i = 1 to indices.count do
(
format "%\r\n" (indices[i]-[1,1,1]) to:out_file
)

format "\r\n" to:out_file


)

out_name = GetSaveFileName()
out_file1= createfile out_name
out_path=(getFilenamePath out_name)+"\\"

format "MESHTEXT\r\n" to: out_file1

currentmaterial=false

meshmaterials=#()

for i=1 to selection.count do
(
    if classof selection[i] == Omnilight then
    (
            out_file = createfile (out_path+selection[i].name+".light")
            export_light selection[i] out_file
            close out_file
    )
    else
    (if classof selection[i] == FreeCamera then
    (
            out_file = createfile (out_path+selection[i].name+".camera")
            export_camera selection[i] out_file
            close out_file
    )
    else
    (
    if isKindOf selection[i] GeometryClass then
    (
    --tmesh = snapshotAsMesh selection[i]
    --in coordsys local
    (
    c=selection[i]
    tmesh=copy c
    convertToMesh tmesh
    mat = c.material
    out_name=""
    for j=1 to c.name.count do
    (
            if c.name[j]!="/" then
                    (out_name=out_name+c.name[j])
            else
                    (out_name=out_name+"_")
    )
	
    out_file = createfile (out_path+mat.name+".material")
    export_material mat out_file
    close out_file
	
	if currentmaterial!=mat do
	(
	--format "material: %\r\n" (mat.name+".material") to: out_file1
	currentmaterial=mat
	)
	
    out_file = createfile (out_path+out_name+".submesh")
    export_mesh tmesh mat out_file
    close out_file

	--format "submesh: %\r\n" (out_name+".submesh") to: out_file1

	meshmaterials=append meshmaterials (meshmaterial (out_name+".submesh") (mat.name+".material"))

    delete tmesh
    in coordsys world
    (
    out_file = createfile (out_name+".node")
    export_node c out_file
    close out_file
    )

    )
    )
    ))
)

fn compareFN v1 v2 =
(
case of
(
(v1.materialname<v2.materialname): -1
(v1.materialname>v2.materialname): 1
default: 0
)
)

qsort meshmaterials compareFN

currentmaterial=false
for m=1 to meshmaterials.count do
(
	local mm=meshmaterials[m]
	if mm.materialname!=currentmaterial do
	(
		format "material: %\r\n" (mm.materialname) to: out_file1
		currentmaterial=mm.materialname
	)
	format "submesh: %\r\n" (mm.meshname) to: out_file1
)

close out_file1
--i=get_index vtx1
--index1=-1
--if i==-1 then (append vertices vtx1;index1=vertices.count)
--else (index1=i)

--i=get_index vtx2
--index2=-1
--if i==-1 then (append vertices vtx2;index2=vertices.count)
--else (index2=i)

--i=get_index vtx3
--index3=-1
--if i==-1 then (append vertices vtx3;index3=vertices.count)
--else (index3=i)

--append indices [index1,index2,index3]



--close out_file

--edit out_name

