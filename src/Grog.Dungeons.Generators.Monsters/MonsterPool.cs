using System;
using System.Collections.Generic;
using System.Text;
using Grog.Dressings.Beings;
using Grog.Kernel;
using Grog.Systems.Ghosts;
using Grog.Systems.Revenge;

namespace Grog.Dungeons.Generators.Monsters;

[Serializable]
public class MonsterPool
{
	public enum MonsterType
	{
		GiantAnt = 0,
		GiantBat = 1,
		Centaur = 2,
		DarkElf = 3,
		FloatingEye = 4,
		FireDrake = 5,
		Goblin = 6,
		HellHound = 7,
		Imp = 8,
		Jackalwere = 9,
		Kobold = 10,
		Leprechaun = 11,
		Manticore = 12,
		Nymph = 13,
		Orc = 14,
		PhaseSpider = 15,
		Quasit = 16,
		GiantRat = 17,
		GiantSpider = 18,
		Tiger = 19,
		UrVile = 20,
		Vampire = 21,
		WillOWisp = 22,
		Xorn = 23,
		YethHound = 24,
		Zombie = 25,
		Alp = 26,
		Balor = 27,
		Chimera = 28,
		Dragon = 29,
		Ettin = 30,
		Fomorian = 31,
		FireGiant = 32,
		Hydra = 33,
		Ifrit = 34,
		Jinn = 35,
		BlackKnight = 36,
		Lich = 37,
		Mummy = 38,
		Naga = 39,
		Ogre = 40,
		PurpleWorm = 41,
		Quinotaur = 42,
		RustMonster = 43,
		GiantScorpion = 44,
		Troll = 45,
		UmberHulk = 46,
		VampireLord = 47,
		BurrowWight = 48,
		Exterminator = 49,
		Yeti = 50,
		ZombieLord = 51,
		Doppelganger = 52,
		Ghost = 53,
		Grog = 54,
		MaxMonster = Grog
	}

	private static readonly int[] _killCount = new int[54];

	private readonly MonsterDefinition[] _monsters;

	private readonly Dictionary<MonsterType, MonsterDefinition> _definitions = new Dictionary<MonsterType, MonsterDefinition>();

	private readonly HashSet<MonsterType> _isArmorClassKnown = new HashSet<MonsterType>();

	private readonly HashSet<MonsterType> _isAttackRoutineKnown = new HashSet<MonsterType>();

	private readonly HashSet<MonsterType> _isDamageRoutineKnown = new HashSet<MonsterType>();

	private readonly HashSet<MonsterType> _areHitDiceKnown = new HashSet<MonsterType>();

