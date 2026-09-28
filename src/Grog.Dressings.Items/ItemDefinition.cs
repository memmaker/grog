using System;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Dressings.Items;

[Serializable]
public class ItemDefinition
{
	private readonly Roll _charges;

	private int _occurrences;

	public Item Template { get; }

	public int MinimumLevel { get; }

	public int Rarity { get; }

	public int MaximumOccurance { get; }

	public ItemDefinition(Item template, int minimumLevel, int rarity = 100, int maximumOccurance = int.MaxValue, Roll charges = null)
	{
		_charges = charges;
		Template = template;
		MinimumLevel = minimumLevel;
		Rarity = rarity;
		MaximumOccurance = maximumOccurance;
		_occurrences = 0;
	}

	public Item CreateItem()
	{
		_occurrences++;
		Item item = new Item(Template);
		if (_charges != null)
		{
			item.Charges = _charges.GetDieResult();
		}
		return item;
	}

	public bool IsAvailable(DungeonLevel dungeonLevel)
	{
		if (dungeonLevel.Level >= MinimumLevel)
		{
			return _occurrences <= MaximumOccurance;
		}
		return false;
	}
}
