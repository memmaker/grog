using System;
using System.Collections.Generic;
using System.Linq;
using Grog.Dressings.Beings;
using Grog.Dressings.Features;
using Grog.Dressings.Items.Types;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Monsters;
using Grog.Dungeons.Generators.Rooms;
using Grog.Kernel;
using Grog.Kernel.Interactions;

namespace Grog.Dressings.Items.Implementations;

[Serializable]
public class ScrollImplementations
{
	public bool UseScrollOfMonsterConfusion(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			dungeonLevel.Message("'s hands start to glow in a deep red color!");
			player.HandsOfConfusion = true;
			Game.Instance.ItemPool.Identify(scroll);
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfMagicMapping(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			if (dungeonLevel.MapLevel())
			{
				dungeonLevel.Message(" gains fresh insights about this dungeon!");
				Game.Instance.ItemPool.Identify(scroll);
			}
			else
			{
				dungeonLevel.Message(" feels a slight headache coming up.");
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfHoldMonster(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			foreach (Being item in dungeonLevel.GetAllBeingsVisibleTo(user))
			{
				if (!item.ResistsParalyzation)
				{
					if (item.IsParalyzed == 0)
					{
						dungeonLevel.Message(item, " stops moving!");
						Game.Instance.ItemPool.Identify(scroll);
					}
					if (item.IsParalyzed >= 0)
					{
						item.IsParalyzed += new Roll(1, 6).GetDieResult();
					}
				}
				else
				{
					dungeonLevel.Message("'s legs feel stiff for a brief moment.");
				}
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfSleep(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			if (!player.ResistsSleep)
			{
				dungeonLevel.Message(" suddenly falls asleeeezzzzz...zzzz...zzzzzzz!");
				player.IsSleeping += new Roll(2, 4).GetDieResult();
				Game.Instance.ItemPool.Identify(scroll);
			}
			else
			{
				dungeonLevel.Message("'s eyes itch for a brief moment.");
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfEnchantArmor(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			List<Item> equippedItemsOfType = player.Inventory.GetEquippedItemsOfType(ItemType.Armor);
			if (equippedItemsOfType.Count == 0)
			{
				dungeonLevel.Message("The scroll crumples to dust!");
			}
			else
			{
				if (equippedItemsOfType.Count > 1)
				{
					throw new GrogException("More than one piece of armor never should be equipped!");
				}
				dungeonLevel.Message(equippedItemsOfType[0].Description + " glows silvery for a moment!");
				equippedItemsOfType[0].AcBonus++;
				Game.Instance.ItemPool.Identify(scroll);
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfIdentify(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			List<Item> unidentifiedItems = player.Inventory.GetUnidentifiedItems();
			if (unidentifiedItems.Count == 0)
			{
				dungeonLevel.Message("The scroll crumples to ashes!");
			}
			else
			{
				Item item = unidentifiedItems[Game.Instance.Random(unidentifiedItems.Count)];
				string text = " identifies the " + item.Name + " as ";
				Game.Instance.ItemPool.Identify(item);
				text = text + item.a() + ".";
				dungeonLevel.Message(text);
				Game.Instance.ItemPool.Identify(scroll);
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfGreaterIdentify(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			List<Item> unidentifiedItems = player.Inventory.GetUnidentifiedItems();
			if (unidentifiedItems.Count == 0)
			{
				dungeonLevel.Message("The scroll burns to ashes!");
			}
			else
			{
				HashSet<ItemType> hashSet = new HashSet<ItemType>();
				Dictionary<ItemType, int> dictionary = new Dictionary<ItemType, int>();
				foreach (Item item in unidentifiedItems)
				{
					hashSet.Add(item.ItemType);
					if (dictionary.ContainsKey(item.ItemType))
					{
						dictionary[item.ItemType]++;
					}
					else
					{
						dictionary[item.ItemType] = 1;
					}
				}
				ItemType selectedType = ItemType.None;
				if (hashSet.Count == 1)
				{
					selectedType = hashSet.Single();
				}
				else
				{
					List<Interaction> list = new List<Interaction>();
					int num = 0;
					foreach (ItemType itemType in hashSet)
					{
						list.Add(new Interaction(97 + num, "Identify all " + Game.Instance.ItemPool.GetPluralItemTypeName(itemType) + " (" + dictionary[itemType] + ")", dungeonLevel, player, player.X, player.Y, (DungeonLevel dl, Player g, int xp, int yp) =>
						{
							selectedType = itemType;
							return true;
						}));
						num++;
					}
					dungeonLevel.InitiateComplexInteraction("Select an item type to identify:", list);
				}
				if (selectedType != ItemType.None)
				{
					dungeonLevel.Message("All " + Game.Instance.ItemPool.GetPluralItemTypeName(selectedType) + " are identified!");
					foreach (Item item2 in unidentifiedItems)
					{
						if (item2.ItemType == selectedType)
						{
							Game.Instance.ItemPool.Identify(item2);
						}
					}
				}
				else
				{
					dungeonLevel.Message("The " + scroll.ShortDescription + " turns to ash.");
				}
				Game.Instance.ItemPool.Identify(scroll);
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfUltimateIdentify(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			List<Item> unidentifiedItems = player.Inventory.GetUnidentifiedItems();
			if (unidentifiedItems.Count == 0)
			{
				dungeonLevel.Message("The scroll explodes and then burns to ashes!");
			}
			else
			{
				dungeonLevel.Message(Game.Instance.GetSpelledOutNumber(unidentifiedItems.Count) + " inventory items suddenly are known to " + player.Name + "!");
				foreach (Item item in unidentifiedItems)
				{
					Game.Instance.ItemPool.Identify(item);
				}
				Game.Instance.ItemPool.Identify(scroll);
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfScareMonster(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			if (player.IsDeaf != 0)
			{
				dungeonLevel.Message(" hears maniacal laughter in the distance!");
			}
			else
			{
				dungeonLevel.Message("Nothing seems to happen.");
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfFoodDetection(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			if (dungeonLevel.MapLevel((int x, int y) => dungeonLevel.GetItemsOfTypeAt(x, y, ItemType.Food).Count > 0, (int x, int y) => dungeonLevel.GetItemsOfTypeAt(x, y, ItemType.Food)[0].Character))
			{
				dungeonLevel.Message(" suddenly smells tasty food!");
				Game.Instance.ItemPool.Identify(scroll);
			}
			else
			{
				dungeonLevel.Message("'s stomach cringes with pain!");
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfStairDetection(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			if (dungeonLevel.MapLevel((int x, int y) => dungeonLevel.GetTileAt(x, y) == Tile.StairDown || dungeonLevel.GetTileAt(x, y) == Tile.StairUp, (int x, int y) => dungeonLevel.GetTileAt(x, y).Character(dungeonLevel, x, y)))
			{
				dungeonLevel.Message(" suddenly sense new venues!");
				Game.Instance.ItemPool.Identify(scroll);
			}
			else
			{
				dungeonLevel.Message(" fails to discern new options!");
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfSecretDoorDetection(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			if (dungeonLevel.MapLevel((int x, int y) => dungeonLevel.GetTileAt(x, y) == Tile.SecretDoor, (int x, int y) => 'S'))
			{
				dungeonLevel.Message(" unearths hidden secrets!");
				Game.Instance.ItemPool.Identify(scroll);
			}
			else
			{
				dungeonLevel.Message(" fails to learn any new insights!");
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfTeleportation(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			Position position = dungeonLevel.FindSuitableTeleportationTargetPosition();
			if (position.IsUndefined)
			{
				dungeonLevel.Message(" shudders for a brief moment...");
			}
			else
			{
				dungeonLevel.MoveThing(player, player.X, player.Y, position.X, position.Y, forceMovement: true);
				Game.Instance.ItemPool.Identify(scroll);
				dungeonLevel.Message(" suddenly appears elsewhere!");
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfEnchantWeapon(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			List<Item> equippedItemsOfType = player.Inventory.GetEquippedItemsOfType(ItemType.MeleeWeapon);
			if (equippedItemsOfType.Count == 0)
			{
				dungeonLevel.Message("The scroll bursts in flames!");
			}
			else
			{
				Item item = equippedItemsOfType[Game.Instance.Random(equippedItemsOfType.Count)];
				dungeonLevel.Message("'s " + item.ShortDescription + " glows blue for a moment!");
				item.MeleeDamage = new Roll(item.MeleeDamage.NumberOfDice, item.MeleeDamage.DieSides + 1, item.MeleeDamage.DieBonus + 1);
				Game.Instance.ItemPool.Identify(scroll);
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfMonsterCreation(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			Position position = dungeonLevel.FindPositionForMonsterCreationAround(player);
			if (position.IsUndefined)
			{
				dungeonLevel.Message("'s neck hair tingles for a brief moment.");
			}
			else
			{
				Being being = Game.Instance.MonsterPool.GetAvailableMonsters(dungeonLevel).CreateNewMonster(dungeonLevel);
				being.Mood = Mood.Hunting;
				dungeonLevel.SetBeing(position.X, position.Y, being);
				Game.Instance.ItemPool.Identify(scroll);
				dungeonLevel.Message("Suddenly an angry " + being.Name + " appears besides " + player.Name + "!");
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfMonsterAggravation(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			if (dungeonLevel.AggravateMonsters(player))
			{
				Game.Instance.ItemPool.Identify(scroll);
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfHordeSummoning(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			IRoom room = dungeonLevel.FindEmptyRoom();
			if (room == null)
			{
				dungeonLevel.Message("The scroll is ripped apart by a sudden gust of wind!");
			}
			else
			{
				foreach (Position insidePosition in room.GetInsidePositions())
				{
					Being being = Game.Instance.MonsterPool.GetAvailableMonsters(dungeonLevel).CreateNewMonster(dungeonLevel);
					being.Mood = Mood.Ambushing;
					dungeonLevel.SetBeing(insidePosition.X, insidePosition.Y, being);
				}
				dungeonLevel.Message(" feels as if in serious trouble!");
				Game.Instance.ItemPool.Identify(scroll);
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfAnnihilation(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			List<Item> equippedItems = player.Inventory.GetEquippedItems();
			if (equippedItems.Count == 0)
			{
				dungeonLevel.Message("The scroll suddenly transforms into a heap of maggots! *Yikes*!");
			}
			else
			{
				Item item = equippedItems[Game.Instance.Random(equippedItems.Count)];
				dungeonLevel.Message("'s " + item.ShortDescription + " turns to dust!");
				player.Inventory.Remove(item);
				Game.Instance.ItemPool.Identify(scroll);
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseTreasureMap(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			Position position = dungeonLevel.FindSuitableTeleportationTargetPosition();
			Game.Instance.ItemPool.Identify(scroll);
			if (position.IsUndefined)
			{
				dungeonLevel.Message("This seems to be an ancient treasure map! Sadly a vital part showing the actual treasure is missing...");
			}
			else
			{
				dungeonLevel.Message("This seems to be an ancient treasure map! It points to a treasure on this dungeon level!");
				dungeonLevel.SetFeatureAt(position.X, position.Y, new TreasureFeature());
				dungeonLevel.MemorizePosition(position.X, position.Y);
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfBlindness(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			Game.Instance.ItemPool.Identify(scroll);
			dungeonLevel.Message(" suddenly can't see anything!");
			player.IsBlind += new Roll(4, 6).GetDieResult();
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfInvisibility(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			if (player.IsInvisible)
			{
				dungeonLevel.Message(" feels dizzy for a brief moment!");
			}
			else
			{
				dungeonLevel.Message(" no longer can be seen!");
				Game.Instance.ItemPool.Identify(scroll);
			}
			if (player.TurnsOfInvisibility >= 0)
			{
				player.TurnsOfInvisibility += new Roll(10, 4).GetDieResult();
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseHolyScriptures(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			dungeonLevel.Message(" chants the holy scriptures contained in this scroll!");
			Game.Instance.ItemPool.Identify(scroll);
			foreach (Being item in dungeonLevel.GetAllBeingsVisibleTo(player))
			{
				if (item.Race == Race.Undead && item.MonsterType != MonsterPool.MonsterType.Lich)
				{
					dungeonLevel.Message(item, " turns to dust!");
					dungeonLevel.RemoveThing(item.X, item.Y);
				}
			}
			player.Moves++;
		}
		return true;
	}

	private bool CannotRead(DungeonLevel dungeonLevel, Being being)
	{
		if (being is Player player)
		{
			if (player.IsBlind != 0)
			{
				dungeonLevel.Message(" can't read while blind...");
				return true;
			}
			return false;
		}
		return true;
	}

	public bool UseScrollOfPrecision(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			Item equippedOrRandomItemOfType = player.Inventory.GetEquippedOrRandomItemOfType(ItemType.MeleeWeapon);
			if (equippedOrRandomItemOfType == null)
			{
				dungeonLevel.Message("The scroll bursts in flames!");
			}
			else
			{
				dungeonLevel.Message("'s " + equippedOrRandomItemOfType.ShortDescription + " glows green for a moment!");
				equippedOrRandomItemOfType.ToHitBonus++;
				Game.Instance.ItemPool.Identify(scroll);
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfPain(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			Item equippedOrRandomItemOfType = player.Inventory.GetEquippedOrRandomItemOfType(ItemType.MeleeWeapon);
			if (equippedOrRandomItemOfType == null)
			{
				dungeonLevel.Message("The scroll bursts in flames!");
			}
			else
			{
				dungeonLevel.Message("'s " + equippedOrRandomItemOfType.ShortDescription + " glows red for a moment!");
				equippedOrRandomItemOfType.MeleeDamage = new Roll(equippedOrRandomItemOfType.MeleeDamage.NumberOfDice, equippedOrRandomItemOfType.MeleeDamage.DieSides, equippedOrRandomItemOfType.MeleeDamage.DieBonus + 2);
				Game.Instance.ItemPool.Identify(scroll);
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfProtection(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			ItemType[] array = ((!Game.Instance.Probability(50)) ? new ItemType[2]
			{
				ItemType.Shield,
				ItemType.Armor
			} : new ItemType[2]
			{
				ItemType.Armor,
				ItemType.Shield
			});
			foreach (ItemType itemType in array)
			{
				Item equippedOrRandomItemOfType = player.Inventory.GetEquippedOrRandomItemOfType(itemType);
				if (equippedOrRandomItemOfType != null)
				{
					dungeonLevel.Message("'s " + equippedOrRandomItemOfType.ShortDescription + " glows blue for a moment!");
					equippedOrRandomItemOfType.AcBonus++;
					Game.Instance.ItemPool.Identify(scroll);
					player.Moves++;
					return true;
				}
			}
			player.Moves++;
			dungeonLevel.Message("The scroll bursts in flames!");
		}
		return true;
	}

	public bool UseScrollOfMight(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			Item equippedOrRandomItemOfType = player.Inventory.GetEquippedOrRandomItemOfType(ItemType.MeleeWeapon);
			if (equippedOrRandomItemOfType == null)
			{
				dungeonLevel.Message("The scroll bursts in flames!");
			}
			else
			{
				dungeonLevel.Message("'s " + equippedOrRandomItemOfType.ShortDescription + " glows fiery red for a moment!");
				equippedOrRandomItemOfType.ToHitBonus += 2;
				equippedOrRandomItemOfType.MeleeDamage = new Roll(equippedOrRandomItemOfType.MeleeDamage.NumberOfDice, equippedOrRandomItemOfType.MeleeDamage.DieSides, equippedOrRandomItemOfType.MeleeDamage.DieBonus + 2);
				Game.Instance.ItemPool.Identify(scroll);
			}
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfPiousLessons(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			dungeonLevel.Message(" studies the pious lessings of Ta'ker'na!", more: true);
			player.GainPiety(dungeonLevel, dungeonLevel.Level * 500);
			player.Moves++;
		}
		return true;
	}

	public bool UseScrollOfMostHolyPreachings(DungeonLevel dungeonLevel, Being applier, Being user, Item scroll)
	{
		if (CannotRead(dungeonLevel, applier))
		{
			return false;
		}
		if (user is Player player)
		{
			dungeonLevel.Message(" studies the most holy preachings of Ta'ker'na!", more: true);
			int numberOfFreePrayers = player.NumberOfFreePrayers;
			player.NumberOfFreePrayers += Math.Max(dungeonLevel.Level / 4, 2);
			int numberOfFreePrayers2 = player.NumberOfFreePrayers;
			player.JudgePiety(dungeonLevel, numberOfFreePrayers, numberOfFreePrayers2);
			player.Moves++;
		}
		return true;
	}
}
