using Grog.Dungeons.Generators.Rooms;
using Grog.Kernel;

namespace Grog.Dungeons.Generators;

public class SpecialRoomGenerator : IGenerator
{
	private readonly int _amount;

	public SpecialRoomGenerator(int amount = 1)
	{
		_amount = amount;
	}

	public void Generate(DungeonLevel dungeonLevel)
	{
		for (int i = 0; i < _amount; i++)
		{
			bool flag = false;
			int num = 50;
			while (!flag && num-- > 0)
			{
				IRoom room = dungeonLevel.FindEmptyRoom();
				if (room == null)
				{
					return;
				}
				flag = Game.Instance.SpecialRoomSystem.Modify(dungeonLevel, room);
			}
		}
	}
}
