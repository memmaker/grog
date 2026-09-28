using System;
using System.Collections.Generic;
using Grog.Dressings.Beings;
using Grog.Dressings.Items;
using Grog.Dressings.Items.Inventory;
using Grog.Dungeons;
using Grog.Kernel;
using Grog.Kernel.Interactions;
using Grog.Systems.Prayers;

namespace Grog.Dressings.Features;

[Serializable]
public class AltarFeature : Feature
{
	[Serializable]
	public class DetectItemStatusItemProcessor : IItemProcessor
	{
		public Item Process(DungeonLevel dungeonLevel, Player grog, Item item)
		{
			if (item.IsCursed && !item.IsCursedStatusKnown)
			{
				dungeonLevel.Message("The " + item.ShortDescription + " emits a black glow!", more: true);
			}
			item.IsCursedStatusKnown = true;
			return item;
		}
	}

	[Serializable]
	public class InteractWithAltarInteractiveInteraction : IInteractionInteraction
	{
		public void Interact(DungeonLevel dungeonLevel, Player grog, int x, int y)
		{
			List<Interaction> list = new List<Interaction>();
			if (PrayerSystem.GetAvailablePrayers(dungeonLevel, grog, x, y, supportedByAltar: true).Length != 0)
			{
				list.Add(new Interaction('p', "Pray for help", dungeonLevel, grog, x, y, (DungeonLevel dl, Player g, int xp, int yp) => PrayForHelp(dl, g, xp, yp, supportedByAltar: true).Item2));
			}
			if (grog.Inventory.GetAllUnequippedItems().Count > 0)
			{
				list.Add(new Interaction('s', "Sacrifice items", dungeonLevel, grog, x, y, Sacrifice));
			}
			if (grog.Gold > 0)
			{
				list.Add(new Interaction('$', "Sacrifice gold", dungeonLevel, grog, x, y, SacrificeGold));
			}
			list.Add(new Interaction('d', "Defile the altar", dungeonLevel, grog, x, y, DefileAltar));
			list.Add(new Interaction('b', "Break the altar", dungeonLevel, grog, x, y, BreakTheAltar));
			list.Add(new Interaction('l', "Leave", dungeonLevel, grog, x, y, Leave));
			dungeonLevel.InitiateComplexInteraction("You stand in front of an ancient altar of Ta'ker'na, black god of the depthless dungeons.", list.ToArray());
		}
	}

