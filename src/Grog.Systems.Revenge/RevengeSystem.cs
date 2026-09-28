using System;
using System.Collections.Generic;
using Grog.Dressings.Beings;
using Grog.Dressings.Items;
using Grog.Dressings.MapElements;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Monsters;
using Grog.Kernel;
using Grog.Kernel.FileAccess;
using Grog.Kernel.GCurses;
using Grog.Systems.Revenge.NameGenerators;

namespace Grog.Systems.Revenge;

[Serializable]
public class RevengeSystem
{
	[Serializable]
	private class MinorIcyAuraInteraction : IMapElementInteraction
	{
		public void Interact(DungeonLevel dungeonLevel, Player grog, int x, int y, IMapElement element, InteractionContext context)
		{
			if (element is Being being && !grog.Inventory.HasEquippedItemWithAbility(ItemAbility.ResistCold))
			{
				int total = Game.Instance.Roll(1, 2);
				dungeonLevel.Message(" is frozen to the bone by the chilling aura of " + being.Name + " for " + total + " damage!");
				grog.SufferDamage(dungeonLevel, being, total, "by being frozen to death by a chilling aura");
			}
		}
	}

	[Serializable]
	private class MajorIcyAuraInteraction : IMapElementInteraction
	{
		public void Interact(DungeonLevel dungeonLevel, Player grog, int x, int y, IMapElement element, InteractionContext context)
		{
			if (element is Being being && !grog.Inventory.HasEquippedItemWithAbility(ItemAbility.ResistCold))
			{
				int total = Game.Instance.Roll(1, 4, 2);
				dungeonLevel.Message(" is badly frozen by the icy aura of " + being.Name + " for " + total + " damage!");
				grog.SufferDamage(dungeonLevel, being, total, "by being frozen to death by an extremely chilling aura");
			}
		}
	}

