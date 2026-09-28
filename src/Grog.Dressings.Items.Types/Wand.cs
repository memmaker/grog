using System;
using Grog.Dressings.Beings;
using Grog.Dressings.Items.Types.Descriptions;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Dressings.Items.Types;

[Serializable]
public class Wand : AliasItem
{
	public Wand(ItemId id, string name, string alias, int value, Func<DungeonLevel, Being, Being, Item, bool> useAction)
		: base(id, ItemType.Wand, '\\', name, alias, 1, 0, value, 0, null, new Roll(1, 2), isMetallic: false, ItemAbility.None, useAction, WandDescriptionGenerator.Instance, 4, 10, 5)
	{
	}
}
