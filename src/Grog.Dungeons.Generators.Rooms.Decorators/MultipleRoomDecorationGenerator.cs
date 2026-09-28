using System;
using System.Collections.Generic;
using Grog.Kernel;

namespace Grog.Dungeons.Generators.Rooms.Decorators;

public class MultipleRoomDecorationGenerator : IGenerator
{
	private readonly int _numberOfRoomsToDecorate;

	private readonly IRoomDecorator[] _generators;

	public MultipleRoomDecorationGenerator(int numberOfRoomsToDecorate, params IRoomDecorator[] generators)
	{
		_numberOfRoomsToDecorate = numberOfRoomsToDecorate;
		_generators = generators;
	}

	public void Generate(DungeonLevel dungeonLevel)
	{
		List<IRoom> list = new List<IRoom>(dungeonLevel.Rooms);
		for (int i = 0; i < list.Count; i++)
		{
			int num = Game.Instance.Random(list.Count);
			int num2 = Game.Instance.Random(list.Count);
			int index = num;
			List<IRoom> list2 = list;
			int index2 = num2;
			IRoom room = list[num2];
			IRoom room2 = list[num];
			IRoom room3 = (list[index] = room);
			room3 = (list2[index2] = room2);
		}
		for (int j = 0; j < Math.Min(_numberOfRoomsToDecorate, list.Count); j++)
		{
			IRoomDecorator[] generators = _generators;
			for (int index2 = 0; index2 < generators.Length; index2++)
			{
				generators[index2].Decorate(dungeonLevel, new List<IRoom> { list[j] });
			}
		}
	}
}
