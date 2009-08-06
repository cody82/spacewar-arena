out_name = GetSaveFileName()
out_file= createfile out_name

obj = selection[1]


format "bonecount: %\r\n" (skinOps.GetNumberBones obj.skin) to: out_file
for i=1 to (skinOps.GetNumberBones obj.skin) do
(
	bone_name=skinOps.GetBoneName obj.skin i 0
	bone = getnodebyname bone_name
	parent_name=""
	if bone.parent!=undefined then
	(
		parent_name=bone.parent.name
	)
	format "bone: % % [%,%,%,%] %\r\n" bone_name bone.position bone.rotation.x bone.rotation.y bone.rotation.z bone.rotation.w parent_name to: out_file
)


close out_file