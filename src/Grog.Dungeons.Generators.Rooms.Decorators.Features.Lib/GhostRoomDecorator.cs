using Grog.Kernel;
using Grog.Systems.Ghosts;

namespace Grog.Dungeons.Generators.Rooms.Decorators.Features.Lib;

public class GhostRoomDecorator : DecorateAndConsumeSingleRoomBase
{
	protected override void Decorate(DungeonLevel dungeonLevel, IRoom room)
	{
		Position insidePosition = room.GetInsidePosition();
		if (!insidePosition.Equals(Position.Undefined) && dungeonLevel.GetThingAt(insidePosition.X, insidePosition.Y) == null)
		{
			Ghost ghost = Game.Instance.Ghosts.CreateGhostMonsterIfAppropriate(dungeonLevel);
			if (ghost != null)
			{
				dungeonLevel.SetBeing(insidePosition.X, insidePosition.Y, ghost);
			}
		}
	}
}