	private static readonly Dictionary<RevengeMonsterProgression, RevengeMonsterProgressionTable> _revengeProgressions = new Dictionary<RevengeMonsterProgression, RevengeMonsterProgressionTable>
	{
		{
			RevengeMonsterProgression.LesserUndead,
			new RevengeMonsterProgressionTable(new CommonNameGenerator(), new RevengeLevelBonus(null, 10, 0, 1, 0, RevengeMonsterPower.None, 0, 1, 0, 0), new RevengeLevelBonus(null, 14, 1, 3, 0, RevengeMonsterPower.None, 0, 0, 1, 0), new RevengeLevelBonus(null, 12, 0, 1, 0, RevengeMonsterPower.Regeneration, 0, 0, 0, 0), new RevengeLevelBonus(null, 10, 1, 0, 0, RevengeMonsterPower.None, 0, 0, 0, 0), new RevengeLevelBonus(null, 8, 0, 1, 0, RevengeMonsterPower.None, 0, 2, 0, 0), new RevengeLevelBonus(null, 6, 1, 0, 0, RevengeMonsterPower.None, 0, 0, 0, 0), new RevengeLevelBonus("-frightening", 4, 0, 1, 0, RevengeMonsterPower.None, 0, 0, 0, 0), new RevengeLevelBonus("-chilling", 4, 0, 0, 0, RevengeMonsterPower.None, 0, 0, 3, 0, AddMinorIcyAura))
		},
		{
			RevengeMonsterProgression.GreaterUndead,
			new RevengeMonsterProgressionTable(new RegalNameGenerator(), new RevengeLevelBonus("overlord", 10, 1, 1, 0, RevengeMonsterPower.ParalyzeOnTouch, 0, 1, 0, 0), new RevengeLevelBonus("-greater", 18, 1, 4, 1, RevengeMonsterPower.Regeneration, 0, 0, 0, 0), new RevengeLevelBonus("-greater", 16, 0, 1, 0, RevengeMonsterPower.None, 0, 1, 0, 0), new RevengeLevelBonus("-greater", 14, 1, 0, 0, RevengeMonsterPower.None, 0, 0, 1, 0), new RevengeLevelBonus("-master", 12, 0, 1, 1, RevengeMonsterPower.DrainStrength, 0, 0, 0, 0), new RevengeLevelBonus("-master", 10, 2, 1, 0, RevengeMonsterPower.Freeze, 0, 1, 0, 0), new RevengeLevelBonus("lord", 8, 0, 0, 0, RevengeMonsterPower.None, 0, 0, 1, 0, AddMajorIcyAura), new RevengeLevelBonus("lord", 6, 2, 1, 1, RevengeMonsterPower.DrainConstitution, 0, 0, 0, 0), new RevengeLevelBonus("lord", 6, 0, 1, 0, RevengeMonsterPower.None, 1, 0, 0, 0))
		},
		{
			RevengeMonsterProgression.Warrior,
			new RevengeMonsterProgressionTable(new BruteNameGenerator(), new RevengeLevelBonus("warmaster", 12, 0, 1, 0, RevengeMonsterPower.SmashWeapon, 0, 1, 0, 0), new RevengeLevelBonus("grunt", 12, 1, 4, 1, RevengeMonsterPower.None, 0, 0, 1, 0), new RevengeLevelBonus("sergeant", 12, 1, 1, 1, RevengeMonsterPower.Disarm, 0, 0, 0, 0), new RevengeLevelBonus("chieftain", 12, 1, 1, 1, RevengeMonsterPower.None, 0, 0, 0, 0), new RevengeLevelBonus("hero", 12, 0, 1, 0, RevengeMonsterPower.SmashShield, 0, 1, 1, 0), new RevengeLevelBonus("weaponmaster", 12, 1, 1, 0, RevengeMonsterPower.None, 0, 1, 0, 1), new RevengeLevelBonus("general", 12, 1, 1, 0, RevengeMonsterPower.SmashArmor, 0, 2, 0, 0), new RevengeLevelBonus("warlord", 12, 1, 1, 0, RevengeMonsterPower.None, 1, 2, 1, 0))
		},
		{
			RevengeMonsterProgression.Sneak,
			new RevengeMonsterProgressionTable(new CommonNameGenerator(), new RevengeLevelBonus("assassin lord", 6, 1, 0, 0, RevengeMonsterPower.None, 0, 1, 0, 0), new RevengeLevelBonus("footpad", 2, 3, 3, 0, RevengeMonsterPower.MoveErratically, 0, 0, 0, 0), new RevengeLevelBonus("sneak", 4, 0, 2, 0, RevengeMonsterPower.None, 0, 1, 0, 0), new RevengeLevelBonus("rogue", 6, 2, 0, 0, RevengeMonsterPower.Poison, 0, 0, 0, 0), new RevengeLevelBonus("mugger", 6, 0, 1, 0, RevengeMonsterPower.None, 0, 2, 0, 0), new RevengeLevelBonus("robber", 6, 0, 0, 0, RevengeMonsterPower.None, 1, 0, 0, 0), new RevengeLevelBonus("assassin", 6, 1, 2, 0, RevengeMonsterPower.SummonAmbush, 0, 3, 0, 0))
		},
		{
			RevengeMonsterProgression.Beast,
			new RevengeMonsterProgressionTable(new BeastNameGenerator(), new RevengeLevelBonus("-raging", 12, 0, 2, 1, RevengeMonsterPower.None, 0, 1, 0, 0), new RevengeLevelBonus("-terrible", 20, 0, 4, 1, RevengeMonsterPower.None, 0, 0, 0, 0), new RevengeLevelBonus("-terrible", 18, 1, 2, 1, RevengeMonsterPower.None, 0, 1, 0, 0), new RevengeLevelBonus("-terrible", 16, 0, 3, 1, RevengeMonsterPower.None, 0, 0, 0, 0), new RevengeLevelBonus("-horrific", 14, 1, 4, 1, RevengeMonsterPower.None, 0, 1, 0, 0), new RevengeLevelBonus("-horrific", 12, 0, 2, 1, RevengeMonsterPower.None, 0, 0, 0, 0), new RevengeLevelBonus("-horrific", 12, 0, 2, 1, RevengeMonsterPower.None, 0, 1, 0, 0), new RevengeLevelBonus("-ravaging", 12, 0, 2, 1, RevengeMonsterPower.None, 0, 0, 0, 1), new RevengeLevelBonus("-ravaging", 12, 0, 2, 1, RevengeMonsterPower.None, 0, 1, 0, 0), new RevengeLevelBonus("-ravaging", 12, 0, 2, 1, RevengeMonsterPower.None, 0, 0, 0, 0), new RevengeLevelBonus("-raging", 12, 0, 2, 1, RevengeMonsterPower.None, 0, 1, 0, 1))
		},
		{
			RevengeMonsterProgression.Brute,
			new RevengeMonsterProgressionTable(new BruteNameGenerator(), new RevengeLevelBonus("-fiendish", 20, 0, 2, 3, RevengeMonsterPower.Regeneration, 0, 1, 0, 0), new RevengeLevelBonus("ruffian", 30, 0, 5, 1, RevengeMonsterPower.None, 0, 2, 0, 0), new RevengeLevelBonus("rowdy", 20, 0, 2, 1, RevengeMonsterPower.None, 0, 2, 0, 0), new RevengeLevelBonus("smasher", 10, 0, 2, 2, RevengeMonsterPower.SmashShield, 1, 2, 0, 0), new RevengeLevelBonus("berserker", 0, 0, 2, 2, RevengeMonsterPower.None, 0, 0, 0, 1), new RevengeLevelBonus("brute", 30, 0, 2, 3, RevengeMonsterPower.SmashArmor, 0, 2, 0, 0))
		}
	};

