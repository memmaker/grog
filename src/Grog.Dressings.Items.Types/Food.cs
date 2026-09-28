using System;
using Grog.Dressings.Beings;
using Grog.Dressings.Items.Types.Descriptions;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Dressings.Items.Types;

[Serializable]
public class Food : Item
{
	public Food(ItemId id, string name, int value, Func<DungeonLevel, Being, Being, Item, bool> useAction)
		: base(id, ItemType.Food, '%', name, null, 1, 0, value, 0, null, new Roll(1, 3), isMetallic: false, ItemAbility.None, useAction, FoodDescriptionGenerator.Instance, 3, 100, 50, canHavePrefixModifier: false, canHaveSuffixModifier: false)
	{
	}
}
