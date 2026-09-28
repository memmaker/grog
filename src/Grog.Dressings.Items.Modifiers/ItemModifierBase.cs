using System;
using Grog.Dressings.Items.Types;

namespace Grog.Dressings.Items.Modifiers;

[Serializable]
public class ItemModifierBase : IItemModifier
{
	private readonly Action<Item> _whenAddedTo;

	private readonly Action<Item> _whenRemovedFrom;

	private readonly Func<Item, bool> _couldBeApplied;

	private readonly ItemType[] _applicableTypes;

	public int MinimumLevel { get; }

	public string Attribute { get; }

	public int ValueAdjustmentPercentage { get; }

	public ItemAbility ItemAbility { get; }

	public ItemType[] GetApplicableItemTypes()
	{
		return _applicableTypes;
	}

	protected ItemModifierBase(int minimumLevel, string attribute, int valueAdjustmentPercentage, ItemAbility itemAbility, Action<Item> whenAddedTo, Action<Item> whenRemovedFrom, Func<Item, bool> couldBeApplied, params ItemType[] applicableTypes)
	{
		_whenAddedTo = whenAddedTo;
		_whenRemovedFrom = whenRemovedFrom;
		_couldBeApplied = couldBeApplied;
		_applicableTypes = applicableTypes;
		MinimumLevel = minimumLevel;
		Attribute = attribute;
		ValueAdjustmentPercentage = valueAdjustmentPercentage;
		ItemAbility = itemAbility;
	}

	public void WhenAddedTo(Item item)
	{
		if (_whenAddedTo != null)
		{
			_whenAddedTo(item);
		}
	}

	public void WhenRemovedFrom(Item item)
	{
		if (_whenRemovedFrom != null)
		{
			_whenRemovedFrom(item);
		}
	}

	public bool CouldBeAppliedTo(Item item)
	{
		if (_couldBeApplied == null)
		{
			return true;
		}
		return _couldBeApplied(item);
	}
}
