using System;
using System.Collections.Generic;
using Grog.Dressings.Beings;
using Grog.Dressings.Items.Types;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Dressings.Items.Inventory;

[Serializable]
public class Inventory
{
	private readonly Player _grog;

	private const int AbsoluteMaximumItemCount = 26;

	private Item[] _items = new Item[26];

	public int MaxItemCount => Math.Min(26, _grog.Strength);

	public int ItemsCarried { get; private set; }

	public int EquippedArmorClassBonus
	{
		get
		{
			int num = 0;
			Item[] items = _items;
			foreach (Item item in items)
			{
				if (item != null && item.IsEquipped)
				{
					num += item.AcBonus;
					if (item.HasItemAbility(ItemAbility.ArmorClassBonus))
					{
						num += item.ItemBonus;
					}
				}
			}
			return num;
		}
	}

	public int EquippedToHitBonus
	{
		get
		{
			int num = 0;
			Item[] items = _items;
			foreach (Item item in items)
			{
				if (item != null && item.IsEquipped)
				{
					num += item.ToHitBonus;
					if (item.HasItemAbility(ItemAbility.MeleeCapability))
					{
						num += item.ItemBonus;
					}
				}
			}
			return num;
		}
	}

	public bool IsCarryingCursedItems => GetFilteredItems((Item i) => i.IsCursed).Count > 0;

	public Inventory(Player grog)
	{
		_grog = grog;
		ItemsCarried = 0;
	}

	public void Add(Item item)
	{
		ItemsCarried++;
		if (char.IsLower(item.AssociatedCharacter) && char.IsLetter(item.AssociatedCharacter))
		{
			int num = item.AssociatedCharacter - 97;
			if (_items[num] == null)
			{
				_items[num] = item;
				return;
			}
		}
		for (int i = 0; i < Math.Min(_items.Length, MaxItemCount); i++)
		{
			if (_items[i] == null)
			{
				_items[i] = item;
				item.AssociatedCharacter = (char)(97 + i);
				break;
			}
		}
	}

	public void EquipInitially(Item item)
	{
		item.IsEquipped = true;
		item.IsIdentified = true;
		Add(item);
	}

	public List<Item> GetInventory()
	{
		List<Item> list = new List<Item>();
		Item[] items = _items;
		foreach (Item item in items)
		{
			if (item != null)
			{
				list.Add(item);
			}
		}
		list.Sort(new AssociatedCharacterComparer());
		return list;
	}

	public bool CanTake(int amount)
	{
		return ItemsCarried + amount <= MaxItemCount;
	}

	public void Remove(Item item)
	{
		int num = 0;
		Item[] items = _items;
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i] == item)
			{
				_items[num] = null;
				ItemsCarried--;
				item.IsEquipped = false;
				return;
			}
			num++;
		}
		throw new GrogException("Failed to remove " + item.Description + " from inventory.");
	}

	public List<Item> GetEquippedItemsOfType(ItemType itemType)
	{
		return GetFilteredItems((Item i) => i.IsEquipped && i.ItemType == itemType);
	}

	public List<Item> GetFilteredItems(Func<Item, bool> matches)
	{
		List<Item> list = new List<Item>();
		Item[] items = _items;
		foreach (Item item in items)
		{
			if (item != null && matches(item))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public bool HasEquippedItemOfType(ItemId itemId)
	{
		Item[] items = _items;
		foreach (Item item in items)
		{
			if (item != null && item.Id == itemId)
			{
				return true;
			}
		}
		return false;
	}

	public List<Item> GetCarriedItemsOfType(ItemType itemType)
	{
		return GetFilteredItems((Item i) => i.ItemType == itemType);
	}

	public List<Item> GetEquippedItems()
	{
		return GetFilteredItems((Item i) => i.IsEquipped);
	}

	public List<Item> GetUsableItems()
	{
		return GetFilteredItems((Item i) => i.UseAction != null);
	}

	public List<Item> GetUsableItemsOfType(ItemType itemType)
	{
		return GetFilteredItems((Item i) => i.UseAction != null && i.ItemType == itemType);
	}

	public bool HasEquippedItemWithAbility(ItemAbility ability)
	{
		Item[] items = _items;
		foreach (Item item in items)
		{
			if (item != null && item.IsEquipped && item.HasItemAbility(ability))
			{
				return true;
			}
		}
		return false;
	}

	public int GetNumberOfEquippedItemsWithAbility(ItemAbility ability)
	{
		return GetFilteredItems((Item i) => i.IsEquipped && i.HasItemAbility(ability)).Count;
	}

	public List<Item> GetUnidentifiedItems()
	{
		return GetFilteredItems((Item i) => !i.IsIdentified && !(i is Food));
	}

	public List<Item> GetAllUnequippedItems()
	{
		return GetFilteredItems((Item i) => !i.IsEquipped);
	}

	public Item GetEquippedOrRandomItemOfType(ItemType itemType)
	{
		List<Item> list = GetEquippedItemsOfType(itemType);
		if (list.Count == 0)
		{
			list = GetCarriedItemsOfType(itemType);
		}
		if (list.Count > 0)
		{
			return list[Game.Instance.Random(list.Count)];
		}
		return null;
	}

	public void IncrementItemOwnership(DungeonLevel dungeonLevel, Player grog)
	{
		foreach (Item item in GetInventory())
		{
			if (!item.IsIdentified)
			{
				item.TurnsOwned++;
				if (item.IsExciting)
				{
					dungeonLevel.Message(" feels excited about the " + item.ShortDescription + "!", more: true);
					item.IsKnownToBeExciting = true;
					Game.Instance.Grog.ResetAutomaticAction();
				}
				else if (item.IsReadyToBeIdentifiedOtherwise)
				{
					string shortDescription = item.ShortDescription;
					item.IsIdentified = true;
					string shortDescription2 = item.ShortDescription;
					dungeonLevel.Message(" suddenly identifies the " + shortDescription + " as a " + shortDescription2 + "!", more: true);
					Game.Instance.Grog.ResetAutomaticAction();
				}
			}
		}
	}

	public Item GetRandomItem(Func<Item, bool> isAppropriateItem)
	{
		List<Item> filteredItems = GetFilteredItems(isAppropriateItem);
		if (filteredItems.Count == 0)
		{
			return null;
		}
		return filteredItems[Game.Instance.Random(filteredItems.Count)];
	}
}