	public MonsterPool()
	{
		_monsters = new MonsterDefinition[53]
		{
			new MonsterDefinition(100, new Being(MonsterType.Orc, Race.Humanoid, "", "orc", RevengeMonsterProgression.Warrior, 'o', 1, 15, 1, 8, 0, new Roll[1]
			{
				new Roll(1, 8)
			})),
			new MonsterDefinition(800, new Being(MonsterType.Goblin, Race.Humanoid, "", "goblin", RevengeMonsterProgression.Warrior, 'g', 1, 13, 1, 6, 0, new Roll[1]
			{
				new Roll(1, 6)
			})),
			new MonsterDefinition(500, new Being(MonsterType.GiantRat, Race.GiantAnimal, "", "giant rat", RevengeMonsterProgression.None, 'r', 1, 12, 1, 4, 0, new Roll[1]
			{
				new Roll(1, 4)
			}, SpecialAbility.CauseDisease, 7, 80)),
			new MonsterDefinition(300, new Being(MonsterType.GiantBat, Race.GiantAnimal, "", "giant bat", RevengeMonsterProgression.None, 'b', 1, 12, 1, 4, 0, new Roll[1]
			{
				new Roll(1, 3)
			})),
			new MonsterDefinition(400, new Being(MonsterType.Kobold, Race.Humanoid, "", "kobold", RevengeMonsterProgression.Sneak, 'k', 2, 13, 1, 4, 0, new Roll[1]
			{
				new Roll(1, 6)
			}, SpecialAbility.SummonAmbush, 3, 60)),
			new MonsterDefinition(400, new Being(MonsterType.Zombie, Race.Undead, "", "zombie", RevengeMonsterProgression.LesserUndead, 'z', 2, 14, 4, 8, 0, new Roll[1]
			{
				new Roll(1, 8)
			}, SpecialAbility.MovesSlowly)),
			new MonsterDefinition(400, new Being(MonsterType.Leprechaun, Race.Fairy, "", "leprechaun", RevengeMonsterProgression.Sneak, 'l', 3, 16, 2, 5, 2, new Roll[1]
			{
				new Roll(1, 3)
			}, SpecialAbility.StealGold, 20, 40)),
			new MonsterDefinition(400, new Being(MonsterType.Quasit, Race.Demon, "", "quasit", RevengeMonsterProgression.Beast, 'q', 3, 17, 2, 8, 0, new Roll[3]
			{
				new Roll(1, 3),
				new Roll(1, 3),
				new Roll(1, 6)
			})),
			new MonsterDefinition(250, new Being(MonsterType.Doppelganger, Race.Fairy, "", "doppelganger", RevengeMonsterProgression.Sneak, '@', 4, 14, 5, 8, 0, new Roll[1]
			{
				new Roll(1, 12)
			}, SpecialAbility.Imitate, 40, 3)),
			new MonsterDefinition(600, new Being(MonsterType.Ogre, Race.Giant, "", "ogre", RevengeMonsterProgression.Brute, 'O', 4, 12, 4, 8, 3, new Roll[1]
			{
				new Roll(2, 6, 2)
			})),
			new MonsterDefinition(400, new Being(MonsterType.Centaur, Race.MythicalBeast, "", "centaur", RevengeMonsterProgression.Warrior, 'c', 5, 14, 4, 8, 0, new Roll[3]
			{
				new Roll(1, 4),
				new Roll(1, 4),
				new Roll(1, 6, 1)
			})),
			new MonsterDefinition(400, new Being(MonsterType.FloatingEye, Race.MythicalBeast, "", "floating eye", RevengeMonsterProgression.None, 'e', 5, 17, 2, 8, 0, new Roll[1]
			{
				new Roll(1, 4)
			}, SpecialAbility.ParalyzeOnHitOrBeingHit, 30, 100)),
			new MonsterDefinition(400, new Being(MonsterType.Imp, Race.Devil, "", "imp", RevengeMonsterProgression.Beast, 'i', 6, 17, 4, 8, 0, new Roll[3]
			{
				new Roll(1, 4),
				new Roll(1, 4),
				new Roll(1, 6)
			})),
			new MonsterDefinition(400, new Being(MonsterType.HellHound, Race.MythicalBeast, "", "hell hound", RevengeMonsterProgression.Beast, 'h', 6, 15, 5, 8, 0, new Roll[1]
			{
				new Roll(2, 4)
			}, SpecialAbility.FireBreath, 4, 6)),
			new MonsterDefinition(400, new Being(MonsterType.Tiger, Race.Animal, "", "tiger", RevengeMonsterProgression.None, 't', 7, 15, 5, 8, 0, new Roll[3]
			{
				new Roll(1, 8),
				new Roll(1, 8),
				new Roll(1, 8, 2)
			})),
			new MonsterDefinition(400, new Being(MonsterType.PhaseSpider, Race.MythicalBeast, "", "phase spider", RevengeMonsterProgression.Beast, 'p', 7, 18, 3, 8, 0, new Roll[1]
			{
				new Roll(2, 4)
			}, SpecialAbility.Desolid)),
			new MonsterDefinition(400, new Being(MonsterType.Yeti, Race.MythicalBeast, "", "yeti", RevengeMonsterProgression.Brute, 'Y', 8, 14, 8, 8, 0, new Roll[3]
			{
				new Roll(1, 6),
				new Roll(1, 6),
				new Roll(1, 6)
			})),
			new MonsterDefinition(400, new Being(MonsterType.Manticore, Race.MythicalBeast, "", "manticore", RevengeMonsterProgression.Beast, 'm', 8, 18, 3, 8, 0, new Roll[1]
			{
				new Roll(2, 4)
			}, SpecialAbility.ShootStings, 5, 12)),
			new MonsterDefinition(400, new Being(MonsterType.GiantSpider, Race.GiantAnimal, "", "giant spider", RevengeMonsterProgression.None, 's', 9, 15, 5, 8, 0, new Roll[1]
			{
				new Roll(1, 6)
			}, SpecialAbility.SpiderWebAndPoison, 20, 8)),
			new MonsterDefinition(400, new Being(MonsterType.UmberHulk, Race.MythicalBeast, "", "umber hulk", RevengeMonsterProgression.Brute, 'U', 9, 16, 7, 8, 7, new Roll[3]
			{
				new Roll(1, 6),
				new Roll(1, 6),
				new Roll(2, 4)
			})),
			new MonsterDefinition(400, new Being(MonsterType.DarkElf, Race.Fairy, "", "dark elf", RevengeMonsterProgression.Sneak, 'd', 10, 19, 8, 8, 0, new Roll[1]
			{
				new Roll(1, 6)
			}, SpecialAbility.Poison, 20, 10)),
			new MonsterDefinition(400, new Being(MonsterType.ZombieLord, Race.Undead, "", "zombie lord", RevengeMonsterProgression.GreaterUndead, 'Z', 10, 13, 16, 8, 0, new Roll[1]
			{
				new Roll(2, 6)
			})),
			new MonsterDefinition(400, new Being(MonsterType.GiantAnt, Race.GiantAnimal, "", "giant ant", RevengeMonsterProgression.None, 'a', 11, 20, 12, 8, 0, new Roll[1]
			{
				new Roll(2, 6)
			}, SpecialAbility.Burrow, 80)),
			new MonsterDefinition(400, new Being(MonsterType.RustMonster, Race.MythicalBeast, "", "rust monster", RevengeMonsterProgression.None, 'R', 11, 17, 6, 8, 0, new Roll[3]
			{
				new Roll(1, 4),
				new Roll(1, 4),
				new Roll(2, 4)
			}, SpecialAbility.RustOnHit, 30)),
			new MonsterDefinition(400, new Being(MonsterType.Jackalwere, Race.MythicalBeast, "", "jackalwere", RevengeMonsterProgression.Beast, 'j', 12, 16, 7, 8, 7, new Roll[1]
			{
				new Roll(2, 6)
			}, SpecialAbility.SleepSong, 20, 10)),
			new MonsterDefinition(400, new Being(MonsterType.Fomorian, Race.Giant, "", "fomorian", RevengeMonsterProgression.Brute, 'F', 12, 13, 12, 8, 0, new Roll[1]
			{
				new Roll(2, 6)
			})),
			new MonsterDefinition(400, new Being(MonsterType.Quinotaur, Race.MythicalBeast, "", "quinotaur", RevengeMonsterProgression.Warrior, 'Q', 13, 14, 9, 8, 18, new Roll[5]
			{
				new Roll(1, 6),
				new Roll(1, 6),
				new Roll(1, 6),
				new Roll(1, 6),
				new Roll(1, 6)
			})),
			new MonsterDefinition(400, new Being(MonsterType.GiantScorpion, Race.GiantAnimal, "", "giant scorpion", RevengeMonsterProgression.None, 'S', 13, 15, 6, 8, 0, new Roll[3]
			{
				new Roll(1, 10),
				new Roll(1, 10),
				new Roll(1, 10)
			}, SpecialAbility.Poison, 33, 12)),
			new MonsterDefinition(400, new Being(MonsterType.BlackKnight, Race.Humanoid, "", "black knight", RevengeMonsterProgression.Warrior, 'K', 14, 28, 10, 8, 20, new Roll[2]
			{
				new Roll(1, 8, 4),
				new Roll(1, 8, 4)
			})),
			new MonsterDefinition(400, new Being(MonsterType.Troll, Race.MythicalBeast, "", "troll", RevengeMonsterProgression.Brute, 'T', 14, 16, 6, 8, 18, new Roll[3]
			{
				new Roll(1, 8),
				new Roll(1, 8),
				new Roll(1, 8)
			}, SpecialAbility.Regeneration, 8)),
			new MonsterDefinition(400, new Being(MonsterType.Vampire, Race.Undead, "", "vampire", RevengeMonsterProgression.GreaterUndead, 'v', 15, 19, 10, 12, 20, new Roll[1]
			{
				new Roll(1, 8, 6)
			}, SpecialAbility.DrainConstitution, 30)),
			new MonsterDefinition(400, new Being(MonsterType.YethHound, Race.MythicalBeast, "", "yeth hound", RevengeMonsterProgression.Beast, 'y', 15, 16, 12, 8, 0, new Roll[1]
			{
				new Roll(3, 5, 10)
			})),
			new MonsterDefinition(400, new Being(MonsterType.Naga, Race.MythicalBeast, "", "naga", RevengeMonsterProgression.Beast, 'N', 16, 18, 12, 8, 0, new Roll[1]
			{
				new Roll(1, 6, 4)
			}, SpecialAbility.Poison, 80, 20)),
			new MonsterDefinition(400, new Being(MonsterType.Hydra, Race.MythicalBeast, "", "hydra", RevengeMonsterProgression.Beast, 'H', 16, 18, 8, 8, 16, new Roll[8]
			{
				new Roll(1, 8),
				new Roll(1, 8),
				new Roll(1, 8),
				new Roll(1, 8),
				new Roll(1, 8),
				new Roll(1, 8),
				new Roll(1, 8),
				new Roll(1, 8)
			})),
			new MonsterDefinition(400, new Being(MonsterType.Mummy, Race.Undead, "", "mummy", RevengeMonsterProgression.GreaterUndead, 'M', 17, 18, 14, 12, 28, new Roll[2]
			{
				new Roll(1, 10, 3),
				new Roll(1, 10, 3)
			}, SpecialAbility.CauseDisease, 50, 300)),
			new MonsterDefinition(400, new Being(MonsterType.UrVile, Race.MythicalBeast, "", "ur vile", RevengeMonsterProgression.Brute, 'u', 17, 22, 14, 8, 0, new Roll[1]
			{
				new Roll(4, 6, 8)
			})),
			new MonsterDefinition(400, new Being(MonsterType.Chimera, Race.MythicalBeast, "", "chimera", RevengeMonsterProgression.Beast, 'C', 18, 20, 15, 8, 0, new Roll[5]
			{
				new Roll(1, 4),
				new Roll(1, 4),
				new Roll(1, 6),
				new Roll(1, 10),
				new Roll(3, 6)
			}, SpecialAbility.FireBreath, 6, 20)),
			new MonsterDefinition(400, new Being(MonsterType.Ettin, Race.Giant, "", "ettin", RevengeMonsterProgression.Brute, 'E', 18, 18, 17, 8, 0, new Roll[2]
			{
				new Roll(2, 8, 4),
				new Roll(2, 8, 4)
			})),
			new MonsterDefinition(400, new Being(MonsterType.Nymph, Race.Fairy, "", "nymph", RevengeMonsterProgression.None, 'n', 19, 28, 4, 8, 8, new Roll[1]
			{
				new Roll(1, 4)
			}, SpecialAbility.BeautyOfTheNymph)),
			new MonsterDefinition(400, new Being(MonsterType.Jinn, Race.MythicalBeast, "", "jinn", RevengeMonsterProgression.Warrior, 'J', 19, 22, 20, 8, 0, new Roll[2]
			{
				new Roll(2, 10, 5),
				new Roll(2, 10, 5)
			})),
			new MonsterDefinition(400, new Being(MonsterType.BurrowWight, Race.Undead, "", "burrow wight", RevengeMonsterProgression.GreaterUndead, 'W', 20, 24, 16, 12, 32, new Roll[2]
			{
				new Roll(1, 12, 6),
				new Roll(1, 12, 6)
			}, SpecialAbility.DrainStrength, 30)),
			new MonsterDefinition(400, new Being(MonsterType.PurpleWorm, Race.MythicalBeast, "", "purple worm", RevengeMonsterProgression.None, 'P', 20, 17, 24, 8, 0, new Roll[2]
			{
				new Roll(1, 6),
				new Roll(1, 20)
			}, SpecialAbility.PoisonAndBurrow, 40, 6)),
			new MonsterDefinition(400, new Being(MonsterType.FireDrake, Race.Dragon, "", "fire drake", RevengeMonsterProgression.Beast, 'f', 21, 24, 16, 8, 0, new Roll[3]
			{
				new Roll(1, 8, 3),
				new Roll(1, 8, 3),
				new Roll(2, 8, 6)
			}, SpecialAbility.FireBreath, 6, 30)),
			new MonsterDefinition(400, new Being(MonsterType.Ifrit, Race.MythicalBeast, "", "ifrit", RevengeMonsterProgression.Warrior, 'I', 21, 25, 20, 8, 0, new Roll[2]
			{
				new Roll(2, 12, 8),
				new Roll(2, 12, 8)
			})),
			new MonsterDefinition(400, new Being(MonsterType.FireGiant, Race.Giant, "", "fire giant", RevengeMonsterProgression.Brute, 'G', 22, 30, 24, 8, 0, new Roll[1]
			{
				new Roll(4, 10, 10)
			}, SpecialAbility.ThrowRocks, 8, 20)),
			new MonsterDefinition(400, new Being(MonsterType.WillOWisp, Race.Fairy, "", "will o_wisp", RevengeMonsterProgression.None, 'I', 22, 36, 8, 8, 16, new Roll[2]
			{
				new Roll(2, 12, 8),
				new Roll(2, 12, 8)
			}, SpecialAbility.Confuse)),
			new MonsterDefinition(400, new Being(MonsterType.Alp, Race.Fairy, "", "alp", RevengeMonsterProgression.Brute, 'A', 23, 24, 16, 8, 0, new Roll[2]
			{
				new Roll(2, 8),
				new Roll(2, 8)
			}, SpecialAbility.Invisibility)),
			new MonsterDefinition(400, new Being(MonsterType.Xorn, Race.MythicalBeast, "", "xorn", RevengeMonsterProgression.Beast, 'x', 23, 32, 15, 8, 60, new Roll[3]
			{
				new Roll(1, 10, 4),
				new Roll(1, 10, 4),
				new Roll(1, 10, 4)
			}, SpecialAbility.Desolid)),
			new MonsterDefinition(400, new Being(MonsterType.VampireLord, Race.Undead, "", "vampire lord", RevengeMonsterProgression.GreaterUndead, 'V', 24, 28, 20, 12, 40, new Roll[1]
			{
				new Roll(1, 12, 10)
			}, SpecialAbility.DrainConstitution, 60)),
			new MonsterDefinition(400, new Being(MonsterType.Exterminator, Race.Humanoid, "", "exterminator", RevengeMonsterProgression.Brute, 'X', 24, 25, 18, 8, 0, new Roll[2]
			{
				new Roll(2, 10, 5),
				new Roll(2, 10, 5)
			})),
			new MonsterDefinition(400, new Being(MonsterType.Dragon, Race.Dragon, "", "dragon", RevengeMonsterProgression.Beast, 'D', 25, 36, 30, 8, 0, new Roll[3]
			{
				new Roll(1, 12, 6),
				new Roll(1, 12, 6),
				new Roll(2, 10, 6)
			}, SpecialAbility.FireBreath, 10, 40)),
			new MonsterDefinition(400, new Being(MonsterType.Balor, Race.Demon, "", "balor", RevengeMonsterProgression.Brute, 'B', 25, 40, 25, 8, 0, new Roll[3]
			{
				new Roll(1, 8, 12),
				new Roll(2, 10, 12),
				new Roll(2, 10, 6)
			})),
			new MonsterDefinition(0, new Being(MonsterType.Lich, Race.Undead, "Kalmius", "lich", RevengeMonsterProgression.None, 'L', 25, 37, 0, 0, 271, new Roll[1]
			{
				new Roll(1, 10)
			}, SpecialAbility.LichPowers), 1)
		};
		MonsterDefinition[] monsters = _monsters;
		foreach (MonsterDefinition monsterDefinition in monsters)
		{
			_definitions.Add(monsterDefinition.Template.MonsterType, monsterDefinition);
		}
	}

