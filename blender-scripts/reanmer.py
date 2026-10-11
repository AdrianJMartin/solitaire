# exports each selected object into its own file


import bpy

view_layer = bpy.context.view_layer
obj_active = view_layer.objects.active
selection = bpy.context.selected_objects

grid = selection[0]

bpy.ops.object.select_all(action='DESELECT')

for obj in grid.children:


    x = int( obj.location.x )
    y = int( obj.location.y )

    name = f"ball-{x}-{6-y}"

    obj.name = name
    
    





view_layer.objects.active = obj_active

for obj in selection:
    obj.select_set(True)
