using System.Collections.Generic;
using Grog.Dressings.Items;
using Grog.Kernel;

namespace Grog.Dungeons.Generators.Rooms.Decorators.Items;

public class MultipleItemRoomDecorator : IRoomDecorator
{
	private readonly int _numberOfItems;

	public MultipleItemRoomDecorator(int numberOfItems)
	{
		_numberOfItems = numberOfItems;
	}

	public void Decorate(DungeonLevel dungeonLevel, List<IRoom> rooms)
	{
		foreach (IRoom room in rooms)
		{
			for (int i = 0; i < _numberOfItems; i++)
			{
				Position insidePositionForItem = room.GetInsidePositionForItem(dungeonLevel);
				if (!insidePositionForItem.IsUndefined)
				{
					Item item = Game.Instance.ItemPool.CreateRandomItem(dungeonLevel, allowForCursedItems: true);
					if (item != null)
					{
						dungeonLevel.AddItem(insidePositionForItem.X, insidePositionForItem.Y, item);
					}
				}
			}
		}
	}
}
