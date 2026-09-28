using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Dressings.Items.Implementations;

[Serializable]
public class FoodImplementations
{
	private static readonly string[] FortuneCookie = new string[22]
	{
		"They say that revenge comes in many forms and shapes!", "They say that repeated failure can increase your plight greatly!", "They say that doom might prolong your plight eternally!", "They say that the spirits of the dead corrupt anything they carry with them for extended periods of time!", "They say that the dungeon master takes precautions to improve his wards and guards over time.", "They say that knowledge comes with persistence.", "They say that not all scrolls are meant to be read.", "They say that excitement quite often is warranted.", "Note: Think of something clever to write here.", "They say that not all passages are obvious to the eye.",
		"They say that immortality can take many forms.", "They say that immortality might be guarded with jealousy.", "They say that all fortune cookies contain lies.", "They say that all fortune cookies are true.", "They say that using hardware is the best way to understand its function.", "They say that curses break your will!", "They say named foes are far more dangerous than all others.", "They say far too many things.", "They say that higher order metals are very desirable.", "They say that names are powers. As is gender and profession.",
		"42", "Die fast, die hard, return harder."
	};

	public bool UseIronRation(DungeonLevel dungeonLevel, Being user, Being target, Item item)
	{
		if (user == target && user is Player player)
		{
			dungeonLevel.Say("A bit stale but very nourishing!");
			player.Satiation += 500;
			player.Moves++;
		}
		return true;
	}

	public bool UseApple(DungeonLevel dungeonLevel, Being user, Being target, Item item)
	{
		if (user == target && user is Player player)
		{
			dungeonLevel.Say("A refreshing snack!");
			player.Satiation += 100;
			player.Moves++;
		}
		return true;
	}

	public bool UseSlimeMold(DungeonLevel dungeonLevel, Being user, Being target, Item item)
	{
		if (user == target && user is Player player)
		{
			dungeonLevel.Say("Slimy - but nonetheless fulfilling!");
			player.Satiation += 250;
			player.Moves++;
		}
		return true;
	}

	public bool UseFortuneCookie(DungeonLevel dungeonLevel, Being user, Being target, Item item)
	{
		if (user == target && user is Player player)
		{
			dungeonLevel.Say("A crunchy snack!", more: true);
			player.Satiation += 100;
			dungeonLevel.Message("It contains a scrap of paper which reads:", more: true);
			dungeonLevel.Say(FortuneCookie[Game.Instance.Random(FortuneCookie.Length)]);
			player.Moves++;
		}
		return true;
	}
}
