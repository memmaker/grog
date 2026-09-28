using System.Collections.Generic;

namespace Grog.Dungeons.Generators.Rooms.Decorators;

public interface IRoomDecorator
{
	void Decorate(DungeonLevel dungeonLevel, List<IRoom> rooms);
}
