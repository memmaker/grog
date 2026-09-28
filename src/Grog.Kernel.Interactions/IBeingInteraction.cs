using Grog.Dressings.Beings;
using Grog.Dungeons;

namespace Grog.Kernel.Interactions;

public interface IBeingInteraction
{
	bool Interact(DungeonLevel dungeonLevel, Being being);
}
