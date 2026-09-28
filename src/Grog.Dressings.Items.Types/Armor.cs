using System;
using Grog.Dressings.Items.Types.Descriptions;
using Grog.Kernel;

namespace Grog.Dressings.Items.Types;

[Serializable]
public class Armor : Item
{
	public Armor(ItemId id, string name, int acBonus, int value, Roll missileDamage, bool isMetallic, bool canHavePrefixModifier = true, bool canHaveSuffixModifier = true)
		: base(id, ItemType.Armor, '[', name, null, 1, acBonus, value, 0, null, missileDamage, isMetallic, ItemAbility.ArmorClassBonus, null, ArmorDescriptionGenerator.Instance, 2, 10, 3, canHavePrefixModifier, canHaveSuffixModifier)
	{
	}
}
