using System;
using System.Text;
using Grog.Dressings.Beings;
using Grog.Dressings.Items.Modifiers;
using Grog.Dressings.Items.Types;
using Grog.Dressings.Items.Types.Descriptions;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Dressings.Items;

[Serializable]
public class Item
{
	private readonly IItemDescriptionGenerator _descriptionGenerator;

	private IItemPrefixModifier _itemPrefixModifier;

	private IItemSuffixModifier _itemSuffixModifier;

	private string _name;

	private int _value;

	private ItemAbility _itemAbility;

	private bool _identified;

	public IItemPrefixModifier Prefix
	{
		set
		{
			_itemPrefixModifier?.WhenRemovedFrom(this);
			_itemPrefixModifier = value;
			_itemPrefixModifier?.WhenAddedTo(this);
		}
	}

	public IItemSuffixModifier Suffix
	{
		set
		{
			_itemSuffixModifier?.WhenRemovedFrom(this);
			_itemSuffixModifier = value;
			_itemSuffixModifier?.WhenAddedTo(this);
		}
	}

	public bool HasPrefix => _itemPrefixModifier != null;

	public bool HasSuffix => _itemSuffixModifier != null;

	public char AssociatedCharacter { get; set; }

	public ItemId Id { get; }

	public ItemType ItemType { get; }

	public char Character { get; }

	public long TurnsOwned { get; set; }

	public bool IsExciting
	{
		get
		{
			if (!IsIdentified && (HasPrefix || HasSuffix) && TurnsOwned > Math.Max(200, 500 - Game.Instance.Grog.Level * 20))
			{
				return !IsKnownToBeExciting;
			}
			return false;
		}
	}

	public bool IsReadyToBeIdentifiedOtherwise
	{
		get
		{
			if (!IsIdentified && ItemType != ItemType.Food)
			{
				return TurnsOwned > Math.Max(300, 900 - Game.Instance.Grog.Level * 30);
			}
			return false;
		}
	}

