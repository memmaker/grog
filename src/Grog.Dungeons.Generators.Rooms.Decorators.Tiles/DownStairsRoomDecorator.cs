using Grog.Dressings;

namespace Grog.Dungeons.Generators.Rooms.Decorators.Tiles;

public class DownStairsRoomDecorator : DecorateAndConsumeSingleRoomBase
{
	protected override void Decorate(DungeonLevel dungeonLevel, IRoom room)
	{
		Position trulyInsidePositionForThing = room.GetTrulyInsidePositionForThing(dungeonLevel);
		dungeonLevel.SetTile(trulyInsidePositionForThing.X, trulyInsidePositionForThing.Y, Tile.StairDown);
	}
}