	private static readonly string[] SkullScene = new string[14]
	{
		"            ", "V          L", " \\/-------\\/ ", "  |       |  ", "  | o   o |  ", "  |       |  ", "  \\   |   /  ", "   |  -  |   ", "   |     |   ", "   | ||| |   ",
		"   |     |   ", "  /\\-----/\\ ", " S         F", "            "
	};

	private static readonly string[] UpliftScene = new string[5] { "/-------\\", "|.......|", "|.......+", "|.......|", "\\-------/" };

	[NonSerialized]
	private RevengeMonsters _revengeMonsters;

	private HashSet<RevengeMonster> _activeRevengeMonsters = new HashSet<RevengeMonster>();

	private static void AddMinorIcyAura(Being being)
	{
		being.AddAdjacentInteraction(new MinorIcyAuraInteraction());
	}

	private static void AddMajorIcyAura(Being being)
	{
		being.AddAdjacentInteraction(new MajorIcyAuraInteraction());
		being.AddDiagonalInteraction(new MajorIcyAuraInteraction());
	}

	public void UpliftRevengeMonster(DungeonLevel dungeonLevel, Being attacker, Being killed)
	{
		if (attacker == null || !attacker.IsAlive || attacker is Player || attacker.MonsterType == MonsterPool.MonsterType.Lich || attacker.RevengeMonsterProgression == RevengeMonsterProgression.None || !(killed is Player grog) || dungeonLevel.Level == 25)
		{
			return;
		}
		if (attacker is RevengeMonster revengeMonster)
		{
			_activeRevengeMonsters.Remove(revengeMonster);
			IncreaseRevengeMonsterLevel(dungeonLevel, revengeMonster, grog);
		}
		else if (Game.Instance.Probability(35) && LoadRevengeMonsters().Count <= 1000)
		{
			RevengeMonster revengeMonster2 = TurnMonsterIntoRevengeMonster(dungeonLevel, attacker, grog);
			if (revengeMonster2 != null)
			{
				SaveRevengeMonster(revengeMonster2);
			}
		}
	}

	private void IncreaseRevengeMonsterLevel(DungeonLevel dungeonLevel, RevengeMonster revengeMonster, Player grog)
	{
		dungeonLevel.ClearMessages();
		dungeonLevel.Message("'s lifeless body drops to the ground!", more: true);
		dungeonLevel.Message(revengeMonster.Name + " stands over it in triumph!", more: true);
		dungeonLevel.Message(revengeMonster.Name + " seems to grow even more powerful!", more: true);
		revengeMonster.RevengeLevel++;
		AdjustForRevengeLevel(revengeMonster);
	}

