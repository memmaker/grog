using Grog.Dressings.Beings;
using Grog.Dungeons;

namespace Grog.Dressings.Items;

public interface IItemProcessor
{
	Item Process(DungeonLevel dungeonLevel, Player grog, Item item);
}
