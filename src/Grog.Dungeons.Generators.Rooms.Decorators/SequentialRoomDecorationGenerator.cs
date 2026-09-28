using System.Collections.Generic;

namespace Grog.Dungeons.Generators.Rooms.Decorators;

public class SequentialRoomDecorationGenerator : IGenerator
{
	private readonly List<IRoomDecorator> _roomDecorators;

	public SequentialRoomDecorationGenerator(List<IRoomDecorator> roomDecorators)
	{
		_roomDecorators = roomDecorators;
	}

	public SequentialRoomDecorationGenerator(params IRoomDecorator[] roomDecorators)
		: this(new List<IRoomDecorator>(roomDecorators))
	{
	}

	public SequentialRoomDecorationGenerator(IRoomDecorator first, IRoomDecorator second, List<IRoomDecorator> others)
	{
		_roomDecorators = new List<IRoomDecorator>();
		_roomDecorators.Add(first);
		_roomDecorators.Add(second);
		_roomDecorators.AddRange(others);
	}

	public SequentialRoomDecorationGenerator(IRoomDecorator one, List<IRoomDecorator> more)
	{
		_roomDecorators = new List<IRoomDecorator>();
		_roomDecorators.Add(one);
		_roomDecorators.AddRange(more);
	}

	public void Generate(DungeonLevel dungeonLevel)
	{
		List<IRoom> list = new List<IRoom>(dungeonLevel.Rooms);
		foreach (IRoomDecorator roomDecorator in _roomDecorators)
		{
			roomDecorator.Decorate(dungeonLevel, list);
			if (list.Count == 0)
			{
				break;
			}
		}
	}
}
