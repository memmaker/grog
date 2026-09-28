using Grog.Dungeons;
using Grog.Dungeons.Generators.Rooms;

namespace Grog.Systems.SpecialRooms;

public abstract class RoomModifierBase : IRoomModifier
{
	public int MinimumLevel { get; }

	public int Rarity { get; }

	protected RoomModifierBase(int minimumLevel, int rarity)
	{
		MinimumLevel = minimumLevel;
		Rarity = rarity;
	}

	public abstract bool Modify(DungeonLevel dungeonLevel, IRoom room);
}
