using System;
using Grog.Dressings.Items.Types;

namespace Grog.Dressings.Items.Modifiers;

[Serializable]
public class ItemPrefixModifier : ItemModifierBase, IItemPrefixModifier, IItemModifier
{
	public ItemPrefixModifier(int minimumLevel, string attribute, int valueAdjustmentPercentage, ItemAbility itemAbility, Action<Item> whenAddedTo, Action<Item> whenRemovedFrom, Func<Item, bool> couldBeApplied, params ItemType[] applicableTypes)
		: base(minimumLevel, attribute, valueAdjustmentPercentage, itemAbility, whenAddedTo, whenRemovedFrom, couldBeApplied, applicableTypes)
	{
	}
}
