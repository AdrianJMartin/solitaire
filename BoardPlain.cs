using Godot;
using System;
using System.Runtime.CompilerServices;





public partial class BoardPlain : Node3D
{

	private Vector3 halo_reset_position;
	private Node3D halo_node;





	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

		halo_node = GetNode<Node3D>("halo");
		halo_reset_position = halo_node.Position;


		var grid = GetNode<Node3D>("grid");
		foreach (Node child in grid.GetChildren())
		{
			if (child is Node3D n)
			{
				if (n.Name.ToString().StartsWith("ball"))
				{
					var s = new SphereShape3D();
					s.Radius = 0.2f;

					var co = new CollisionShape3D();
					co.Shape = s;

					var sb = new StaticBody3D();
					sb.AddChild(co);

					n.AddChild(sb);

					sb.MouseEntered += () => _on_static_body_3d_mouse_entered_handler(n);
					sb.MouseExited += () => _on_static_body_3d_mouse_exited_handler(n);
				}
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void _on_static_body_3d_mouse_entered_handler(Godot.Node3D n)
	{
		Godot.GD.Print($"mouse entered: {n.Name}");
		show_selection_arround( n );
	}
	public void _on_static_body_3d_mouse_exited_handler(Godot.Node3D n)
	{
		Godot.GD.Print($"mouse exited: {n.Name}");
		GetNode<Node3D>("halo").Position = halo_reset_position;
	}

	private void show_selection_arround( Godot.Node3D n)
	{
		halo_node.Position = n.Position;
	}
	private void hide_selection_node( Godot.Node3D n)
	{
		halo_node.Position = halo_reset_position;
	}

}
