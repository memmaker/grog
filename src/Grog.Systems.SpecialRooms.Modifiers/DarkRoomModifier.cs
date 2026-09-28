using Grog.Dungeons;
using Grog.Dungeons.Generators.Rooms;
using Grog.Dungeons.Generators.Rooms.SpecialMessages;

namespace Grog.Systems.SpecialRooms.Modifiers;

public class DarkRoomModifier : RoomModifierBase
{
	public DarkRoomModifier(int minimumLevel, int rarity)
		: base(minimumLevel, rarity)
	{
	}

	public override bool Modify(DungeonLevel dungeonLevel, IRoom room)
	{
		room.IsDark = true;
		room.SpecialMessage = new ConstantSpecialMessageProvider("This room is pitch black!");
		return true;
	}
}
