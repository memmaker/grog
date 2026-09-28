using System;
using Grog.Kernel;

namespace Grog.Dungeons.Generators.Rooms.Builders;

public class RectangularRoomBuilder : IRoomBuilder
{
	public IRoom DigRoom(DungeonLevel dungeonLevel, int x, int y, int maxWidth, int maxHeight)
	{
		if (maxWidth < 7)
		{
			throw new GrogException("Grid too small. Minimum grid width of 7 required instead of " + maxWidth + ".");
		}
		if (maxHeight < 6)
		{
			throw new GrogException("Grid too small. Minimum grid height of 6 required instead of " + maxHeight + ".");
		}
		int num = Math.Min(Math.Max(4 + Game.Instance.Random((maxWidth - 2) * 2 / 3), 4), 14);
		int num2 = Math.Min(Math.Max((maxHeight - 2) / 3 + Game.Instance.Random((maxHeight - 2) * 2 / 3), 4), 7);
		int num3 = maxWidth - num - 2;
		int num4 = maxHeight - num2 - ((y <= 0) ? 1 : 2);
		int num5 = x + 1 + ((num3 > 0) ? Game.Instance.Random(num3 + 1) : 0);
		int num6 = y + ((y > 0) ? 1 : 0) + ((num4 > 0) ? Game.Instance.Random(num4 + 1) : 0);
		return dungeonLevel.AddRoom(num5, num6, num5 + num - 1, num6 + num2 - 1);
	}
}
