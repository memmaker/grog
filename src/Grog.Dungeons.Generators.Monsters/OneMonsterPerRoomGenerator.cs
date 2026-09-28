using Grog.Dressings.Beings;
using Grog.Dungeons.Generators.Rooms;

namespace Grog.Dungeons.Generators.Monsters;

public class OneMonsterPerRoomGenerator : MonsterGeneratorBase
{
	protected override void GenerateMonsters(MonsterList availableMonsters, DungeonLevel dungeonLevel)
	{
		foreach (IRoom room in dungeonLevel.Rooms)
		{
			Position insidePosition;
			do
			{
				insidePosition = room.GetInsidePosition();
			}
			while (insidePosition.Equals(Position.Undefined) || dungeonLevel.GetThingAt(insidePosition.X, insidePosition.Y) != null);
			Being being = availableMonsters.CreateNewMonster(dungeonLevel);
			dungeonLevel.SetBeing(insidePosition.X, insidePosition.Y, being);
		}
	}
}