	public MonsterList GetAvailableMonsters(DungeonLevel dungeonLevel)
	{
		return GetAvailableMonsters(dungeonLevel.Level);
	}

	public MonsterList GetAvailableMonsters(int level)
	{
		int num = Math.Min(_monsters.Length, 2 + level * 2);
		MonsterList monsterList = new MonsterList(level);
		MonsterDefinition[] monsters = _monsters;
		foreach (MonsterDefinition monsterDefinition in monsters)
		{
			if (monsterDefinition.IsAvailable(level))
			{
				monsterList.Add(level, monsterDefinition);
			}
			num--;
			if (num == 0)
			{
				break;
			}
		}
		return monsterList;
	}

	private MonsterList GetAvailableMonsters(int level, Race race)
	{
		int num = Math.Min(_monsters.Length, 2 + level * 2);
		MonsterList monsterList = new MonsterList(level);
		MonsterDefinition[] monsters = _monsters;
		foreach (MonsterDefinition monsterDefinition in monsters)
		{
			if (monsterDefinition.IsAvailable(level) && monsterDefinition.Template.Race == race)
			{
				monsterList.Add(level, monsterDefinition);
			}
			num--;
			if (num == 0)
			{
				break;
			}
		}
		return monsterList;
	}

	public Being CreateMonster(DungeonLevel dungeonLevel, MonsterType monsterType)
	{
		RevengeMonster revengeMonster = Game.Instance.RevengeSystem.PullRevengeMonsterForType(dungeonLevel, monsterType);
		if (revengeMonster != null)
		{
			return revengeMonster;
		}
		return _definitions[monsterType].CreateMonster();
	}

