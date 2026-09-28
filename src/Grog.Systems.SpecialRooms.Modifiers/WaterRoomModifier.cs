using Grog.Dressings;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Rooms;
using Grog.Dungeons.Generators.Rooms.SpecialMessages;

namespace Grog.Systems.SpecialRooms.Modifiers;

public class WaterRoomModifier : RoomModifierBase
{
	public WaterRoomModifier(int minimumLevel, int rarity)
		: base(minimumLevel, rarity)
	{
	}

	public override bool Modify(DungeonLevel dungeonLevel, IRoom room)
	{
		room.SpecialMessage = new ConstantSpecialMessageProvider("This room is filled with water!");
		foreach (Position insidePosition in room.GetInsidePositions())
		{
			dungeonLevel.SetFeatureAt(insidePosition, Feature.Water);
		}
		return true;
	}
}