	public string Name
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (IsCursedStatusKnown)
			{
				if (IsCursed)
				{
					stringBuilder.Append("cursed ");
				}
				else
				{
					stringBuilder.Append("uncursed ");
				}
			}
			if (HasPrefix && IsIdentified)
			{
				stringBuilder.Append(_itemPrefixModifier.Attribute).Append(' ');
			}
			stringBuilder.Append(_name);
			if (HasSuffix && IsIdentified)
			{
				stringBuilder.Append(' ').Append(_itemSuffixModifier.Attribute);
			}
			return stringBuilder.ToString();
		}
		set
		{
			_name = value;
		}
	}

	public string Alias { get; }

	public int EquipmentSlots { get; }

	public int AcBonus { get; set; }

	public int ToHitBonus { get; set; }

	public int Value
	{
		get
		{
			return _value * (_itemPrefixModifier?.ValueAdjustmentPercentage ?? 100) / 100 * (_itemSuffixModifier?.ValueAdjustmentPercentage ?? 100) / 100;
		}
		set
		{
			_value = value;
		}
	}

	public Roll MeleeDamage { get; set; }

	public Roll MissileDamage { get; }

	public int ItemBonus { get; set; }

	public Func<DungeonLevel, Being, Being, Item, bool> UseAction { get; }

	public bool IsEquipped { get; set; }

	public bool IsCursed { get; set; }

	public bool IsCursedStatusKnown { get; set; }

	public bool IsIdentified
	{
		get
		{
			if (_identified)
			{
				return true;
			}
			if (ItemType == ItemType.Potion || ItemType == ItemType.Ring || ItemType == ItemType.Wand || ItemType == ItemType.Scroll)
			{
				return Game.Instance.ItemPool.IsIdentified(this);
			}
			return false;
		}
		set
		{
			if (_identified != value)
			{
				_identified = value;
				if (_identified)
				{
					Game.Instance.ItemPool.Identify(this);
				}
			}
		}
	}

	public string Description => _descriptionGenerator.GetItemDescription(this);

	public string ShortDescription => _descriptionGenerator.GetShortItemDescription(this);

	public bool IsMetallic { get; }

	public int ThrowingRange { get; }

	public int ProbabilityToBreakOnHit { get; }

	public int ProbabilityToBreakOnMiss { get; }

	public bool CanHavePrefixModifier { get; }

	public bool CanHaveSuffixModifier { get; }

	public bool IsEquippable
	{
		get
		{
			if (ItemType != ItemType.Armor && ItemType != ItemType.Ring && ItemType != ItemType.Shield)
			{
				return ItemType == ItemType.MeleeWeapon;
			}
			return true;
		}
	}

	public bool CanBeModifiedWithPrefix
	{
		get
		{
			if (CanHavePrefixModifier)
			{
				return !HasPrefix;
			}
			return false;
		}
	}

	public bool CanBeModifiedWithSuffix
	{
		get
		{
			if (CanHaveSuffixModifier)
			{
				return !HasSuffix;
			}
			return false;
		}
	}

	public int Charges { get; set; }

	public bool IsKnownToBeExciting { get; set; }

	public Item(ItemId id, ItemType itemType, char character, string name, string alias, int equipmentSlots, int acBonus, int value, int toHitBonus, Roll meleeDamage, Roll missileDamage, bool isMetallic, ItemAbility itemAbility, Func<DungeonLevel, Being, Being, Item, bool> useAction, IItemDescriptionGenerator descriptionGenerator, int throwingRange, int probabilityToBreakOnHit, int probabilityToBreakOnMiss, bool canHavePrefixModifier, bool canHaveSuffixModifier, IItemPrefixModifier itemPrefixModifier = null, IItemSuffixModifier itemSuffixModifier = null)
	{
		_descriptionGenerator = descriptionGenerator;
		_itemPrefixModifier = itemPrefixModifier;
		_itemSuffixModifier = itemSuffixModifier;
		Id = id;
		ItemType = itemType;
		Character = character;
		Name = name;
		Alias = alias;
		EquipmentSlots = equipmentSlots;
		AcBonus = acBonus;
		ToHitBonus = toHitBonus;
		MeleeDamage = meleeDamage;
		MissileDamage = missileDamage;
		Value = value;
		_itemAbility = itemAbility;
		UseAction = useAction;
		ThrowingRange = throwingRange;
		ProbabilityToBreakOnHit = probabilityToBreakOnHit;
		ProbabilityToBreakOnMiss = probabilityToBreakOnMiss;
		CanHavePrefixModifier = canHavePrefixModifier;
		CanHaveSuffixModifier = canHaveSuffixModifier;
		IsEquipped = false;
		IsMetallic = isMetallic;
	}

	public Item(Item template)
		: this(template.Id, template.ItemType, template.Character, template.Name, template.Alias, template.EquipmentSlots, template.AcBonus, template.Value, template.ToHitBonus, (template.MeleeDamage == null) ? null : new Roll(template.MeleeDamage), (template.MissileDamage == null) ? null : new Roll(template.MissileDamage), template.IsMetallic, template._itemAbility, template.UseAction, template._descriptionGenerator, template.ThrowingRange, template.ProbabilityToBreakOnHit, template.ProbabilityToBreakOnMiss, template.CanHavePrefixModifier, template.CanHaveSuffixModifier, template._itemPrefixModifier, template._itemSuffixModifier)
	{
		IsIdentified = false;
		AssociatedCharacter = ' ';
	}

	public bool HasItemAbility(ItemAbility ability)
	{
		if (_itemAbility == ability)
		{
			return true;
		}
		if (HasPrefix && _itemPrefixModifier.ItemAbility == ability)
		{
			return true;
		}
		if (HasSuffix && _itemSuffixModifier.ItemAbility == ability)
		{
			return true;
		}
		return false;
	}

	public string A()
	{
		string text = a();
		return char.ToUpper(text[0]) + text.Substring(1);
	}

	public string a()
	{
		string description = Description;
		if (description.EndsWith("s"))
		{
			return description;
		}
		return (("aeiouAEIOU".IndexOf(description[0]) == -1) ? "a" : "an") + " " + description;
	}
}
