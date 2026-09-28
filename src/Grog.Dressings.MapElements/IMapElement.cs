using Grog.Dressings.Beings;
using Grog.Dungeons;

namespace Grog.Dressings.MapElements;

public interface IMapElement
{
	void AddAdjacentInteraction(IMapElementInteraction interaction);

	void InteractWithAdjacentElement(DungeonLevel dungeonLevel, Player grog, int x, int y, IMapElement element, InteractionContext context);

	void AddDiagonalInteraction(IMapElementInteraction interaction);

	void InteractWithDiagonalElement(DungeonLevel dungeonLevel, Player grog, int x, int y, IMapElement element, InteractionContext context);
}
