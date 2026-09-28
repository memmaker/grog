namespace Grog.Kernel;

public static class DirectionExtensions
{
	public static (int, int) GetDirectionalModifiers(this Direction direction)
	{
		return direction switch
		{
			Direction.MinDirection => (0, -1), 
			Direction.East => (1, 0), 
			Direction.South => (0, 1), 
			Direction.West => (-1, 0), 
			_ => (0, 0), 
		};
	}
}
