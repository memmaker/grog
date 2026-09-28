using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grog.Dressings.Items.Inventory;

public class ItemSelectionList : IEnumerable<Item>, IEnumerable
{
	private List<Item> _items;

	private readonly bool _useAssociatedItemCharacters;

	private StringBuilder _associatedKeys = new StringBuilder();

	private Dictionary<char, Item> _associatedItems = new Dictionary<char, Item>();

	private Dictionary<Item, char> _associatedChars = new Dictionary<Item, char>();

	public int Count => _items.Count();

	public Item this[int itemIndex] => _items[itemIndex];

	public string AssociatedKeys => _associatedKeys.ToString();

	public int MaximumLength
	{
		get
		{
			int num = 0;
			foreach (Item item in _items)
			{
				num = Math.Max(num, item.Description.Length);
			}
			return num;
		}
	}

	public ItemSelectionList(List<Item> items, bool useAssociatedItemCharacters)
	{
		_items = items;
		_useAssociatedItemCharacters = useAssociatedItemCharacters;
		BuildSelectionListInternally();
	}

	private void BuildSelectionListInternally()
	{
		_associatedKeys.Clear();
		_associatedItems.Clear();
		_associatedChars.Clear();
		int num = 0;
		foreach (Item item in _items)
		{
			char c = (_useAssociatedItemCharacters ? item.AssociatedCharacter : ((char)(num + 97)));
			_associatedKeys.Append(c);
			_associatedItems.Add(c, item);
			_associatedChars.Add(item, c);
			num++;
		}
	}

	public IEnumerator<Item> GetEnumerator()
	{
		return _items.GetEnumerator();
	}

	public Item GetItemAssociatedWith(char c)
	{
		if (_associatedItems.ContainsKey(c))
		{
			return _associatedItems[c];
		}
		return null;
	}

	public void Remove(Item item)
	{
		_items.Remove(item);
		BuildSelectionListInternally();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public char CharacterAssociatedWith(Item item)
	{
		return _associatedChars[item];
	}
}
