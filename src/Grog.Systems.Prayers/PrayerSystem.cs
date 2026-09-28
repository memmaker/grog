using System.Collections.Generic;
using Grog.Dressings.Beings;
using Grog.Dressings.Items;
using Grog.Dressings.Items.Implementations.Food;
using Grog.Dungeons;
using Grog.Kernel;
using Grog.Kernel.Interactions;

namespace Grog.Systems.Prayers;

public static class PrayerSystem
{
	public static Interaction[] GetAvailablePrayers(DungeonLevel dungeonLevel, Player grog, int x, int y, bool supportedByAltar)
	{
		List<Prayer> list = new List<Prayer>();
		if (grog.IsSick != 0)
		{
			list.Add(new Prayer('a', "Health", dungeonLevel, grog, x, y, PrayForHealth, supportedByAltar));
		}
		if (grog.IsStunned != 0)
		{
			list.Add(new Prayer('b', "Stable mind", dungeonLevel, grog, x, y, PrayForStableMind, supportedByAltar));
		}
		if (grog.IsConfused != 0)
		{
			list.Add(new Prayer('c', "Clear mind", dungeonLevel, grog, x, y, PrayForClearMind, supportedByAltar));
		}
		if (grog.IsBlind != 0)
		{
			list.Add(new Prayer('d', "Good eyesight", dungeonLevel, grog, x, y, PrayForGoodEyesight, supportedByAltar));
		}
		if (grog.IsDrunk != 0)
		{
			list.Add(new Prayer('e', "Sobriety", dungeonLevel, grog, x, y, PrayForSobriety, supportedByAltar));
		}
		if (grog.IsDeaf != 0)
		{
			list.Add(new Prayer('f', "Good hearing", dungeonLevel, grog, x, y, PrayForGoodHearing, supportedByAltar));
		}
		if (grog.IsPoisoned != 0)
		{
			list.Add(new Prayer('g', "Cleansed blood", dungeonLevel, grog, x, y, PrayForCleansedBlood, supportedByAltar));
		}
		if (grog.HitPoints < grog.MaxHitPoints)
		{
			list.Add(new Prayer('h', "Healing", dungeonLevel, grog, x, y, PrayForHealing, supportedByAltar));
		}
		SatiationLevel satiationLevel = grog.SatiationLevel;
		if (satiationLevel == SatiationLevel.Starving || satiationLevel == SatiationLevel.VeryHungry || satiationLevel == SatiationLevel.Hungry)
		{
			list.Add(new Prayer('i', "Nourishment", dungeonLevel, grog, x, y, PrayForNourishment, supportedByAltar));
		}
		if (grog.Inventory.IsCarryingCursedItems)
		{
			list.Add(new Prayer('j', "Uncursing", dungeonLevel, grog, x, y, PrayForUncursing, supportedByAltar));
		}
		if (grog.IsFrozen != 0)
		{
			list.Add(new Prayer('k', "Warmth", dungeonLevel, grog, x, y, PrayForWarmth, supportedByAltar));
		}
		if (grog.NumberOfFreePrayers > grog.NumberOfPrayersUttered + 15 && !grog.IsChampion)
		{
			list.Add(new Prayer('x', "Enlightenment", dungeonLevel, grog, x, y, PrayForEnlightenment, supportedByAltar));
		}
		if (grog.NumberOfFreePrayers > grog.NumberOfPrayersUttered + 2 && grog.IsChampion)
		{
			list.Add(new Prayer('m', "Might", dungeonLevel, grog, x, y, PrayForMight, supportedByAltar));
		}
		return list.ToArray();
	}

	private static bool PrayForWarmth(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		dungeonLevel.Message(" is warmed by a holy light filling the area!", more: true);
		grog.IsFrozen = 0;
		return true;
	}

	private static bool PrayForUncursing(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		foreach (Item item in grog.Inventory.GetInventory())
		{
			if (item.IsCursed)
			{
				item.IsCursed = false;
				dungeonLevel.Message("The " + item.ShortDescription + " glows in a silvery light.", more: true);
				item.IsCursedStatusKnown = true;
			}
		}
		return true;
	}

	private static bool PrayForHealth(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		dungeonLevel.Message(" is cured by a soothing light filling the room!", more: true);
		grog.IsSick = 0;
		return true;
	}

	private static bool PrayForStableMind(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		dungeonLevel.Message("'s mind clears up and is filled with a very strong resolve!", more: true);
		grog.IsStunned = 0;
		return true;
	}

	private static bool PrayForClearMind(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		dungeonLevel.Message("'s thoughts are cleared by a bright light lifting your soul!", more: true);
		grog.IsConfused = 0;
		return true;
	}

	private static bool PrayForGoodEyesight(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		dungeonLevel.Message("'s vision is restored by divine blessings!", more: true);
		grog.IsBlind = 0;
		return true;
	}

	private static bool PrayForSobriety(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		dungeonLevel.Message(" feels sober again!", more: true);
		grog.IsDrunk = 0;
		return true;
	}

	private static bool PrayForGoodHearing(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		dungeonLevel.Message("'s hearing is restored by divine blessings!", more: true);
		grog.IsDeaf = 0;
		return true;
	}

	private static bool PrayForCleansedBlood(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		dungeonLevel.Message("'s body is cleansed by a bright holy light emanating from the ceiling!", more: true);
		grog.IsPoisoned = 0;
		return true;
	}

	private static bool PrayForHealing(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		dungeonLevel.Message(" is healed by a holy ray of light emanating from the ceiling!", more: true);
		grog.HitPoints = grog.MaxHitPoints;
		return true;
	}

	private static bool PrayForEnlightenment(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		dungeonLevel.Message("A thundering voice suddenly shakes the dungeon itself!", more: true);
		dungeonLevel.Say("Thou shalt be my champion!");
		dungeonLevel.More();
		grog.IsChampion = true;
		grog.Strength += 3;
		grog.Dexterity += 3;
		grog.Constitution += 3;
		grog.MaxHitPoints += 20 + grog.Level * 2;
		grog.HitPoints += 20 + grog.Level * 2;
		return true;
	}

	private static bool PrayForMight(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		dungeonLevel.Message(" is permanently strengthened by divine might!", more: true);
		if (Game.Instance.Probability(50))
		{
			grog.MaxHitPoints += grog.Level + 10;
			grog.HitPoints += grog.Level + 10;
		}
		else
		{
			grog.Strength += 2;
			grog.Dexterity += 2;
			grog.Constitution += 2;
		}
		return true;
	}

	private static bool PrayForNourishment(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		dungeonLevel.Message(" is satiated by divine mana!", more: true);
		grog.Satiation = 1600;
		return true;
	}
}
