namespace Tests;

public class SolitaireTest
{
    [Fact]
    public void Solitaire_Initializes_Correctly()
    {
        // arrange
        var solitaire = new Solitaire();
        const ulong expected = (1UL << 33) - 1;

        // act 
        var bits = solitaire.HolesBits;

        //assert
        Assert.Equal(expected, bits);

    }



    [Fact]
    public void Solitaire_BitsToPositions_Returns_Correct_Positions()
    {
        // arrange
        var solitaire = new Solitaire();
        var bits = (0x1ul << 3) - 1;
        List<Vec2i> expected = [
            new Vec2i(2, 0),
            new Vec2i(3, 0),
            new Vec2i(4, 0)
            ];

        // act 
        var positions = solitaire.BitsToPositions(bits);

        //assert
        Assert.Equal(expected, positions);
    }

    

    [Fact]
    public void Solitaire_PositionsToBits_Returns_Correct_Bits()
    {
        // arrange
        var solitaire = new Solitaire();
        List<Vec2i> positions = [
            new Vec2i(2, 0),
            new Vec2i(3, 0),
            new Vec2i(4, 0)
            ];
        var expected = (0x1ul << 3) - 1;

        // act 
        var bits = solitaire.PositionsToBits(positions);

        //assert
        Assert.Equal(expected, bits);
    }



}