	private RevengeMonster TurnMonsterIntoRevengeMonster(DungeonLevel dungeonLevel, Being attacker, Player grog)
	{
		dungeonLevel.PrepareScene();
		int x = (dungeonLevel.Width - SkullScene[0].Length) / 2;
		int num = 4;
		for (int i = 0; i < SkullScene.Length; i++)
		{
			Curses.Instance.SetCursorPosition(x, num + i);
			Curses.Instance.Write(SkullScene[i]);
		}
		int sy = Curses.Instance.CursorY + 2;
		ShowSceneText(dungeonLevel, sy, "Your lifeless body slumps to the ground.", attacker.Name + " is engulfed by pitch black vapors and called to another place.");
		dungeonLevel.PrepareScene();
		x = (dungeonLevel.Width - UpliftScene[0].Length) / 2;
		num = 2 + (dungeonLevel.Height - UpliftScene.Length) / 3;
		for (int j = 0; j < UpliftScene.Length; j++)
		{
			Curses.Instance.SetCursorPosition(x, num + j);
			Curses.Instance.Write(UpliftScene[j]);
		}
		sy = Curses.Instance.CursorY + 2;
		Curses.Instance.SetCursorPosition(dungeonLevel.Width / 2 - 1, num + UpliftScene.Length / 2);
		Curses.Instance.InvertColors();
		Curses.Instance.Write(Game.Instance.MonsterPool.GetMonsterDefinition(MonsterPool.MonsterType.Lich).Template.Character(null, -1, -1));
		Curses.Instance.InvertColors();
		Curses.Instance.SetCursorPosition(dungeonLevel.Width / 2 + 1, num + UpliftScene.Length / 2);
		Curses.Instance.Write(attacker.Character(null, -1, -1));
		ShowSceneText(dungeonLevel, sy, "Kalmius the lich has summoned " + attacker.Name + "!", "His voice booms:", "\"Ye have slain a vile foe!\"");
		if (string.IsNullOrEmpty(attacker.ChristenedName))
		{
			string nameForRevengeMonster = GetNameForRevengeMonster(attacker);
			ShowSceneText(dungeonLevel, sy, "\"'" + nameForRevengeMonster + "' shall by thy name henceforth!\"", "\"Spread terror under this proud name - to increase my infamy!\"", "With an evil gesture Kalmius infuses " + nameForRevengeMonster + " with dark powers!");
			attacker.ChristenedName = nameForRevengeMonster;
		}
		RevengeMonster revengeMonster = new RevengeMonster(attacker);
		revengeMonster.RevengeLevel = 1;
		AdjustForRevengeLevel(revengeMonster);
		return revengeMonster;
	}

	private void AdjustForRevengeLevel(RevengeMonster revengeMonster)
	{
		RevengeLevelBonus revengeLevelBonusFor = _revengeProgressions[revengeMonster.RevengeMonsterProgression].GetRevengeLevelBonusFor(revengeMonster);
		revengeMonster.Title = revengeLevelBonusFor.Title;
		revengeMonster.HitPoints += revengeLevelBonusFor.HitPointBonus;
		revengeMonster.MaxHitPoints += revengeLevelBonusFor.HitPointBonus;
		revengeMonster.ArmorClass += revengeLevelBonusFor.ArmorClassBonus;
		revengeMonster.RevengeToHitBonus += revengeLevelBonusFor.ToHitBonus;
		revengeMonster.RevengeToDamageBonus += revengeLevelBonusFor.ToDamageBonus;
		if (revengeLevelBonusFor.RevengeMonsterPower != RevengeMonsterPower.None)
		{
			revengeMonster.AddRevengeMonsterPower(revengeLevelBonusFor.RevengeMonsterPower);
		}
		revengeMonster.NaturalDamage = revengeLevelBonusFor.GetModifiedNaturalDamage(revengeMonster.NaturalDamage);
		if (revengeLevelBonusFor.ModifyBeing != null)
		{
			revengeLevelBonusFor.ModifyBeing(revengeMonster);
		}
	}

	private string GetNameForRevengeMonster(Being attacker)
	{
		RevengeMonsterProgressionTable revengeMonsterProgressionTable = _revengeProgressions[attacker.RevengeMonsterProgression];
		HashSet<string> existingRevengeMonsterNames = GetExistingRevengeMonsterNames();
		string text;
		do
		{
			text = revengeMonsterProgressionTable.GetRandomRevengeMonsterName();
		}
		while (text != null && existingRevengeMonsterNames.Contains(text));
		if (text == null)
		{
			for (int i = 0; i < int.MaxValue; i++)
			{
				text = "Mortifer" + ((i == 0) ? "" : (" " + ToQuasiRoman(i)));
				if (!existingRevengeMonsterNames.Contains(text))
				{
					return text;
				}
			}
		}
		return text;
	}

	private string ToQuasiRoman(int number)
	{
		if (number < 1 || number > 3999)
		{
			return number.ToString();
		}
		if (number >= 1000)
		{
			return "M" + ToQuasiRoman(number - 1000);
		}
		if (number >= 900)
		{
			return "CM" + ToQuasiRoman(number - 900);
		}
		if (number >= 500)
		{
			return "D" + ToQuasiRoman(number - 500);
		}
		if (number >= 400)
		{
			return "CD" + ToQuasiRoman(number - 400);
		}
		if (number >= 100)
		{
			return "C" + ToQuasiRoman(number - 100);
		}
		if (number >= 90)
		{
			return "XC" + ToQuasiRoman(number - 90);
		}
		if (number >= 50)
		{
			return "L" + ToQuasiRoman(number - 50);
		}
		if (number >= 40)
		{
			return "XL" + ToQuasiRoman(number - 40);
		}
		if (number >= 10)
		{
			return "X" + ToQuasiRoman(number - 10);
		}
		if (number >= 9)
		{
			return "IX" + ToQuasiRoman(number - 9);
		}
		if (number >= 5)
		{
			return "V" + ToQuasiRoman(number - 5);
		}
		if (number >= 4)
		{
			return "IV" + ToQuasiRoman(number - 4);
		}
		return "I" + ToQuasiRoman(number - 1);
	}

