using Grog.Dressings.Features;
using Grog.Dungeons.Generators.Rooms.Decorators.Features.Lib.Base;

namespace Grog.Dungeons.Generators.Rooms.Decorators.Features.Lib;

public class AltarFeatureRoomDecorator : FeatureRoomDecoratorBase
{
	public AltarFeatureRoomDecorator()
		: base(useEmptyRoomsOnly: true)
	{
	}

	protected override void Decorate(DungeonLevel dungeonLevel, IRoom room)
	{
		Position unusedTrulyInsidePosition = room.GetUnusedTrulyInsidePosition(dungeonLevel);
		if (!unusedTrulyInsidePosition.Equals(Position.Undefined))
		{
			dungeonLevel.SetFeatureAt(unusedTrulyInsidePosition.X, unusedTrulyInsidePosition.Y, new AltarFeature(dungeonLevel));
		}
	}
}
