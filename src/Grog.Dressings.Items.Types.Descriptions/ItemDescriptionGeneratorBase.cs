using System;

namespace Grog.Dressings.Items.Types.Descriptions;

[Serializable]
public abstract class ItemDescriptionGeneratorBase : IItemDescriptionGenerator
{
	private readonly string _equippedText;

	public ItemDescriptionGeneratorBase(string equippedText)
	{
		_equippedText = equippedText;
	}

	public string GetItemDescription(Item item)
	{
		if (!item.IsIdentified)
		{
			return (GetUnidentifiedDescription(item) + " " + GetEquippedStatusDescription(item)).Trim();
		}
		return ((GetIdentifiedDescription(item) + " " + GetIdentifiedSpecification(item)).Trim() + " " + GetEquippedStatusDescription(item)).Trim();
	}

	public string GetShortItemDescription(Item item)
	{
		if (!item.IsIdentified)
		{
			return GetUnidentifiedDescription(item).Trim();
		}
		return (GetIdentifiedDescription(item) + " " + GetIdentifiedSpecification(item)).Trim();
	}

	protected virtual string GetUnidentifiedDescription(Item item)
	{
		return item.Alias ?? item.Name;
	}

	protected virtual string GetIdentifiedDescription(Item item)
	{
		return item.Name;
	}

	protected abstract string GetIdentifiedSpecification(Item item);

	public string GetEquippedStatusDescription(Item item)
	{
		if (!item.IsEquipped)
		{
			return "";
		}
		return "(" + _equippedText + ")";
	}
}
