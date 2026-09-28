using System;
using System.Collections.Generic;
using Grog.Dressings.Items.Types;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Dressings.Items.Modifiers;

[Serializable]
public class ItemModifiers
{
	private List<IItemModifier> _modifiers;

	private Dictionary<ItemType, List<IItemPrefixModifier>> _prefixModifiers;

	private Dictionary<ItemType, List<IItemSuffixModifier>> _suffixModifiers;

	public ItemModifiers()
	{
		_modifiers = new List<IItemModifier>
		{
			new ItemPrefixModifier(14, "quicksilver", 250, ItemAbility.DexterityPlusThree, null, null, IsMetallicItem, ItemType.MeleeWeapon),
			new ItemPrefixModifier(8, "stealthy", 125, ItemAbility.Stealth, null, null, null, default(ItemType)),
			new ItemPrefixModifier(7, "rustproof", 130, ItemAbility.ResistRust, null, null, IsMetallicItem, ItemType.MeleeWeapon, ItemType.Armor, ItemType.Shield),
			new ItemPrefixModifier(3, "fickle", 135, ItemAbility.DexterityPlusOne, (Item i) =>
			{
				i.ToHitBonus--;
			}, (Item i) =>
			{
				i.ToHitBonus++;
			}, null, ItemType.MeleeWeapon),
			new ItemPrefixModifier(2, "masterwork", 150, ItemAbility.None, (Item i) =>
			{
				i.ItemBonus++;
			}, (Item i) =>
			{
				i.ItemBonus--;
			}, null, ItemType.MeleeWeapon, ItemType.Armor, ItemType.Shield),
			new ItemPrefixModifier(4, "mithril", 250, ItemAbility.None, (Item i) =>
			{
				i.ItemBonus += 2;
			}, (Item i) =>
			{
				i.ItemBonus -= 2;
			}, IsMetallicItem, ItemType.MeleeWeapon, ItemType.Armor, ItemType.Shield),
			new ItemPrefixModifier(7, "enchanted", 350, ItemAbility.None, (Item i) =>
			{
				i.ItemBonus += 3;
			}, (Item i) =>
			{
				i.ItemBonus -= 3;
			}, null, ItemType.MeleeWeapon, ItemType.Armor, ItemType.Shield),
			new ItemPrefixModifier(11, "adamantium", 450, ItemAbility.None, (Item i) =>
			{
				i.ItemBonus += 4;
			}, (Item i) =>
			{
				i.ItemBonus -= 4;
			}, IsMetallicItem, ItemType.MeleeWeapon, ItemType.Armor, ItemType.Shield),
			new ItemPrefixModifier(16, "eternium", 550, ItemAbility.None, (Item i) =>
			{
				i.ItemBonus += 5;
			}, (Item i) =>
			{
				i.ItemBonus -= 5;
			}, IsMetallicItem, ItemType.MeleeWeapon, ItemType.Armor, ItemType.Shield),
			new ItemPrefixModifier(22, "antediluvian", 650, ItemAbility.None, (Item i) =>
			{
				i.ItemBonus += 6;
			}, (Item i) =>
			{
				i.ItemBonus -= 6;
			}, null, ItemType.MeleeWeapon, ItemType.Armor, ItemType.Shield),
			new ItemPrefixModifier(2, "nasty", 130, ItemAbility.None, (Item i) =>
			{
				i.MeleeDamage.DieSides++;
			}, (Item i) =>
			{
				i.MeleeDamage.DieSides--;
			}, null, ItemType.MeleeWeapon),
			new ItemPrefixModifier(5, "wicked", 160, ItemAbility.None, (Item i) =>
			{
				i.MeleeDamage.DieSides += 3;
			}, (Item i) =>
			{
				i.MeleeDamage.DieSides -= 3;
			}, null, ItemType.MeleeWeapon),
			new ItemPrefixModifier(9, "bloodthirsty", 150, ItemAbility.None, (Item i) =>
			{
				i.MeleeDamage.DieBonus += 4;
			}, (Item i) =>
			{
				i.MeleeDamage.DieBonus -= 4;
			}, null, ItemType.MeleeWeapon),
			new ItemPrefixModifier(8, "brutal", 200, ItemAbility.None, (Item i) =>
			{
				i.MeleeDamage.NumberOfDice++;
			}, (Item i) =>
			{
				i.MeleeDamage.NumberOfDice--;
			}, null, ItemType.MeleeWeapon),
			new ItemPrefixModifier(2, "unwieldy", 30, ItemAbility.None, (Item i) =>
			{
				i.ToHitBonus -= 8;
			}, (Item i) =>
			{
				i.ToHitBonus += 8;
			}, null, ItemType.MeleeWeapon),
			new ItemPrefixModifier(3, "balanced", 150, ItemAbility.None, (Item i) =>
			{
				i.ToHitBonus += 4;
			}, (Item i) =>
			{
				i.ToHitBonus -= 4;
			}, null, ItemType.MeleeWeapon),
			new ItemPrefixModifier(4, "barbaric", 180, ItemAbility.None, (Item i) =>
			{
				i.ToHitBonus -= 4;
				i.MeleeDamage.DieBonus += 4;
			}, (Item i) =>
			{
				i.ToHitBonus += 4;
				i.MeleeDamage.DieBonus -= 4;
			}, null, ItemType.MeleeWeapon),
			new ItemSuffixModifier(10, "of slaughtering", 300, ItemAbility.None, (Item i) =>
			{
				i.MeleeDamage.NumberOfDice++;
				i.MeleeDamage.DieSides += 2;
			}, (Item i) =>
			{
				i.MeleeDamage.NumberOfDice--;
				i.MeleeDamage.DieSides -= 2;
			}, null, ItemType.MeleeWeapon),
			new ItemSuffixModifier(3, "of strength", 200, ItemAbility.StrengthPlusOne, null, null, null, ItemType.MeleeWeapon, ItemType.Armor, ItemType.Shield),
			new ItemSuffixModifier(6, "of ogre strength", 300, ItemAbility.StrengthPlusTwo, null, null, null, ItemType.MeleeWeapon, ItemType.Armor, ItemType.Shield),
			new ItemSuffixModifier(9, "of giant strength", 400, ItemAbility.StrengthPlusThree, null, null, null, ItemType.MeleeWeapon, ItemType.Armor, ItemType.Shield),
			new ItemSuffixModifier(5, "of toughness", 180, ItemAbility.ConstitutionPlusOne, null, null, null, ItemType.Armor, ItemType.Shield),
			new ItemSuffixModifier(11, "of might", 260, ItemAbility.ConstitutionPlusTwo, null, null, null, ItemType.Armor, ItemType.Shield),
			new ItemSuffixModifier(18, "of great might", 340, ItemAbility.ConstitutionPlusThree, null, null, null, ItemType.Armor, ItemType.Shield),
			new ItemSuffixModifier(5, "of health", 120, ItemAbility.ImmunityToSickness, null, null, null, ItemType.MeleeWeapon, ItemType.Armor, ItemType.Shield),
			new ItemSuffixModifier(18, "of regeneration", 500, ItemAbility.Regenerates, null, null, null, default(ItemType)),
			new ItemPrefixModifier(15, "vampiric", 500, ItemAbility.Regenerates, null, null, null, ItemType.MeleeWeapon),
			new ItemSuffixModifier(10, "of fortification", 140, ItemAbility.ImmunityToConfusion, (Item i) =>
			{
				i.AcBonus++;
			}, (Item i) =>
			{
				i.AcBonus--;
			}, null, ItemType.MeleeWeapon, ItemType.Armor, ItemType.Shield),
			new ItemSuffixModifier(4, "of speed", 150, ItemAbility.DexterityPlusOne, null, null, null, ItemType.MeleeWeapon),
			new ItemSuffixModifier(13, "of the eye", 110, ItemAbility.SeeInvisible, null, null, null, ItemType.MeleeWeapon, ItemType.Armor, ItemType.Shield),
			new ItemSuffixModifier(8, "of great speed", 200, ItemAbility.DexterityPlusTwo, null, null, null, ItemType.MeleeWeapon),
			new ItemSuffixModifier(8, "of demon slaying", 200, ItemAbility.DemonSlaying, null, null, null, ItemType.MeleeWeapon),
			new ItemSuffixModifier(12, "of dragon slaying", 400, ItemAbility.DragonSlaying, null, null, null, ItemType.MeleeWeapon),
			new ItemSuffixModifier(7, "of undead slaying", 800, ItemAbility.UndeadSlaying, null, null, null, ItemType.MeleeWeapon),
			new ItemSuffixModifier(2, "of animal slaying", 300, ItemAbility.AnimalSlaying, null, null, null, ItemType.MeleeWeapon),
			new ItemSuffixModifier(5, "of devil slaying", 200, ItemAbility.DevilSlaying, null, null, null, ItemType.MeleeWeapon)
		};
		_prefixModifiers = new Dictionary<ItemType, List<IItemPrefixModifier>>();
		_suffixModifiers = new Dictionary<ItemType, List<IItemSuffixModifier>>();
		foreach (IItemModifier modifier in _modifiers)
		{
			ItemType[] applicableItemTypes = modifier.GetApplicableItemTypes();
			foreach (ItemType key in applicableItemTypes)
			{
				if (modifier is IItemPrefixModifier item)
				{
					if (_prefixModifiers.ContainsKey(key))
					{
						_prefixModifiers[key].Add(item);
					}
					else
					{
						_prefixModifiers[key] = new List<IItemPrefixModifier> { item };
					}
				}
				if (modifier is IItemSuffixModifier item2)
				{
					if (_suffixModifiers.ContainsKey(key))
					{
						_suffixModifiers[key].Add(item2);
						continue;
					}
					_suffixModifiers[key] = new List<IItemSuffixModifier> { item2 };
				}
			}
		}
	}

