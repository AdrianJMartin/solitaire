using Godot;
using System;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Diagnostics.Metrics;
using System.Data;

public class ValidMove
{
	public enum Direction
	{
		North,
		South,
		East,
		West
	}
	public Node3D source;
	public Node3D middle;
	public Vector3 landing;
	public Direction direction;

	public ValidMove(Node3D s, Node3D m, Vector3 l, Direction d = Direction.North)
	{
		source = s;
		middle = m;
		landing = l;
		direction = d;
	}
}


public partial class BoardPlain : Node3D
{
	private Vector3 haloResetPosition;
	private Node3D haloNode;
	private Material redMaterial = null;
	private Material blueMaterial = null;
	private Material debugMaterial = null;

	[Export]
	public PackedScene MarbleScene { get; set; }

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
			if (IsNodeReady())
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

	private Dictionary<Vector3, int> PositionToBitMap = [];

	private void ClearBoard()
	{
		foreach (var b in activeMarbles)
		{
			b.Free();
		}

		activeMarbles.Clear();

		PositionToBitMap.Clear();
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
		var bitIndex = 0;

		foreach (var r in rows)
		{
			var x = rows[0].Length / -2;
			foreach (var c in r)
			{
				List<Node3D> nodes = [];
				nodes.Clear();

				switch (c)
				{
					case '.':
						{
							var pos = new Vector3(x, 0.0f, y);
							break;
						}
					case 'M':
						{
							var n = (Node3D)marble.Duplicate();
							n.Position = new Vector3(x, 0.0f, y);
							n.Visible = true;

							var st = n.GetChild<StaticBody3D>(0);

							st.MouseEntered += () => OnStaticBody3dMouseEnteredHandler(n);
							st.MouseExited += () => OnStaticBody3dMouseExitedHandler(n);
							st.InputEvent += (camera, @event, clickPosition, clickNormal, shapeIdx) => OnInputEventHandler(camera, @event, clickPosition, clickNormal, shapeIdx, n);

							n.Name = $"{marbleName}_{counter++}";
							activeMarbles.Add(n);
							AddChild(n);
							break;
						}
				}

				if (c != 'x')
				{
					var n = (Node3D)hole.Duplicate();
					n.Visible = true;
					n.Position = new Vector3(x, 0.0f, y);
					PositionToBitMap.Add(n.Position, bitIndex++);
					AddChild(n);

					var text = new Label3D();
					text.Text = $"{n.Position}";
					text.Position = new Vector3(x, 0.0f, y - 0.5f);
					text.Rotation = new Vector3(-MathF.PI / 2.0f, 0.0f, 0.0f);
					AddChild(text);
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

		redMaterial = ResourceLoader.Load<Material>("mat-red.tres");
		blueMaterial = ResourceLoader.Load<Material>("mat-blue.tres");
		debugMaterial = ResourceLoader.Load<Material>("mat-debug.tres");

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
		ShowValidMoves(n);
	}
	public void OnStaticBody3dMouseExitedHandler(Godot.Node3D n)
	{
		Godot.GD.Print($"mouse exited: {n.Name}");
		HideSelectionNode(n);
		HideValidMoves();
		ResetMaterials();
	}

	private void ResetMaterials()
	{
		foreach (MeshInstance3D m in activeMarbles)
		{
			m.MaterialOverride = null;
		}
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

	private Node3D MarbleAtPosition(Vector3 pos)
	{
		var marbles = FindChildren("marble_*", "Node3D", true, false);
		foreach (Node3D m in marbles.Cast<Node3D>())
		{
			if (pos.IsEqualApprox(m.Position))
			{
				return m;
			}
		}
		return null;
	}

	private ulong PositionToBits()
	{
		ulong bits = 0x0;

		foreach (var (pos, bitIndex) in PositionToBitMap)
		{
			if (MarbleAtPosition(pos) != null)
			{
				bits |= 0x1ul << bitIndex;
			}
		}

		return bits;
	}

	List<ValidMove> validMoves = [];

	private void ShowValidMoves(Node3D s)
	{
		// we need to check if the landing spot is valid too! eg on the board!

		Node3D middle = null;
		Node3D landing = null;
		Vector3 landingPosition = new(0.0f, 0.0f, 0.0f);

		// north
		middle = MarbleAtPosition(s.Position + new Vector3(0.0f, 0.0f, 1.0f));
		landingPosition = s.Position + new Vector3(0.0f, 0.0f, 2.0f);
		landing = MarbleAtPosition(landingPosition);

		// the valid move model, nuetrally points from positive to negative z, so we need to rotate it to point in the direction of the jump

		if (middle != null && landing == null && PositionToBitMap.ContainsKey(landingPosition))
		{
			var validMoveNode3d = (Node3D)FindChild("arrow-jump");
			var n = (Node3D)validMoveNode3d.Duplicate();
			n.Position = s.Position;
			n.Rotate(new Vector3(0.0f, 1.0f, 0.0f), MathF.PI);
			n.Visible = true;
			validMoves.Add(new ValidMove(s, middle, landingPosition, ValidMove.Direction.South));
			AddChild(n);
			((MeshInstance3D)middle).MaterialOverride = blueMaterial;
			GD.Print("South");
		}

		// south 
		middle = MarbleAtPosition(s.Position + new Vector3(0.0f, 0.0f, -1.0f));
		landingPosition = s.Position + new Vector3(0.0f, 0.0f, -2.0f);
		landing = MarbleAtPosition(landingPosition);

		if (middle != null && landing == null && PositionToBitMap.ContainsKey(landingPosition))
		{
			var validMoveNode3d = (Node3D)FindChild("arrow-jump");
			var n = (Node3D)validMoveNode3d.Duplicate();
			n.Position = s.Position;
			n.Rotate(new Vector3(0.0f, 1.0f, 0.0f), 0.0f);
			n.Visible = true;
			validMoves.Add(new ValidMove(s, middle, landingPosition, ValidMove.Direction.North));
			AddChild(n);
			((MeshInstance3D)middle).MaterialOverride = blueMaterial;
			GD.Print("south");
		}

		// east 
		middle = MarbleAtPosition(s.Position + new Vector3(-1.0f, 0.0f, 0.0f));
		landingPosition = s.Position + new Vector3(-2.0f, 0.0f, 0.0f);
		landing = MarbleAtPosition(landingPosition);

		if (middle != null && landing == null && PositionToBitMap.ContainsKey(landingPosition))
		{
			var validMoveNode3d = (Node3D)FindChild("arrow-jump");
			var n = (Node3D)validMoveNode3d.Duplicate();
			n.Position = s.Position;
			n.Rotate(new Vector3(0.0f, 1.0f, 0.0f), MathF.PI * 0.5f);
			n.Visible = true;
			validMoves.Add(new ValidMove(s, middle, landingPosition, ValidMove.Direction.East));
			AddChild(n);
			((MeshInstance3D)middle).MaterialOverride = blueMaterial;
			GD.Print("east");
		}

		// west 
		middle = MarbleAtPosition(s.Position + new Vector3(1.0f, 0.0f, 0.0f));
		landingPosition = s.Position + new Vector3(2.0f, 0.0f, 0.0f);
		landing = MarbleAtPosition(landingPosition);

		if (middle != null && landing == null && PositionToBitMap.ContainsKey(landingPosition))
		{
			var validMoveNode3d = (Node3D)FindChild("arrow-jump");
			var n = (Node3D)validMoveNode3d.Duplicate();
			n.Position = s.Position;
			n.Rotate(new Vector3(0.0f, 1.0f, 0.0f), MathF.PI * -0.5f);
			n.Visible = true;
			validMoves.Add(new ValidMove(s, middle, landingPosition, ValidMove.Direction.West));
			AddChild(n);
			((MeshInstance3D)middle).MaterialOverride = blueMaterial;
			GD.Print("west");
		}



	}

	private void OnInputEventHandler(Node camera, InputEvent @event, Vector3 clickPosition, Vector3 clickNormal, long shapeIdx, Node3D n)
	{
		if (@event is InputEventMouseButton mouseEvent)
		{
			GD.Print($"{@event.GetType().Name} | {@event}");
			if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
			{
				GD.Print($"Left mouse button clicked at: {clickPosition}");
				GD.Print($"Clicked on marble: {n.Name}");
				GD.Print($"Valid moves: {validMoves.Count}");

				if (validMoves.Count == 1)
				{

					// copy in the marble with animation
					var m = MarbleScene.Instantiate<Node3D>();
					AddChild(m);
					m.Position = validMoves[0].source.Position;

					// roatate the marble to face the direction of the jump
					// the initial rotaion of the animationis east, which does not matche the logic above....lol
					switch (validMoves[0].direction)
					{
						case ValidMove.Direction.North:
							m.Rotate(new Vector3(0.0f, 1.0f, 0.0f), MathF.PI * 0.5f);
							break;
						case ValidMove.Direction.East:
							m.Rotate(new Vector3(0.0f, 1.0f, 0.0f), MathF.PI);
							break;
						case ValidMove.Direction.West:
							break;
						case ValidMove.Direction.South:
							m.Rotate(new Vector3(0.0f, 1.0f, 0.0f), MathF.PI * 0.1f);
							break;
					}

					// play the jump animation
					var animPlayer = m.GetNode<AnimationPlayer>("AnimationPlayer");
					animPlayer.Play("jump");
					animPlayer.AnimationFinished += (animName) =>
					{
						if (animName == "jump")
						{
							// remove the middle marble
							MarbleAtPosition(validMoves[0].middle.Position).QueueFree();

							// move the source marble to the landing position
							validMoves[0].source.Position = validMoves[0].landing;

							((MeshInstance3D)validMoves[0].source).MaterialOverride = debugMaterial;

							m.QueueFree();

							// hide the valid moves
							HideValidMoves();

						}
					};


				}
			}
		}
	}


	private void HideValidMoves()
	{
		foreach (var n in validMoves)
		{
			n.source.QueueFree();
		}

		validMoves.Clear();
	}


	public void debugButton()
	{
		GD.Print("Debug Pressed!");

		var bits = PositionToBits();
		var asString = Convert.ToString((long)bits, 2).PadLeft(64, '0');

		GD.Print($"As bits: {asString}");
		GD.Print($"As number: {bits}");
	}
}