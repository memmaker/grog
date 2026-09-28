using System;
using Grog.Dressings;
using Grog.Kernel;

namespace Grog.Dungeons.Generators.Rooms.Generators;

public abstract class RoomGeneratorBase : IGenerator
{
	protected RoomGeneratorBase(DungeonLevel dungeonLevel)
	{
		for (int i = 0; i < dungeonLevel.Width; i++)
		{
			for (int j = 0; j < dungeonLevel.Height; j++)
			{
				dungeonLevel.SetTile(i, j, Tile.Wall);
			}
		}
	}

	public abstract void Generate(DungeonLevel dungeonLevel);

	protected void Distribute<T>(DungeonLevel dungeonLevel, int amount, Func<int, int, bool> isValidPosition, Func<T> createThing, Action<int, int, T> placeThing, int attempts = 100)
	{
		for (int i = 0; i < amount; i++)
		{
			int num = attempts;
			while (num-- > 0)
			{
				int arg = Game.Instance.Random(dungeonLevel.Width);
				int arg2 = Game.Instance.Random(dungeonLevel.Height);
				if (isValidPosition(arg, arg2))
				{
					T arg3 = createThing();
					placeThing(arg, arg2, arg3);
					break;
				}
			}
		}
	}
}
