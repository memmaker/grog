using System.Collections.Generic;
using Grog.Kernel;

namespace Grog.Dungeons.Generators.Rooms.Decorators;

public abstract class DecorateAndConsumeSingleRoomBase : IRoomDecorator
{
	public void Decorate(DungeonLevel dungeonLevel, List<IRoom> rooms)
	{
		IRoom room = FindAppropriateRoom(dungeonLevel, rooms);
		if (room != null && rooms.Count > 1)
		{
			rooms.Remove(room);
		}
		if (room != null)
		{
			Decorate(dungeonLevel, room);
		}
	}

	protected virtual IRoom FindAppropriateRoom(DungeonLevel dungeonLevel, List<IRoom> rooms)
	{
		if (rooms.Count <= 0)
		{
			return null;
		}
		return rooms[Game.Instance.Random(rooms.Count)];
	}

	protected abstract void Decorate(DungeonLevel dungeonLevel, IRoom room);
}