	private HashSet<string> GetExistingRevengeMonsterNames()
	{
		return LoadRevengeMonsters().GetExistingRevengeMonsterNames();
	}

	private void ShowSceneText(DungeonLevel dungeonLevel, int sy, params string[] sceneText)
	{
		for (int i = sy; i < 2 + dungeonLevel.Height; i++)
		{
			Curses.Instance.SetCursorPosition(0, i);
			Curses.Instance.ClearToEndOfLine();
		}
		for (int j = 0; j < sceneText.Length; j++)
		{
			Curses.Instance.SetCursorPosition((Curses.Instance.WindowWidth - sceneText[j].Length) / 2, sy + j);
			Curses.Instance.WriteLine(char.ToUpper(sceneText[j][0]) + sceneText[j].Substring(1));
		}
		Curses.Instance.InvertColors();
		Curses.Instance.SetCursorPosition((Curses.Instance.WindowWidth - 10) / 2, Curses.Instance.CursorY);
		Curses.Instance.Write("---more---");
		Curses.Instance.InvertColors();
		Curses.Instance.ReadKey(showReadKey: false, showCursor: false);
	}

	private void SaveRevengeMonster(RevengeMonster revengeMonster)
	{
		revengeMonster.HitPoints = revengeMonster.MaxHitPoints;
		RevengeMonsters revengeMonsters = LoadRevengeMonsters();
		revengeMonsters.Add(revengeMonster);
		SaveRevengeMonsters(revengeMonsters);
	}

	private void SaveRevengeMonsters(RevengeMonsters revengeMonsters)
	{
		FileManager.WriteObject("grog_v1.rmf", revengeMonsters);
	}

	private RevengeMonsters LoadRevengeMonsters()
	{
		if (_revengeMonsters == null)
		{
			_revengeMonsters = FileManager.ReadObject("grog_v1.rmf", () => new RevengeMonsters());
		}
		return _revengeMonsters;
	}

	public RevengeMonster PullRevengeMonsterForType(DungeonLevel dungeonLevel, MonsterPool.MonsterType monsterType)
	{
		RevengeMonsters revengeMonsters = LoadRevengeMonsters();
		RevengeMonster revengeMonster = revengeMonsters.PullRevengeMonsterForMonsterType(dungeonLevel, monsterType);
		if (revengeMonster != null)
		{
			SaveRevengeMonsters(revengeMonsters);
			revengeMonster.Activate();
			_activeRevengeMonsters.Add(revengeMonster);
		}
		return revengeMonster;
	}

	public void StoreActiveRevengeMonstersForLater()
	{
		if (_activeRevengeMonsters.Count <= 0)
		{
			return;
		}
		RevengeMonsters revengeMonsters = LoadRevengeMonsters();
		foreach (RevengeMonster activeRevengeMonster in _activeRevengeMonsters)
		{
			if (activeRevengeMonster.IsAlive)
			{
				activeRevengeMonster.HitPoints = activeRevengeMonster.MaxHitPoints;
				revengeMonsters.Add(activeRevengeMonster);
			}
		}
		SaveRevengeMonsters(revengeMonsters);
		_activeRevengeMonsters.Clear();
	}

	public void MakeRevengeMonstersUniqueAgain(Dictionary<long, Being> beingsByUid)
	{
		HashSet<RevengeMonster> hashSet = new HashSet<RevengeMonster>();
		foreach (RevengeMonster activeRevengeMonster in _activeRevengeMonsters)
		{
			if (beingsByUid.ContainsKey(activeRevengeMonster.UID))
			{
				hashSet.Add((RevengeMonster)beingsByUid[activeRevengeMonster.UID]);
			}
			else
			{
				hashSet.Add(activeRevengeMonster);
			}
		}
		_activeRevengeMonsters = hashSet;
	}

	public int GetMaximumRevengeProgressionLevelFor(RevengeMonster rm)
	{
		return _revengeProgressions[rm.RevengeMonsterProgression].MaximumDynamicRevengeProgressionLevel;
	}

	public void AddInformation(List<string> information)
	{
		RevengeMonsters revengeMonsters = LoadRevengeMonsters();
		information.Add("Revenge monsters: " + revengeMonsters.Count);
	}
}
