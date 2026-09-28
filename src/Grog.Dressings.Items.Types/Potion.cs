using System;
using Grog.Dressings.Beings;
using Grog.Dressings.Items.Types.Descriptions;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Dressings.Items.Types;

[Serializable]
public class Potion : AliasItem
{
	public Potion(ItemId id, string name, string alias, int value, Func<DungeonLevel, Being, Being, Item, bool> useAction)
		: base(id, ItemType.Potion, '!', name, alias, 1, 0, value, 0, null, new Roll(1, 2), isMetallic: false, ItemAbility.None, useAction, PotionDescriptionGenerator.Instance, 5, 100, 80)
	{
	}
}
