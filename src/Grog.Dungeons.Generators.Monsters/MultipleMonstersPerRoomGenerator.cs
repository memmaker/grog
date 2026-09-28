using System;
using Grog.Dressings.Beings;
using Grog.Dungeons.Generators.Rooms;

namespace Grog.Dungeons.Generators.Monsters;

public class MultipleMonstersPerRoomGenerator : MonsterGeneratorBase
{
	private readonly Func<int> _numberOfMonsters;

	public MultipleMonstersPerRoomGenerator(Func<int> numberOfMonsters)
	{
		_numberOfMonsters = numberOfMonsters;
	}

	protected override void GenerateMonsters(MonsterList availableMonsters, DungeonLevel dungeonLevel)
	{
		foreach (IRoom room in dungeonLevel.Rooms)
		{
			int num = 0;
			int num2 = 0;
			int count = room.GetInsidePositions().Count;
			int num3 = _numberOfMonsters();
			while (num < num3)
			{
				Position insidePosition = room.GetInsidePosition();
				if (dungeonLevel.GetThingAt(insidePosition.X, insidePosition.Y) == null)
				{
					Being being = availableMonsters.CreateNewMonster(dungeonLevel);
					dungeonLevel.SetBeing(insidePosition.X, insidePosition.Y, being);
					num++;
				}
				else if (num2 > count)
				{
					num++;
				}
				else
				{
					num2++;
				}
			}
		}
	}
}
