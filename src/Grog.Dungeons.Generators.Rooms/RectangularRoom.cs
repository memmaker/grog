using System;
using Grog.Dressings;

namespace Grog.Dungeons.Generators.Rooms;

[Serializable]
public class RectangularRoom : RoomBase
{
	public int X1 { get; }

	public int Y1 { get; }

	public int X2 { get; }

	public int Y2 { get; }

	public RectangularRoom(DungeonLevel dungeonLevel, int x1, int y1, int x2, int y2)
	{
		X1 = x1;
		Y1 = y1;
		X2 = x2;
		Y2 = y2;
		for (int i = x1; i <= x2; i++)
		{
			for (int j = y1; j <= y2; j++)
			{
				Tile tile = ((i == x1 || i == x2 || j == y1 || j == y2) ? Tile.WallOfRoom : Tile.Floor);
				dungeonLevel.SetTile(i, j, tile);
				if (tile == Tile.Floor)
				{
					AddFloor(i, j);
				}
				else
				{
					AddWall(i, j);
				}
			}
		}
	}

	public override bool IsOutsideOfRoom(int x, int y)
	{
		if (x >= X1 && x <= X2 && y >= Y1)
		{
			return y > Y2;
		}
		return true;
	}

	public override bool IsWallOfRoom(int x, int y)
	{
		if (x != X1 && x != X2 && y != Y1)
		{
			return y == Y2;
		}
		return true;
	}

	public override bool IsInsideOfRoom(int x, int y)
	{
		if (x > X1 && x < X2 && y > Y1)
		{
			return y < Y2;
		}
		return false;
	}
}
