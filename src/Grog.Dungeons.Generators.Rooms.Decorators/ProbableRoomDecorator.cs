using System.Collections.Generic;
using Grog.Kernel;

namespace Grog.Dungeons.Generators.Rooms.Decorators;

public class ProbableRoomDecorator : IRoomDecorator
{
	private readonly int _probability;

	private readonly IRoomDecorator _roomDecorator;

	public ProbableRoomDecorator(int probability, IRoomDecorator roomDecorator)
	{
		_probability = probability;
		_roomDecorator = roomDecorator;
	}

	public void Decorate(DungeonLevel dungeonLevel, List<IRoom> rooms)
	{
		if (Game.Instance.Random(100) < _probability)
		{
			_roomDecorator.Decorate(dungeonLevel, rooms);
		}
	}
}
