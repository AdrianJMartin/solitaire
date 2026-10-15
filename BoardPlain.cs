using Godot;
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Diagnostics.Metrics;


public partial class BoardPlain : Node3D
{

	private Vector3 haloResetPosition;
	private Node3D haloNode;


	[Export]
	public int BoardStyle
	{
		get
		{
			return boardStyle;
		}
		set
		{
			boardStyle = value;
			if( IsNodeReady())
			{
				ResetBoard(boardLayouts[boardStyle]);
			}
		}
	}

	private int boardStyle = 1;


	private string[] boardLayouts =
	{
		"xxMMMxx" + "\n" +
		"xxMMMxx" + "\n" +
		"MMMMMMM" + "\n" +
		"MMM.MMM" + "\n" +
		"MMMMMMM" + "\n" +
		"xxMMMxx" + "\n" +
		"xxMMMxx"
		,
		"xxMMMxx" + "\n" +
		"xMMMMMx" + "\n" +
		"MMMMMMM" + "\n" +
		"MMM.MMM" + "\n" +
		"MMMMMMM" + "\n" +
		"xMMMMMx" + "\n" +
		"xxMMMxx"
		,
		"MMMxxxxxx" + "\n" +
		"MMMxxxxxx" + "\n" +
		"MMMxxxxxx" + "\n" +
		"MMMxxxxxx" + "\n" +
		"MMMxxxxxx" + "\n" +
		"MMMxxxxxx" + "\n" +
		"MMMMMMMMM" + "\n" +
		"M.MMMMMMM" + "\n" +
		"MMMMMMMMM"
	};

	private List<Node3D> activeMarbles = [];

	private void ClearBoard()
	{
		foreach (var b in activeMarbles)
		{
			b.Free();
		}

		activeMarbles.Clear();
	}

	private void ResetBoard(string style)
	{

		ClearBoard();

		var marble = (Node3D)GetNode("ball-red");
		var hole = (Node3D)GetNode("hole");


		var rows = style.Split('\n');
		var y = rows.Length / -2;

		var marbleName = "marble";
		var counter = 0;

		foreach (var r in rows)
		{
			var x = rows[0].Length / -2;
			foreach (var c in r)
			{
				List<Node3D> nodes = [];
				nodes.Clear();

				switch (c)
				{
					case 'M':
						{
							var n = (Node3D)marble.Duplicate();
							var st = n.GetChild<StaticBody3D>(0);
							st.MouseEntered += () => OnStaticBody3dMouseEnteredHandler(n);
							st.MouseExited += () => OnStaticBody3dMouseExitedHandler(n);

							n.Name = $"{marbleName}_{counter++}";

							nodes.Add(n);
							break;
						}
				}

				if (c != 'x')
				{

					var n = (Node3D)hole.Duplicate();
					nodes.Add(n);
				}


				foreach (var n in nodes)
				{
					n.Position = new Vector3(x, 0.0f, y);
					n.Visible = true;
					activeMarbles.Add(n);
					AddChild(n);
				}

				x++;
			}
			y++;
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		{
			Node3D marble = GetNode<Node3D>("ball-red");
			var s = new SphereShape3D
			{
				Radius = 0.2f
			};

			var co = new CollisionShape3D
			{
				Shape = s
			};

			var sb = new StaticBody3D();
			sb.AddChild(co);
			marble.AddChild(sb);
		}


		foreach (var assetName in new[] { "ball-red", "ball-blue", "halo", "arrow-jump", "hole" })
		{
			GetNode<Node3D>(assetName).Visible = false;
		}

		haloNode = GetNode<Node3D>("halo");
		haloResetPosition = haloNode.Position;

		ResetBoard(boardLayouts[boardStyle]);

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnStaticBody3dMouseEnteredHandler(Godot.Node3D n)
	{
		Godot.GD.Print($"mouse entered: {n.Name}");
		ShowSelectionArround(n);
	}
	public void OnStaticBody3dMouseExitedHandler(Godot.Node3D n)
	{
		Godot.GD.Print($"mouse exited: {n.Name}");
		HideSelectionNode(n);
	}

	private void ShowSelectionArround(Godot.Node3D n)
	{
		haloNode.Position = n.Position;
		haloNode.Visible = true;
	}
	private void HideSelectionNode(Godot.Node3D n)
	{
		haloNode.Position = haloResetPosition;
		haloNode.Visible = false;
	}

}
