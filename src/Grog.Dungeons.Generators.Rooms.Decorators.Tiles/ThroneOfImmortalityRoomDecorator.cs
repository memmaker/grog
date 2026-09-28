using Grog.Dressings;
using Grog.Dressings.Beings;
using Grog.Dungeons.Generators.Monsters;
using Grog.Kernel;

namespace Grog.Dungeons.Generators.Rooms.Decorators.Tiles;

public class ThroneOfImmortalityRoomDecorator : DecorateAndConsumeSingleRoomBase
{
	protected override void Decorate(DungeonLevel dungeonLevel, IRoom room)
	{
		Position trulyInsidePositionForThing = room.GetTrulyInsidePositionForThing(dungeonLevel);
		dungeonLevel.SetTile(trulyInsidePositionForThing.X, trulyInsidePositionForThing.Y, Tile.ThroneOfImmortality);
		Being being = Game.Instance.MonsterPool.CreateMonster(dungeonLevel, MonsterPool.MonsterType.Lich);
		being.ChristenedName = "Kalmius";
		being.Mood = Mood.WaitForPlayer;
		dungeonLevel.SetBeing(trulyInsidePositionForThing.X, trulyInsidePositionForThing.Y, being);
	}
}
