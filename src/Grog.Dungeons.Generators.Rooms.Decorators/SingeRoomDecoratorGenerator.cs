using System.Collections.Generic;
using Grog.Kernel;

namespace Grog.Dungeons.Generators.Rooms.Decorators;

public class SingeRoomDecoratorGenerator : IGenerator
{
	private readonly IRoomDecorator _decorator;

	public SingeRoomDecoratorGenerator(IRoomDecorator decorator)
	{
		_decorator = decorator;
	}

	public void Generate(DungeonLevel dungeonLevel)
	{
		List<IRoom> list = new List<IRoom>(dungeonLevel.Rooms);
		IRoom item = list[Game.Instance.Random(list.Count)];
		List<IRoom> rooms = new List<IRoom> { item };
		_decorator.Decorate(dungeonLevel, rooms);
	}
}
