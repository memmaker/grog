using Grog.Dressings.Beings;
using Grog.Dungeons;

namespace Grog.Dressings.MapElements;

public interface IMapElementInteraction
{
	void Interact(DungeonLevel dungeonLevel, Player grog, int x, int y, IMapElement element, InteractionContext context);
}
