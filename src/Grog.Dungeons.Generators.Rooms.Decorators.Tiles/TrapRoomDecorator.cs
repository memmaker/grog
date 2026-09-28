using Grog.Dressings.Features;

namespace Grog.Dungeons.Generators.Rooms.Decorators.Tiles;

public class TrapRoomDecorator : DecorateAndConsumeSingleRoomBase
{
	protected override void Decorate(DungeonLevel dungeonLevel, IRoom room)
	{
		Position insidePositionForThing = room.GetInsidePositionForThing(dungeonLevel);
		if (!insidePositionForThing.Equals(Position.Undefined) && HiddenTrapFeature.IsSuitablePosition(dungeonLevel, insidePositionForThing))
		{
			dungeonLevel.SetFeatureAt(insidePositionForThing.X, insidePositionForThing.Y, new HiddenTrapFeature());
		}
	}
}
