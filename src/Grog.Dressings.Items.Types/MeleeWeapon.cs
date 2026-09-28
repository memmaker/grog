using System;
using Grog.Dressings.Items.Types.Descriptions;
using Grog.Kernel;

namespace Grog.Dressings.Items.Types;

[Serializable]
public class MeleeWeapon : Item
{
	public MeleeWeapon(ItemId id, string name, int equipmentSlots, int value, int toHitBonus, Roll meleeDamage, bool isMetallic, bool canHavePrefixModifier = true, bool canHaveSuffixModifier = true)
		: base(id, ItemType.MeleeWeapon, '(', name, null, equipmentSlots, 0, value, toHitBonus, meleeDamage, meleeDamage, isMetallic, ItemAbility.MeleeCapability, null, MeleeWeaponDescriptionGenerator.Instance, 5, 20, 15, canHavePrefixModifier, canHaveSuffixModifier)
	{
	}
}
