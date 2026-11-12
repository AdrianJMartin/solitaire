using System;
using System.Collections.Generic;

// Going to only work on the british peg solitaire layout for now, but will add the european layout later. 

/* 
    The board is a 7x7 grid, with the following layout:
    
        0 1 2 3 4 5 6
      0     X X X
      1     X X X
      2 X X X X X X X
      3 X X X X X X X
      4 X X X X X X X
      5     X X X
      6     X X X

    Where 'X' represents a peg and 'O' represents an empty space. The goal of the game is to remove pegs by jumping over them with another peg, and the game is won when only one peg remains on the board.
*/

/*
    keep godot out of this class, and only use it to handle the logic of the game.
    we will need an adapter class to handle the godot specific stuff, like drawing the board and handling user input.
*/

public readonly record struct Vec2i( int x , int y);


public class Solitaire
{
    private List<Vec2i> holes = new List<Vec2i>();
    private Dictionary<Vec2i, int> positionToIndex = new Dictionary<Vec2i, int>();
    private Dictionary<int, Vec2i> indexToPosition = new Dictionary<int, Vec2i>();

    public ulong HolesBits = 0;
    public ulong StartingPegsBits = ( ( 0x1ul <<33) - 1 ) & ~( 0x1ul << 16) ; // all pegs except the center hole
    public ulong GoalPegBits = ( 0x1ul << 16) ;


    private ulong PegsBits = 0;

    public ulong PegPositionBits
    {
        get
        {
            return PegsBits;
        }
        set
        {
            PegsBits = value;
        }
    }


    public Solitaire()
    {
        // initialize the board with the british layout
        for (int y = 0; y < 7; y++)
        {
            for (int x = 0; x < 7; x++)
            {
                var hole = ( x > 1 && x < 5) || (y > 1 && y < 5);

                if (hole)
                {
                    holes.Add(new Vec2i(x, y));
                    positionToIndex[new Vec2i(x, y)] = holes.Count - 1;
                    indexToPosition[holes.Count - 1] = new Vec2i(x, y);
                }
            }
        }



        

        HolesBits = PositionsToBits(holes);
    }

    private ulong PositionToBit(Vec2i pos)
    {
        return 1UL << positionToIndex[pos];
    }

    // map board positions to bits in a 64 bit integer, where the least significant bit is the top left corner of the board, and the most significant bit is the bottom right corner of the board.
    public ulong PositionsToBits(List<Vec2i> positions)
    {
        ulong bits = 0;
        foreach (Vec2i pos in positions)
        {
            bits |= PositionToBit(pos);
        }
        return bits;
    }

    public List<Vec2i> BitsToPositions(ulong bits)
    {
        List<Vec2i> positions = [];
        for (int i = 0; i < 64; i++)
        {
            if ((bits & (1UL << i)) != 0)
            {
                positions.Add(indexToPosition[i]);
            }
        }
        return positions;
    }

    
}










