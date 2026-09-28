using System;
using Grog.Dressings.Beings;
using Grog.Dressings.Items.Types.Descriptions;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Dressings.Items.Types;

[Serializable]
public class Scroll : AliasItem
{
	public Scroll(ItemId id, string name, string alias, int value, Func<DungeonLevel, Being, Being, Item, bool> useAction)
		: base(id, ItemType.Scroll, '?', name, alias, 1, 0, value, 0, null, new Roll(1, 2, -1), isMetallic: false, ItemAbility.None, useAction, ScrollDescriptionGenerator.Instance, 1, 50, 35)
	{
	}
}
