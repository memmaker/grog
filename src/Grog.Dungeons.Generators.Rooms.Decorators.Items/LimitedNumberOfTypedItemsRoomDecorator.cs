using System.Collections.Generic;
using Grog.Dressings.Items;
using Grog.Dressings.Items.Types;
using Grog.Kernel;

namespace Grog.Dungeons.Generators.Rooms.Decorators.Items;

public class LimitedNumberOfTypedItemsRoomDecorator : IRoomDecorator
{
	private readonly ItemType _type;

	private readonly int _numberOfItems;

	public LimitedNumberOfTypedItemsRoomDecorator(ItemType type, int numberOfItems)
	{
		_type = type;
		_numberOfItems = numberOfItems;
	}

	public void Decorate(DungeonLevel dungeonLevel, List<IRoom> rooms)
	{
		List<IRoom> list = new List<IRoom>(rooms);
		for (int i = 0; i < _numberOfItems; i++)
		{
			if (list.Count <= 0)
			{
				break;
			}
			IRoom room = list[Game.Instance.Random(list.Count)];
			list.Remove(room);
			Position insidePositionForItem = room.GetInsidePositionForItem(dungeonLevel);
			if (!insidePositionForItem.IsUndefined)
			{
				Item item = Game.Instance.ItemPool.CreateRandomItemOfType(dungeonLevel, _type, allowForCursedItems: true);
				if (item != null)
				{
					dungeonLevel.AddItem(insidePositionForItem.X, insidePositionForItem.Y, item);
				}
			}
		}
	}
}
