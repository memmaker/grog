using System.Collections;
using System.Collections.Generic;
using Grog.Dressings.Items.Types;
using Grog.Kernel;

namespace Grog.Dressings.Items;

public class ListOfItemDefinitions : IEnumerable<ItemDefinition>, IEnumerable
{
	private List<ItemDefinition> _definitions = new List<ItemDefinition>();

	private int _total;

	public void Add(ItemDefinition definition)
	{
		_definitions.Add(definition);
		_total += definition.Rarity;
	}

	public ItemDefinition GetRandomItemDefinition()
	{
		if (_total == 0)
		{
			return null;
		}
		int num = Game.Instance.Random(_total);
		foreach (ItemDefinition definition in _definitions)
		{
			num -= definition.Rarity;
			if (num < 0)
			{
				return definition;
			}
		}
		throw new GrogException("Failed to select a random item definition: " + num + "/" + _total);
	}

	public void FilterBy(ItemType type)
	{
		List<ItemDefinition> list = new List<ItemDefinition>();
		foreach (ItemDefinition definition in _definitions)
		{
			if (definition.Template.ItemType == type)
			{
				list.Add(definition);
			}
			else
			{
				_total -= definition.Rarity;
			}
		}
		_definitions = list;
	}

	public IEnumerator<ItemDefinition> GetEnumerator()
	{
		return _definitions.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
