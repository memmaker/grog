using Grog.Dungeons;
using Grog.Dungeons.Generators.Rooms;
using Grog.Dungeons.Generators.Rooms.SpecialMessages;

namespace Grog.Systems.SpecialRooms.Modifiers;

public class SpecialPowerRoomModifier : RoomModifierBase
{
	private readonly string _specialMessage;

	private readonly SpecialRoomPower _specialRoomPower;

	public SpecialPowerRoomModifier(int minimumLevel, int rarity, string specialMessage, SpecialRoomPower specialRoomPower)
		: base(minimumLevel, rarity)
	{
		_specialMessage = specialMessage;
		_specialRoomPower = specialRoomPower;
	}

	public override bool Modify(DungeonLevel dungeonLevel, IRoom room)
	{
		if (_specialMessage != null)
		{
			room.SpecialMessage = new ConstantSpecialMessageProvider(_specialMessage);
		}
		room.SpecialPower = _specialRoomPower;
		return true;
	}
}
