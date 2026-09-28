using System;
using Grog.Dressings.Items.Types.Descriptions;
using Grog.Kernel;

namespace Grog.Dressings.Items.Types;

[Serializable]
public class Shield : Item
{
	public Shield(ItemId id, string name, int acBonus, int value, Roll missileDamage, bool isMetallic, bool canHavePrefixModifier = true, bool canHaveSuffixModifier = true)
		: base(id, ItemType.Shield, ']', name, null, 1, acBonus, value, 0, null, missileDamage, isMetallic, ItemAbility.ArmorClassBonus, null, ShieldDescriptionGenerator.Instance, 4, 30, 15, canHavePrefixModifier, canHaveSuffixModifier)
	{
	}
}