	private bool IsMetallicItem(Item item)
	{
		return item.IsMetallic;
	}

	public Item GetModifiedItem(DungeonLevel dungeonLevel, Item item)
	{
		AddPrefixModifier(dungeonLevel, item);
		AddSuffixModifier(dungeonLevel, item);
		return item;
	}

	public void ModifyItemForExcessivelyLongName(Item item)
	{
		item.Prefix = (IItemPrefixModifier)_modifiers[9];
		item.Suffix = (IItemSuffixModifier)_modifiers[17];
	}

	public bool AddSuffixModifier(DungeonLevel dungeonLevel, Item item)
	{
		if (item.CanBeModifiedWithSuffix && _suffixModifiers.ContainsKey(item.ItemType) && Game.Instance.Probability(dungeonLevel.Level + 2))
		{
			List<IItemSuffixModifier> list = _suffixModifiers[item.ItemType];
			List<IItemSuffixModifier> list2 = new List<IItemSuffixModifier>();
			int num = 0;
			foreach (IItemSuffixModifier item2 in list)
			{
				if (item2.MinimumLevel <= dungeonLevel.Level && item2.CouldBeAppliedTo(item))
				{
					list2.Add(item2);
					num += item2.MinimumLevel;
				}
			}
			if (num > 0)
			{
				int num2 = Game.Instance.Random(num);
				foreach (IItemSuffixModifier item3 in list2)
				{
					num2 -= item3.MinimumLevel;
					if (num2 <= 0)
					{
						item.Suffix = item3;
						return true;
					}
				}
			}
		}
		return false;
	}

	public bool AddPrefixModifier(DungeonLevel dungeonLevel, Item item)
	{
		if (item.CanBeModifiedWithPrefix && _prefixModifiers.ContainsKey(item.ItemType) && Game.Instance.Probability(dungeonLevel.Level + 2))
		{
			List<IItemPrefixModifier> list = _prefixModifiers[item.ItemType];
			List<IItemPrefixModifier> list2 = new List<IItemPrefixModifier>();
			int num = 0;
			foreach (IItemPrefixModifier item2 in list)
			{
				if (item2.MinimumLevel <= dungeonLevel.Level && item2.CouldBeAppliedTo(item))
				{
					list2.Add(item2);
					num += item2.MinimumLevel;
				}
			}
			if (num > 0)
			{
				int num2 = Game.Instance.Random(num);
				foreach (IItemPrefixModifier item3 in list2)
				{
					num2 -= item3.MinimumLevel;
					if (num2 <= 0)
					{
						item.Prefix = item3;
						return true;
					}
				}
			}
		}
		return false;
	}

	public void AddInformation(List<string> information)
	{
		information.Add("Item modifiers: " + _modifiers.Count);
	}
}
