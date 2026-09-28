using System.Collections.Generic;

namespace Grog.Dressings.Items.Inventory;

internal class AssociatedCharacterComparer : IComparer<Item>
{
	public int Compare(Item x, Item y)
	{
		if (x.ItemType != y.ItemType)
		{
			return x.ItemType.CompareTo(y.ItemType);
		}
		return x.AssociatedCharacter.CompareTo(y.AssociatedCharacter);
	}
}
