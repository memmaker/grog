using Grog.Dressings;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Rooms;
using Grog.Dungeons.Generators.Rooms.SpecialMessages;

namespace Grog.Systems.SpecialRooms.Modifiers;

public class WebbedRoomModifier : RoomModifierBase
{
	public WebbedRoomModifier(int minimumLevel, int rarity)
		: base(minimumLevel, rarity)
	{
	}

	public override bool Modify(DungeonLevel dungeonLevel, IRoom room)
	{
		room.SpecialMessage = new ConstantSpecialMessageProvider("Thousands of tiny spiders cover the surfaces of this room!");
		foreach (Position insidePosition in room.GetInsidePositions())
		{
			dungeonLevel.SetFeatureAt(insidePosition, Feature.SpiderWeb);
		}
		return true;
	}
}
