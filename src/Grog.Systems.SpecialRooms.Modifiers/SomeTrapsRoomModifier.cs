using Grog.Dressings.Features;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Rooms;
using Grog.Dungeons.Generators.Rooms.SpecialMessages;
using Grog.Kernel;

namespace Grog.Systems.SpecialRooms.Modifiers;

public class SomeTrapsRoomModifier : RoomModifierBase
{
	private readonly int _probability;

	public SomeTrapsRoomModifier(int minimumLevel, int rarity, int probability = 0)
		: base(minimumLevel, rarity)
	{
		_probability = probability;
	}

	public override bool Modify(DungeonLevel dungeonLevel, IRoom room)
	{
		int num = ((_probability == 0) ? (15 + dungeonLevel.Level * 2) : _probability);
		room.SpecialMessage = new ConstantSpecialMessageProvider("This room is covered by " + ((num > 80) ? "many " : "") + "scratches and old blood clots.");
		foreach (Position insidePosition in room.GetInsidePositions())
		{
			if (HiddenTrapFeature.IsSuitablePosition(dungeonLevel, insidePosition) && Game.Instance.Probability(num))
			{
				dungeonLevel.SetFeatureAt(insidePosition, new HiddenTrapFeature());
			}
		}
		return true;
	}
}
