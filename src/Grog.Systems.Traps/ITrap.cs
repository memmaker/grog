using Grog.Dressings.Beings;
using Grog.Dungeons;

namespace Grog.Systems.Traps;

public interface ITrap
{
	string Name { get; }

	bool IsAutoIdentifying { get; }

	void Activate(DungeonLevel dungeonLevel, Being being, int x, int y, bool willfulActivation);
}
