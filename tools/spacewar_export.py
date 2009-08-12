#!BPY

"""
Name: 'Spacewar mesh (*.submesh)...'
Blender: 249
Group: 'Export'
Tooltip: 'Export active object to Spacewar2006 format'
"""

import bpy
import Blender
from Blender import Mesh, Scene, Window, sys, Image, Draw
import BPyMesh

__author__ = "cody"
__version__ = "0.01"
__bpydoc__ = """\
This script exports Spacewar files from Blender.
"""

def rvec3d(v):	return round(v[0], 6), round(v[1], 6), round(v[2], 6)
def rvec2d(v):	return round(v[0], 6), round(v[1], 6)

def file_callback(filename):
	
	if not filename.lower().endswith('.submesh'):
		filename += '.submesh'
	
	scn= bpy.data.scenes.active
	ob= scn.objects.active
	if not ob:
		Blender.Draw.PupMenu('Error%t|Select 1 active object')
		return
	
	file = open(filename, 'wb')
	
	EXPORT_APPLY_MODIFIERS = Draw.Create(1)
	EXPORT_NORMALS = Draw.Create(1)
	EXPORT_UV = Draw.Create(1)
	EXPORT_COLORS = Draw.Create(1)
	#EXPORT_EDGES = Draw.Create(0)
	
	pup_block = [\
	('Apply Modifiers', EXPORT_APPLY_MODIFIERS, 'Use transformed mesh data.'),\
	('Normals', EXPORT_NORMALS, 'Export vertex normal data.'),\
	('UVs', EXPORT_UV, 'Export texface UV coords.'),\
	('Colors', EXPORT_COLORS, 'Export vertex Colors.'),\
	#('Edges', EXPORT_EDGES, 'Edges not connected to faces.'),\
	]
	
	if not Draw.PupBlock('Export...', pup_block):
		return
	
	is_editmode = Blender.Window.EditMode()
	if is_editmode:
		Blender.Window.EditMode(0, '', 0)
	
	Window.WaitCursor(1)
	
	EXPORT_APPLY_MODIFIERS = EXPORT_APPLY_MODIFIERS.val
	EXPORT_NORMALS = EXPORT_NORMALS.val
	EXPORT_UV = EXPORT_UV.val
	EXPORT_COLORS = EXPORT_COLORS.val
	#EXPORT_EDGES = EXPORT_EDGES.val
	
	mesh = BPyMesh.getMeshFromObject(ob, None, EXPORT_APPLY_MODIFIERS, False, scn)
	
	if not mesh:
		Blender.Draw.PupMenu('Error%t|Could not get mesh data from active object')
		return
	
	mesh.transform(ob.matrixWorld)
	
	faceUV = mesh.faceUV
	vertexUV = mesh.vertexUV
	vertexColors = mesh.vertexColors
	
	if (not faceUV) and (not vertexUV):		EXPORT_UV = False
	if not vertexColors:					EXPORT_COLORS = False
	
	if not EXPORT_UV:						faceUV = vertexUV = False
	if not EXPORT_COLORS:					vertexColors = False
	
	# incase
	color = uvcoord = uvcoord_key = normal = normal_key = None
	
	verts = [] # list of dictionaries
	# vdict = {} # (index, normal, uv) -> new index
	vdict = [{} for i in xrange(len(mesh.verts))]
	vert_count = 0
	for i, f in enumerate(mesh.faces):
		smooth = f.smooth
		if not smooth:
			normal = tuple(f.no)
			normal_key = rvec3d(normal)
			
		if faceUV:			uv = f.uv
		if vertexColors:	col = f.col
		for j, v in enumerate(f):
			if smooth:
				normal=		tuple(v.no)
				normal_key = rvec3d(normal)
			
			if faceUV:
				uvcoord=	uv[j][0], 1.0-uv[j][1]
				uvcoord_key = rvec2d(uvcoord)
			elif vertexUV:
				uvcoord=	v.uvco[0], 1.0-v.uvco[1]
				uvcoord_key = rvec2d(uvcoord)
			
			if vertexColors:	color=		col[j].r, col[j].g, col[j].b
			
			
			key = normal_key, uvcoord_key, color
			
			vdict_local = vdict[v.index]
			
			if (not vdict_local) or (not vdict_local.has_key(key)):
				vdict_local[key] = vert_count;
				verts.append( (tuple(v.co), normal, uvcoord, color) )
				vert_count += 1
	
	
	file.write('SUBMESHTEXT0001\n')
	file.write('#Created by Blender3D %s - www.blender.org, source file: %s\n' % (Blender.Get('version'), Blender.Get('filename').split('/')[-1].split('\\')[-1] ))
	
	#file.write('element vertex %d\n' % len(verts))
	file.write('vertex format: position:3,texture0:2,normal:3\n')
	file.write('vertex count: %d\n'%len(verts))

	for i, v in enumerate(verts):
		file.write('[%.6f,%.6f,%.6f]' % v[0]) # co
		#if EXPORT_UV:
		file.write(' [%.6f,%.6f]' % v[2]) # uv
		#if EXPORT_NORMALS:
		file.write(' [%.6f,%.6f,%.6f]' % v[1]) # no
		
		#if EXPORT_COLORS:
		#	file.write('%u %u %u' % v[3]) # col
		file.write('\n')
	
	triangles=[]
	for (i, f) in enumerate(mesh.faces):
		#file.write('%d ' % len(f))
		smooth = f.smooth
		if not smooth: no = rvec3d(f.no)
		
		if faceUV:			uv = f.uv
		if vertexColors:	col = f.col

		if(len(f)==3):
			triangle=[]
			for j, v in enumerate(f):
				if f.smooth:		normal=		rvec3d(v.no)
				else:				normal=		no
				if faceUV:			uvcoord=	rvec2d((uv[j][0], 1.0-uv[j][1]))
				elif vertexUV:		uvcoord=	rvec2d((v.uvco[0], 1.0-v.uvco[1]))
				if vertexColors:	color=		col[j].r, col[j].g, col[j].b
		
				triangle+=[vdict[v.index][normal, uvcoord, color]]
			triangles+=[triangle]
				#file.write('%d ' % vdict[v.index][normal, uvcoord, color])
			#file.write('\n')
		else:
			x=[]
			for j, v in enumerate(f):
				if f.smooth:		normal=		rvec3d(v.no)
				else:				normal=		no
				if faceUV:			uvcoord=	rvec2d((uv[j][0], 1.0-uv[j][1]))
				elif vertexUV:		uvcoord=	rvec2d((v.uvco[0], 1.0-v.uvco[1]))
				if vertexColors:	color=		col[j].r, col[j].g, col[j].b
		
				#file.write('%d ' % vdict[v.index][normal, uvcoord, color])
				x+=[vdict[v.index][normal, uvcoord, color]]
			triangles+=[[x[1],x[2],x[0]]]
			triangles+=[[x[2],x[3],x[0]]]
			#file.write('[%d,%d,%d]\n'%())
			#file.write('[%d,%d,%d]\n'%(x[1],x[2],x[3]))

	file.write('triangle count: %d\n'%len(triangles))
	for (i, f) in enumerate(triangles):
		file.write('[%d,'%f[0])
		file.write('%d,'%f[1])
		file.write('%d]\n'%f[2])

	file.close()
	
	if is_editmode:
		Blender.Window.EditMode(1, '', 0)

def main():
	Blender.Window.FileSelector(file_callback, 'Spacewar submesh Export', Blender.sys.makename(ext='.submesh'))

if __name__=='__main__':
	main()