	public void NoteKill(MonsterType monsterType)
	{
		if (monsterType < MonsterType.Grog)
		{
			_killCount[(int)monsterType]++;
		}
	}

	public bool HasBeenKilled(MonsterType monsterType)
	{
		return KillCount(monsterType) > 0;
	}

	public int KillCount(MonsterType monsterType)
	{
		return _killCount[(int)monsterType];
	}

	public string GetMonsterDescription(Being being)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(being.Character(Game.Instance.DungeonMaster.CurrentDungeonLevel, being.X, being.Y));
		stringBuilder.Append(" - ");
		stringBuilder.Append(being.ShortenedName);
		bool flag = false;
		if (_isArmorClassKnown.Contains(being.MonsterType))
		{
			stringBuilder.Append(" (AC: ").Append(being.ArmorClass);
			flag = true;
		}
		if (_areHitDiceKnown.Contains(being.MonsterType))
		{
			if (being.HitDice == 0 && being.HitDieBonus > 0)
			{
				if (!being.HitPointsKnown)
				{
					if (flag)
					{
						stringBuilder.Append(", ");
					}
					else
					{
						stringBuilder.Append(" (");
					}
					stringBuilder.Append("HD: ").Append('+').Append(being.HitDieBonus);
					flag = true;
				}
			}
			else if (being.HitDice > 0)
			{
				if (flag)
				{
					stringBuilder.Append(", ");
				}
				else
				{
					stringBuilder.Append(" (");
				}
				stringBuilder.Append("HD: ").Append(being.HitDice).Append('d')
					.Append(being.HitDieSides);
				if (being.HitDieBonus < 0)
				{
					stringBuilder.Append(being.HitDieBonus);
				}
				else if (being.HitDieBonus > 0)
				{
					stringBuilder.Append('+').Append(being.HitDieBonus);
				}
				flag = true;
			}
		}
		if (being.HitPointsKnown)
		{
			if (flag)
			{
				stringBuilder.Append(", ");
			}
			else
			{
				stringBuilder.Append(" (");
			}
			stringBuilder.Append("H: ").Append(being.HitPoints).Append("/")
				.Append(being.MaxHitPoints);
			flag = true;
		}
		if (!_isDamageRoutineKnown.Contains(being.MonsterType))
		{
			if (_isAttackRoutineKnown.Contains(being.MonsterType))
			{
				if (flag)
				{
					stringBuilder.Append(", ");
				}
				else
				{
					stringBuilder.Append(" (");
				}
				stringBuilder.Append("#A: ").Append(being.NaturalDamage.Length);
				flag = true;
			}
		}
		else
		{
			if (flag)
			{
				stringBuilder.Append(", ");
			}
			else
			{
				stringBuilder.Append(" (");
			}
			stringBuilder.Append("D: ");
			for (int i = 0; i < being.NumberOfMeleeAttacks; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append('/');
				}
				stringBuilder.Append(being.NaturalDamage[i].NumberOfDice).Append('d').Append(being.NaturalDamage[i].DieSides);
				if (being.NaturalDamage[i].DieBonus < 0)
				{
					stringBuilder.Append(being.NaturalDamage[i].DieBonus);
				}
				else if (being.NaturalDamage[i].DieBonus > 0)
				{
					stringBuilder.Append('+').Append(being.NaturalDamage[i].DieBonus);
				}
			}
			flag = true;
		}
		if (flag)
		{
			stringBuilder.Append(')');
		}
		return stringBuilder.ToString();
	}

	public void LearnArmorClass(Being being)
	{
		if (being != null)
		{
			_isArmorClassKnown.Add(being.MonsterType);
		}
	}

	public void LearnAttackRoutine(Being being)
	{
		if (being != null)
		{
			_isAttackRoutineKnown.Add(being.MonsterType);
		}
	}

	public void LearnHitDice(Being being)
	{
		if (being != null)
		{
			_areHitDiceKnown.Add(being.MonsterType);
		}
	}

	public void LearnDamageRoutine(Being being)
	{
		if (being != null)
		{
			_isDamageRoutineKnown.Add(being.MonsterType);
		}
	}

	public void ForgetMonsterKnowledge(Ghost ghost)
	{
		_isArmorClassKnown.Remove(ghost.MonsterType);
		_isAttackRoutineKnown.Remove(ghost.MonsterType);
		_areHitDiceKnown.Remove(ghost.MonsterType);
		_isDamageRoutineKnown.Remove(ghost.MonsterType);
	}

	public MonsterDefinition GetMonsterDefinition(MonsterType monsterType)
	{
		MonsterDefinition[] monsters = _monsters;
		foreach (MonsterDefinition monsterDefinition in monsters)
		{
			if (monsterDefinition.Template.MonsterType == monsterType)
			{
				return monsterDefinition;
			}
		}
		return null;
	}

	public Being CreateRandomMonster(DungeonLevel dungeonLevel)
	{
		return GetAvailableMonsters(dungeonLevel.Level).CreateNewMonster(dungeonLevel);
	}

	public Being CreateRandomMonster(DungeonLevel dungeonLevel, Race race)
	{
		return GetAvailableMonsters(dungeonLevel.Level, race).CreateNewMonster(dungeonLevel);
	}

	public void AddInformation(List<string> information)
	{
		information.Add("Monster types: " + _monsters.Length);
	}
}
