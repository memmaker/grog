using System;
using Grog.Kernel;

namespace Grog.Dressings.Items.Types.Descriptions;

[Serializable]
public class MeleeWeaponDescriptionGenerator : ItemDescriptionGeneratorBase
{
	public static readonly MeleeWeaponDescriptionGenerator Instance = new MeleeWeaponDescriptionGenerator();

	public MeleeWeaponDescriptionGenerator()
		: base("wielded")
	{
	}

	protected override string GetIdentifiedSpecification(Item item)
	{
		int num = item.ToHitBonus + item.ItemBonus + Game.Instance.Grog.MultiAttackBonus;
		return ((item.ItemBonus != 0 || num != 0) ? (((num == 0) ? "+0" : ((num < 0) ? (num.ToString() ?? "") : ("+" + num))) + ", " + ((item.ItemBonus != 0) ? (((item.ItemBonus < 0) ? (item.ItemBonus.ToString() ?? "") : ("+" + item.ItemBonus)) + " ") : "+0 ")) : "") + "[" + item.MeleeDamage.GetSpecification(item.ItemBonus + Game.Instance.Grog.ToDamageBonus) + " damage]";
	}
}
