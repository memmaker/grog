using Grog.Dressings;
using Grog.Dungeons;

namespace Grog.Kernel.Interactions;

public interface IMoveIntoInteraction
{
	bool MoveInto(DungeonLevel dungeonLevel, Thing attacker, Thing target, int fx, int fy, int tx, int ty);
}
