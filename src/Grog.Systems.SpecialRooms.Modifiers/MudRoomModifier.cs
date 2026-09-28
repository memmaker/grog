using Grog.Dressings;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Rooms;
using Grog.Dungeons.Generators.Rooms.SpecialMessages;

namespace Grog.Systems.SpecialRooms.Modifiers;

public class MudRoomModifier : RoomModifierBase
{
	public MudRoomModifier(int minimumLevel, int rarity)
		: base(minimumLevel, rarity)
	{
	}

	public override bool Modify(DungeonLevel dungeonLevel, IRoom room)
	{
		room.SpecialMessage = new ConstantSpecialMessageProvider("The floor of this room is covered with thick mud!");
		foreach (Position insidePosition in room.GetInsidePositions())
		{
			dungeonLevel.SetFeatureAt(insidePosition, Feature.Mud);
		}
		return true;
	}
}
