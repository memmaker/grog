using System.Collections.Generic;
using Grog.Dungeons.Generators.Rooms.Decorators.DecorationGenerators;

namespace Grog.Dungeons.Generators.Rooms.Decorators;

public class GenerativeDecoratorSequentialRoomProcessingGenerator : IGenerator
{
	private readonly IRoomDecorationGenerator _generator;

	public GenerativeDecoratorSequentialRoomProcessingGenerator(IRoomDecorationGenerator generator)
	{
		_generator = generator;
	}

	public void Generate(DungeonLevel dungeonLevel)
	{
		foreach (IRoom item in new List<IRoom>(dungeonLevel.Rooms))
		{
			List<IRoom> rooms = new List<IRoom> { item };
			_generator.GetNextDecorator(item)?.Decorate(dungeonLevel, rooms);
		}
	}
}
