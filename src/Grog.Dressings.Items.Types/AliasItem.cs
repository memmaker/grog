using System;
using Grog.Dressings.Beings;
using Grog.Dressings.Items.Types.Descriptions;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Dressings.Items.Types;

[Serializable]
public class AliasItem : Item
{
	public AliasItem(ItemId id, ItemType itemType, char character, string name, string alias, int equipmentSlots, int acBonus, int value, int toHitBonus, Roll meleeDamage, Roll missileDamage, bool isMetallic, ItemAbility itemAbility, Func<DungeonLevel, Being, Being, Item, bool> useAction, IItemDescriptionGenerator descriptionGenerator, int throwingRange, int probabilityToBreakOnHit, int probabilityToBreakOnMiss)
		: base(id, itemType, character, name, alias, equipmentSlots, acBonus, value, toHitBonus, meleeDamage, missileDamage, isMetallic, itemAbility, useAction, descriptionGenerator, throwingRange, probabilityToBreakOnHit, probabilityToBreakOnMiss, canHavePrefixModifier: false, canHaveSuffixModifier: false)
	{
		if (alias == null)
		{
			throw new GrogException("An alias must be defined for item '" + id.ToString() + "/" + name + "'.");
		}
		Game.Instance.ItemPool.RegisterAlias(itemType, id, alias);
	}

	public AliasItem(Item template)
		: base(template)
	{
	}
}
