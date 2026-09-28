using System;
using Grog.Dressings.Beings;
using Grog.Dressings.Items.Types.Descriptions;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Dressings.Items.Types;

[Serializable]
public class Ring : AliasItem
{
	public Ring(ItemId id, string name, string alias, ItemAbility itemAbility, int value, int acBonus = 0, int toHitBonus = 0, Func<DungeonLevel, Being, Being, Item, bool> useAction = null)
		: base(id, ItemType.Ring, '=', name, alias, 1, acBonus, value, toHitBonus, new Roll(0, 0, 1), new Roll(0, 0, 1), isMetallic: true, itemAbility, useAction, RingDescriptionGenerator.Instance, 7, 10, 10)
	{
	}

	public Ring(Item template)
		: base(template)
	{
	}
}
