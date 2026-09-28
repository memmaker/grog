using Grog.Dressings;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Rooms;
using Grog.Dungeons.Generators.Rooms.SpecialMessages;
using Grog.Kernel;

namespace Grog.Systems.SpecialRooms.Modifiers;

public class TombRoomModifier : RoomModifierBase
{
	public TombRoomModifier(int minimumLevel, int rarity)
		: base(minimumLevel, rarity)
	{
	}

	public override bool Modify(DungeonLevel dungeonLevel, IRoom room)
	{
		bool flag = false;
		foreach (Position insidePosition in room.GetInsidePositions())
		{
			if (Game.Instance.Probability(30))
			{
				dungeonLevel.SetFeatureAt(insidePosition, Feature.Gravestone);
				flag = true;
			}
		}
		if (flag)
		{
			room.SpecialMessage = new ConstantSpecialMessageProvider("This seems to be an ancient graveyard!");
		}
		return flag;
	}
}
