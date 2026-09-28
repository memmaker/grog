using Grog.Dungeons;
using Grog.Dungeons.Generators.Rooms;

namespace Grog.Systems.SpecialRooms;

public interface IRoomModifier
{
	int MinimumLevel { get; }

	int Rarity { get; }

	bool Modify(DungeonLevel dungeonLevel, IRoom room);
}
