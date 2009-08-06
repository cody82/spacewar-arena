flip=true

struct vertex (position, texture, normal)

fn convert_point3 v =
(
	if flip then
		(return [-v.x,v.z,v.y])
	else
		(return v)
)

fn convert_quat q=
(
	if flip then
		(return (quat -q.x q.z q.y q.w))
	else
		(return v)
)

fn convert_tex v=
(
	return [v.x,1-v.y,v.z]
)

fn export_camera cam outfile=
(
)

fn export_light light outfile=
(
)

fn export_material mat outfile=
(
	format "MATERIALTEXT\r\n" to:outfile
	format "diffuse: [%,%,%]\r\n" mat.diffuse.red mat.diffuse.green mat.diffuse.blue to:outfile
	format "specular: [%,%,%]\r\n" mat.specular.red mat.specular.green mat.specular.blue to:outfile
	format "ambient: [%,%,%]\r\n" mat.ambient.red mat.ambient.green mat.ambient.blue to:outfile
	format "glossiness: %\r\n" mat.glossiness to:outfile
	
	if mat.reflectionmap!=undefined do
		(format "environmentmap: %\r\n" (getFilenameFile mat.reflectionmap.filename) to:outfile)
	if mat.diffusemap!=undefined do
		(format "diffusemap: %\r\n" (getFilenameFile mat.diffusemap.filename) to:outfile)


	
)

fn export_submesh mesh outfile=
(
	format "SUBMESHTEXT\r\n" to:outfile
	
	vertices=#()
	--faces=#()
	
	for f=1 to mesh.numfaces do
	(
		face=getFace mesh f
		tface=getTVFace mesh f
		
		pos1=convert_point3 (getVert mesh face.x)
		pos2=convert_point3 (getVert mesh face.y)
		pos3=convert_point3 (getVert mesh face.z)
		
		tex1=convert_tex (getTVert mesh tface.x)
		tex2=convert_tex (getTVert mesh tface.y)
		tex3=convert_tex (getTVert mesh tface.z)
		
		normal1=convert_point3 (getNormal mesh face.x)
		normal2=convert_point3 (getNormal mesh face.y)
		normal3=convert_point3 (getNormal mesh face.z)
		
		vertices[face.x]=vertex pos1 tex1 normal1
		vertices[face.y]=vertex pos2 tex2 normal2
		vertices[face.z]=vertex pos3 tex3 normal3
		
	)
	
	format "vertex count: %\r\n" vertices.count to:outfile
	
	for v=1 to vertices.count do
	(
		format "% % %\r\n" vertices[v].position vertices[v].normal vertices[v].texture to:outfile
	)
	
	for f=1 to mesh.numfaces do
	(
		format "%\r\n" ((getFace mesh f)-[1,1,1]) to:outfile
	)
)

out_name = GetSaveFileName()
out_file1= createfile out_name
out_path=(getFilenamePath out_name)+"\\"

format "MESHTEXT\r\n" to: out_file1

for i=1 to selection.count do
(
	if (iskindof selection[i] geometryclass) do
	(
		tmesh= snapshotasmesh selection[i]
		mat=selection[i].material
		material_filename=mat.name+".material"
		submesh_filename=selection[i].name+".submesh"
		
		format "material: %\r\n" material_filename to: out_file1
		format "submesh: %\r\n" submesh_filename to: out_file1
		
		material_file=createfile (out_path+material_filename)
		submesh_file=createfile (out_path+submesh_filename)
		
		export_material mat material_file
		export_submesh tmesh submesh_file
		
		close material_file
		close submesh_file
	)
)

close out_file1