	[Serializable]
	public class EncounterAltarBeingInteraction : IBeingInteraction
	{
		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			if (being is Player)
			{
				dungeonLevel.Message(" encounters an altar of Ta'ker'na, the black god of the depthless dungeons.");
			}
			return true;
		}
	}

	private const string _altarInteractionText = "You stand in front of an ancient altar of Ta'ker'na, black god of the depthless dungeons.";

	public AltarFeature(DungeonLevel dungeonLevel)
		: base('_', new InteractWithAltarInteractiveInteraction(), new EncounterAltarBeingInteraction(), null, new DetectItemStatusItemProcessor())
	{
	}

	private static bool SacrificeGold(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		int gold = dungeonLevel.GetGold("sacrifice");
		if (gold > 0)
		{
			dungeonLevel.Message(" sacrifices " + gold + " gold piece" + ((gold != 1) ? "s" : "") + ".", more: true);
			grog.Gold -= gold;
			grog.GainPiety(dungeonLevel, gold);
		}
		return false;
	}

	public static (bool, bool) PrayForHelp(DungeonLevel dungeonLevel, Player grog, int x, int y, bool supportedByAltar)
	{
		Interaction[] availablePrayers = PrayerSystem.GetAvailablePrayers(dungeonLevel, grog, x, y, supportedByAltar);
		if (availablePrayers.Length == 0)
		{
			dungeonLevel.Message(" prays in silent contemplation.");
			grog.Moves++;
			return (false, false);
		}
		return (dungeonLevel.InitiateComplexInteraction("What do you want to pray for?", availablePrayers), !grog.IsAlive);
	}

	private static bool Sacrifice(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		List<Item> allUnequippedItems = grog.Inventory.GetAllUnequippedItems();
		if (allUnequippedItems.Count == 0)
		{
			dungeonLevel.Message(" does not own any suitable items.");
			return false;
		}
		ItemSelectionList list = new ItemSelectionList(allUnequippedItems, useAssociatedItemCharacters: true);
		dungeonLevel.ProcessItemSelectionList(() => GetSacrificableInfos(list), list, (ItemSelectionList l, char c, bool ism) => ProcessSacrificeKey(dungeonLevel, grog, l, c, ism));
		return !grog.IsAlive;
	}

	private static List<string> GetSacrificableInfos(ItemSelectionList list)
	{
		return new List<string> { "[" + list.AssociatedKeys + "] Sacrifice item" };
	}

	private static bool ProcessSacrificeKey(DungeonLevel dungeonLevel, Player grog, ItemSelectionList list, char c, bool isCtrlModified)
	{
		if (isCtrlModified)
		{
			return false;
		}
		Item itemAssociatedWith = list.GetItemAssociatedWith(c);
		if (itemAssociatedWith == null)
		{
			return false;
		}
		dungeonLevel.Message("The " + itemAssociatedWith.ShortDescription + " is annihilated in a column of flames!", more: true);
		grog.GainPiety(dungeonLevel, itemAssociatedWith.Value);
		grog.Inventory.Remove(itemAssociatedWith);
		return false;
	}

	private static bool DefileAltar(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		dungeonLevel.Message("*NOW* " + grog.Name + " has the attention of Ta'ker'na!", more: true);
		ProcessAltarDestruction(dungeonLevel, grog, x, y);
		return !grog.IsAlive;
	}

	private static bool BreakTheAltar(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		if (grog.Strength < 18)
		{
			dungeonLevel.Message(" is too weak to harm Ta'ker'nas altar...", more: true);
		}
		else
		{
			dungeonLevel.Message(" smashes Ta'ker'nas altar to pieces!", more: true);
			ProcessAltarDestruction(dungeonLevel, grog, x, y);
			if (dungeonLevel.GetFeatureAt(x, y) == null)
			{
				dungeonLevel.Message("Earlier gifts to Ta'ker'na drop from the altar holding box to the ground!", more: true);
				int num = 1;
				for (int i = 0; i < dungeonLevel.Level; i++)
				{
					if (Game.Instance.Probability(50))
					{
						num++;
					}
				}
				for (int j = 0; j < num; j++)
				{
					Item item = Game.Instance.ItemPool.CreateRandomItem(dungeonLevel);
					dungeonLevel.AddItem(x, y, item);
				}
			}
		}
		return !grog.IsAlive;
	}

	private static void ProcessAltarDestruction(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		dungeonLevel.ClearFeatureAt(x, y);
		if (grog.IsChampion)
		{
			dungeonLevel.Message(" no longer champions Ta'ker'na!", more: true);
			grog.IsChampion = false;
			grog.DrainStrength(dungeonLevel, null, 3);
			grog.DrainDexterity(dungeonLevel, null, 3);
			grog.DrainConstitution(dungeonLevel, null, 3);
		}
		if (grog.HasGainedImmortality)
		{
			dungeonLevel.Message("Ta'ker'nas wrath is repelled by the immortal essence of " + grog.Name + "!");
			grog.GainExperience(dungeonLevel, 200);
			return;
		}
		if (grog.Piety > 0)
		{
			dungeonLevel.Message("Ta'ker'na withdraws his favor from " + grog.Name + "!", more: true);
			grog.Piety = 0L;
		}
		if (dungeonLevel.SummonMonsters(dungeonLevel, x, y, 5, 50, () => SummonWrathMonster(dungeonLevel)) > 0)
		{
			dungeonLevel.Message("Ta'ker'na retaliates by summoning unspeakable horrors upon " + grog.Name + "!", more: true);
		}
		else
		{
			dungeonLevel.Message("Ta'ker'nas ire causes the earth to shake... and then its ire seems to abide.", more: true);
		}
	}

	private static Being SummonWrathMonster(DungeonLevel dungeonLevel)
	{
		Being being = Game.Instance.MonsterPool.GetAvailableMonsters(dungeonLevel).CreateNewMonster(dungeonLevel);
		being.Mood = Mood.Hunting;
		return being;
	}

	private static bool Leave(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		return true;
	}
}
