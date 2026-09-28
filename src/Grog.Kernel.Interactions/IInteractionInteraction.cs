using Grog.Dressings.Beings;
using Grog.Dungeons;

namespace Grog.Kernel.Interactions;

public interface IInteractionInteraction
{
	void Interact(DungeonLevel dungeonLevel, Player grog, int x, int y);
}
