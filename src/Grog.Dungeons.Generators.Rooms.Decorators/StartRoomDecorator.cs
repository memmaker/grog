using Grog.Dressings;
using Grog.Kernel;

namespace Grog.Dungeons.Generators.Rooms.Decorators;

public class StartRoomDecorator : DecorateAndConsumeSingleRoomBase
{
	protected override void Decorate(DungeonLevel dungeonLevel, IRoom room)
	{
		Position trulyInsidePositionForThing = room.GetTrulyInsidePositionForThing(dungeonLevel);
		dungeonLevel.SetTile(trulyInsidePositionForThing.X, trulyInsidePositionForThing.Y, Tile.StairUp);
		dungeonLevel.SetBeing(trulyInsidePositionForThing.X, trulyInsidePositionForThing.Y, Game.Instance.Grog);
	}
}
