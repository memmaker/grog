using Grog.Dungeons;

namespace Grog.Dressings;

public interface IThingAction
{
	void Act(DungeonLevel dungeonLevel, Thing thing);
}
