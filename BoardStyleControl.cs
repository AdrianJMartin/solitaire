using Godot;
using System;

public partial class BoardStyleControl : VBoxContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var board = GetNode<BoardPlain>("../board-plain");
		GetNode<Button>("boardStyleEnglish").ButtonUp += () => { board.BoardStyle = 0; };
		GetNode<Button>("boardStyleEuropean").ButtonUp += () => { board.BoardStyle = 1; };
		GetNode<Button>("boardStyleCustom").ButtonUp += () => { board.BoardStyle = 2; };
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

}
