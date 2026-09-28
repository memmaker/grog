using System.Collections.Generic;

namespace Grog.Dungeons.Generators.Rooms.Decorators;

public class MultipleGeneratorsPerRoomGenerator : IGenerator
{
	private readonly int _numberOfGenerations;

	private readonly IRoomDecorator[] _generators;

	public MultipleGeneratorsPerRoomGenerator(int numberOfGenerations, params IRoomDecorator[] generators)
	{
		_numberOfGenerations = numberOfGenerations;
		_generators = generators;
	}

	public void Generate(DungeonLevel dungeonLevel)
	{
		foreach (IRoom item in new List<IRoom>(dungeonLevel.Rooms))
		{
			IRoomDecorator[] generators = _generators;
			foreach (IRoomDecorator roomDecorator in generators)
			{
				for (int j = 0; j < _numberOfGenerations; j++)
				{
					roomDecorator.Decorate(dungeonLevel, new List<IRoom> { item });
				}
			}
		}
	}
}
