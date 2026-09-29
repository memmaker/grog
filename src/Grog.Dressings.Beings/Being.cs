using System;
using System.Collections.Generic;
using Grog.Dressings.Items;
using Grog.Dressings.Items.Types;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Monsters;
using Grog.Dungeons.Generators.Rooms;
using Grog.Kernel;
using Grog.Kernel.Interactions;
using Grog.Systems.Revenge;

namespace Grog.Dressings.Beings;

[Serializable]
public class Being : Thing
{
	[Serializable]
	public class MoveIntoToAttackMoveIntoInteraction : IMoveIntoInteraction
	{
		public bool MoveInto(DungeonLevel dungeonLevel, Thing attacker, Thing target, int fx, int fy, int tx, int ty)
		{
			Being being = (Being)attacker;
			if (being.WillAttack(being, target))
			{
				being.MeleeAttack(dungeonLevel, being, (Being)target);
				being.Moves++;
			}
			return false;
		}
	}

	[Serializable]
	public class ActDependingOnMoodThingAction : IThingAction
	{
		public void Act(DungeonLevel dungeonLevel, Thing thing)
		{
			Being being = (Being)thing;
			being.Moved = false;
			if (being.IsSleeping != 0 || being.IsParalyzed != 0 || (being.IsStunned != 0 && Game.Instance.Random(100) > being.Level * 4))
			{
				being.Moves++;
				return;
			}
			if (being.SpecialAbility == SpecialAbility.BeautyOfTheNymph && dungeonLevel.Grog.CanSee(dungeonLevel, being) && dungeonLevel.Grog.IsStunned == 0 && Game.Instance.Probability(being.Level * 5 - dungeonLevel.Grog.Level * 3))
			{
				dungeonLevel.Message(being, dungeonLevel.Grog.Name + " is stunned by the beauty of " + being.Name + "!");
				dungeonLevel.Grog.IsStunned += new Roll(1, 3).GetDieResult();
			}
			switch (being.Mood)
			{
			case Mood.WaitForPlayer:
			{
				if (!being.CanSee(dungeonLevel, dungeonLevel.Grog) || (!dungeonLevel.Message(being, " gazes at " + dungeonLevel.Grog.Name + " with its icy stare...", more: true) && !dungeonLevel.Message("'s brain is filled with a chilling voice!", more: true)))
				{
					break;
				}
				int failedPlayerCount = Game.Instance.HighscoreManager.GetFailedPlayerCount();
				dungeonLevel.Say("Thou hast come here? Finally? I expected to ye to fail like the " + ((failedPlayerCount < 10) ? "many" : failedPlayerCount.ToString()) + " fools before ye!", more: true);
				being.Mood = Mood.FightStatically;
				being.MaxHitPoints += failedPlayerCount;
				being.HitPoints += failedPlayerCount;
				goto case Mood.FightStatically;
			}
			case Mood.FightStatically:
				if (Math.Abs(dungeonLevel.Grog.X - being.X) <= 1 && Math.Abs(dungeonLevel.Grog.Y - being.Y) <= 1)
				{
					being.Mood = Mood.Hunting;
				}
				if (!being.FightAtRange(dungeonLevel, being) && being.Mood == Mood.Hunting)
				{
					being.FightMelee(dungeonLevel, being);
				}
				break;
			case Mood.Sleeping:
				if (dungeonLevel.GetRoomAt(dungeonLevel.Grog.X, dungeonLevel.Grog.Y) == dungeonLevel.GetRoomAt(being.X, being.Y) && Game.Instance.Random(50) == 0 && (!dungeonLevel.Grog.Inventory.HasEquippedItemWithAbility(ItemAbility.Stealth) || Game.Instance.Probability(10)))
				{
					dungeonLevel.Message(being, " awakens!", more: true);
					being.Mood = ((!being.CanSee(dungeonLevel, dungeonLevel.Grog)) ? Mood.Ambushing : Mood.Hunting);
					being.ResetAutomaticAction();
				}
				break;
			case Mood.Ambushing:
				if (!being.SingMagicalSong(dungeonLevel, being, dungeonLevel.Grog, 50) && being.CanSee(dungeonLevel, dungeonLevel.Grog))
				{
					if (!dungeonLevel.Message(being, " moves towards you threateningly!", more: true))
					{
						dungeonLevel.Message(" feels endangered!", more: true);
					}
					being.Mood = Mood.Hunting;
				}
				break;
			case Mood.Moving:
				if (!being.SingMagicalSong(dungeonLevel, being, dungeonLevel.Grog, 30))
				{
					being.GetRandomMovementModifier(being, out var xm, out var ym);
					if (dungeonLevel.IsOpenForMonster(being, being.X + xm, being.Y + ym))
					{
						dungeonLevel.MoveThing(being, being.X, being.Y, being.X + xm, being.Y + ym);
					}
				}
				break;
			case Mood.Hunting:
				if (!being.SingMagicalSong(dungeonLevel, being, dungeonLevel.Grog, 100) && !being.FightAtRange(dungeonLevel, being))
				{
					being.FightMelee(dungeonLevel, being);
				}
				break;
			}
			if (being.IsAlive && being.SpecialAbility == SpecialAbility.SpiderWebAndPoison && !dungeonLevel.HasFeatureAt(being.X, being.Y) && Game.Instance.Random(100) < 10)
			{
				dungeonLevel.Message(being, " spins a web!");
				dungeonLevel.SetFeatureAt(being.X, being.Y, Feature.SpiderWeb);
			}
			being.Moves++;
		}
	}

	private string _type;

	private int _experienceValue;

	public Mood Mood;

	private int _armorClass;

	public virtual string Type
	{
		get
		{
			return _type;
		}
		set
		{
			_type = value;
		}
	}

	public RevengeMonsterProgression RevengeMonsterProgression { get; }

	public MonsterPool.MonsterType MonsterType { get; }

	public Race Race { get; }

	public string ChristenedName { get; set; }

	public long UID { get; protected set; }

	public virtual string Name
	{
		get
		{
			if (Game.Instance.DungeonMaster.CurrentDungeonLevel == null || !Game.Instance.DungeonMaster.CurrentDungeonLevel.IsValid(X, Y) || !Game.Instance.DungeonMaster.CurrentDungeonLevel.IsRoomAt(X, Y) || !Game.Instance.DungeonMaster.CurrentDungeonLevel.GetRoomAt(X, Y).IsDark)
			{
				if (!string.IsNullOrEmpty(ChristenedName))
				{
					return ChristenedName + " the " + Type;
				}
				return "the " + Type;
			}
			if (!Game.Instance.Probability(50))
			{
				return "something";
			}
			return "someone";
		}
	}

	public string ShortenedName
	{
		get
		{
			if (!Name.StartsWith("the "))
			{
				return Name;
			}
			return Name.Substring(4);
		}
	}

	public int Level { get; set; }

	public int HitDice { get; }

	public int HitDieSides { get; }

	public int HitDieBonus { get; }

	public virtual int NumberOfMeleeAttacks => NaturalDamage.Length;

	public Roll[] NaturalDamage { get; set; }

	public virtual bool IsAlive
	{
		get
		{
			if (HitPoints > 0 && Strength > 0 && Dexterity > 0)
			{
				return Constitution > 0;
			}
			return false;
		}
	}

	public int HitPoints { get; set; }

	public int MaxHitPoints { get; set; }

	[field: NonSerialized] public IAutomaticAction AutomaticAction { get; set; } //RVIP: an in-progress run/rest is not saved (its classes are not [Serializable]; saving during one wrote a truncated save)

	public int Gold { get; set; }

	public int ExtraExperienceValue { get; set; }

	public SpecialAbility SpecialAbility { get; set; }

	public int FirstAbilityValue { get; }

	public int SecondAbilityValue { get; }

	public virtual int Strength { get; set; }

	public virtual int Dexterity { get; set; }

	public virtual int Constitution { get; set; }

	public int IsSick { get; set; }

	public int Moves { get; set; }

	public int IsConfused { get; set; }

	public int IsParalyzed { get; set; }

	public int IsBlind { get; set; }

	public int IsSleeping { get; set; }

	public int IsPoisoned { get; set; }

	public int IsStunned { get; set; }

	public int IsDeaf { get; set; }

	public virtual int ToHitBonus => Level + (Strength - 10) >> 1;

	public virtual int ToDamageBonus => Strength - 10 >> 1;

	public override bool IsActive => IsAlive;

	public virtual bool IsImmuneToFire
	{
		get
		{
			if (Race != Race.Demon && Race != Race.Devil)
			{
				return Race == Race.Dragon;
			}
			return true;
		}
	}

	public virtual int ArmorClass
	{
		get
		{
			return _armorClass + ((Dexterity < 10) ? (Dexterity - 10 >> 1) : ((Dexterity - 10) / 2 + (Dexterity - 10) / 3)) + ((IsFrozen != 0) ? (-4 - IsFrozen / 10) : 0);
		}
		set
		{
			_armorClass = value;
		}
	}

	public int ExperienceValue
	{
		get
		{
			int num = _experienceValue;
			switch (SpecialAbility)
			{
			case SpecialAbility.MoveErratically:
				num += Math.Max(4, _experienceValue / 4);
				break;
			case SpecialAbility.CauseDisease:
				num += FirstAbilityValue * SecondAbilityValue / 100;
				break;
			case SpecialAbility.MovesSlowly:
				num /= 2;
				break;
			case SpecialAbility.Imitate:
				num += Math.Max(3, _experienceValue / 3);
				break;
			case SpecialAbility.StealGold:
				num += Math.Max(3, _experienceValue / 5);
				break;
			case SpecialAbility.Desolid:
				num += Math.Max(1, _experienceValue / 10);
				break;
			case SpecialAbility.Burrow:
				num += Math.Max(1, _experienceValue / 9);
				break;
			case SpecialAbility.ParalyzeOnHitOrBeingHit:
				num += Math.Max(3, _experienceValue / 2);
				break;
			case SpecialAbility.Poison:
				num += Math.Max(1, _experienceValue / 3);
				break;
			case SpecialAbility.PoisonAndBurrow:
				num += Math.Max(1, _experienceValue / 3) + Math.Max(1, _experienceValue / 9);
				break;
			case SpecialAbility.SpiderWebAndPoison:
				num += Math.Max(1, _experienceValue / 3) + Math.Max(1, _experienceValue / 6);
				break;
			case SpecialAbility.DrainConstitution:
			case SpecialAbility.DrainStrength:
				num += _experienceValue;
				break;
			case SpecialAbility.LichPowers:
				num += _experienceValue * 3;
				break;
			case SpecialAbility.Regeneration:
				num += FirstAbilityValue * 10;
				break;
			case SpecialAbility.Invisibility:
				num += num / 2;
				break;
			case SpecialAbility.Confuse:
				num *= 3;
				break;
			case SpecialAbility.BeautyOfTheNymph:
				num *= 4;
				break;
			case SpecialAbility.RustOnHit:
				num += FirstAbilityValue * 5;
				break;
			case SpecialAbility.ShootStings:
				num += FirstAbilityValue * 2 + SecondAbilityValue * 3;
				break;
			case SpecialAbility.ThrowRocks:
				num += FirstAbilityValue * 2 + SecondAbilityValue * 3;
				break;
			case SpecialAbility.FireBreath:
				num += FirstAbilityValue * 3 + SecondAbilityValue * 5;
				break;
			}
			if (this is RevengeMonster revengeMonster)
			{
				foreach (RevengeMonsterPower revengeMonsterPower in revengeMonster.RevengeMonsterPowers)
				{
					switch (revengeMonsterPower)
					{
					case RevengeMonsterPower.Regeneration:
						num += Math.Max(1, Level / 6) * 10;
						break;
					case RevengeMonsterPower.DrainStrength:
					case RevengeMonsterPower.DrainConstitution:
						num += _experienceValue;
						break;
					case RevengeMonsterPower.MoveErratically:
						num += Math.Max(4, _experienceValue / 4);
						break;
					case RevengeMonsterPower.ParalyzeOnTouch:
						num += Math.Max(3, _experienceValue / 2);
						break;
					case RevengeMonsterPower.Poison:
						num += Math.Max(1, _experienceValue / 3);
						break;
					case RevengeMonsterPower.Freeze:
						num += Math.Max(2, _experienceValue / 3);
						break;
					case RevengeMonsterPower.Disarm:
						num += Math.Max(3, _experienceValue / 3);
						break;
					case RevengeMonsterPower.SmashShield:
						num += Math.Max(3, _experienceValue / 4);
						break;
					case RevengeMonsterPower.SmashWeapon:
						num += Math.Max(3, _experienceValue / 3);
						break;
					case RevengeMonsterPower.SmashArmor:
						num += Math.Max(3, _experienceValue / 2);
						break;
					}
				}
			}
			return num + Math.Min(ExtraExperienceValue, num / 3);
		}
		set
		{
			_experienceValue = value;
		}
	}

	public virtual bool ResistsSickness
	{
		get
		{
			if (Race != Race.Undead)
			{
				return Game.Instance.Random(100) < Level * 4;
			}
			return true;
		}
	}

	public virtual bool ResistsParalyzation
	{
		get
		{
			if (Race != Race.Undead)
			{
				return Game.Instance.Random(100) < Level * 4;
			}
			return true;
		}
	}

	public virtual bool ResistsConfusion
	{
		get
		{
			if (Race != Race.Undead && Race != Race.Demon && Race != Race.Fairy)
			{
				return Game.Instance.Random(100) < Level * 4;
			}
			return true;
		}
	}

	public virtual bool ResistsBlindness => Game.Instance.Random(100) < Level * 4;

	public virtual bool ResistsIntoxication
	{
		get
		{
			if (Race != Race.Undead && Race != Race.Demon && Race != Race.Fairy)
			{
				return Game.Instance.Random(100) < Constitution * 3 + Level * 2;
			}
			return true;
		}
	}

	public virtual bool ResistsPoison
	{
		get
		{
			if (Race != Race.Undead && Race != Race.Demon && Race != Race.Fairy)
			{
				return Game.Instance.Random(100) < Constitution * 2 + Level;
			}
			return true;
		}
	}

	public virtual bool ResistsSleep => Race == Race.Undead;

	public virtual bool ResistsStrengthDrain => Race == Race.Undead;

	public virtual bool AggravatesMonsters => false;

	public int IsHastened { get; set; }

	public int IsDrunk { get; set; }

	public int IsLevitating { get; set; }

	public bool IsInverted { get; set; }

	public virtual bool IsInvisible
	{
		get
		{
			if (SpecialAbility != SpecialAbility.Invisibility)
			{
				return TurnsOfInvisibility > 0;
			}
			return true;
		}
	}

	public int TurnsOfInvisibility { get; set; }

	public virtual bool CanSeeInvisible
	{
		get
		{
			if (SpecialAbility != SpecialAbility.LichPowers && SpecialAbility != SpecialAbility.Invisibility && Race != Race.Demon && Race != Race.Fairy)
			{
				return Game.Instance.Random(100) < Level * 2;
			}
			return true;
		}
	}

	public virtual bool CanMoveFreely
	{
		get
		{
			if (SpecialAbility != SpecialAbility.Desolid)
			{
				return SpecialAbility == SpecialAbility.SpiderWebAndPoison;
			}
			return true;
		}
	}

	public bool HitPointsKnown { get; set; }

	public int IsFrozen { get; set; }

	public Being(MonsterPool.MonsterType monsterType, Race race, string name, string type, RevengeMonsterProgression revengeMonsterProgression, char character, int level, int armorClass, int hitDice, int hitDieSides, int hitDieBonus, Roll[] naturalDamage, SpecialAbility specialAbility = SpecialAbility.None, int firstAbilityValue = 0, int secondAbilityValue = 0)
		: base(character, null)
	{
		MonsterType = monsterType;
		Race = race;
		ChristenedName = name;
		Type = type;
		RevengeMonsterProgression = revengeMonsterProgression;
		Level = level;
		ArmorClass = armorClass;
		HitDice = hitDice;
		HitDieSides = hitDieSides;
		HitDieBonus = hitDieBonus;
		NaturalDamage = naturalDamage;
		MaxHitPoints = (HitPoints = Roll(hitDice, hitDieSides, hitDieBonus));
		SpecialAbility = specialAbility;
		FirstAbilityValue = firstAbilityValue;
		SecondAbilityValue = secondAbilityValue;
		Act = new ActDependingOnMoodThingAction();
		MoveInto = new MoveIntoToAttackMoveIntoInteraction();
		Mood = GetRandomMood();
		UID = Game.Instance.NextUID;
		Strength = (Dexterity = (Constitution = 10));
		Gold = 0;
		IsConfused = 0;
		ExperienceValue = GetBaseExperienceValue();
		IsInverted = false;
	}

	public virtual bool HasRevengePower(RevengeMonsterPower power)
	{
		return false;
	}

	private int GetBaseExperienceValue()
	{
		int num = MaxHitPoints + (ArmorClass - 10) * 3 + Level;
		Roll[] naturalDamage = NaturalDamage;
		foreach (Roll roll in naturalDamage)
		{
			num += roll.Minimum + roll.Maximum + roll.Average;
		}
		return num;
	}

	private int Roll(int hitDice, int hitDieSides, int hitDieBonus)
	{
		int num = 0;
		for (int i = 0; i < hitDice; i++)
		{
			num += Game.Instance.Random(hitDieSides) + 1;
		}
		if (hitDieSides * hitDice >= 15 && num < hitDice * hitDieSides * 3 / 10)
		{
			num = hitDice * hitDieSides * 3 / 10;
		}
		return num + hitDieBonus;
	}

	public Being(Being template)
		: this(template.MonsterType, template.Race, template.ChristenedName, template.Type, template.RevengeMonsterProgression, template.Character(null, -1, -1), template.Level, template.ArmorClass, template.HitDice, template.HitDieSides, template.HitDieBonus, template.NaturalDamage, template.SpecialAbility, template.FirstAbilityValue, template.SecondAbilityValue)
	{
		Act = template.Act;
		MoveInto = template.MoveInto;
		Mood = GetRandomMood();
	}

	private bool SingMagicalSong(DungeonLevel dungeonLevel, Being being, Player grog, int probabilityPercentage)
	{
		if (being.SpecialAbility != SpecialAbility.SleepSong)
		{
			return false;
		}
		if (grog.IsDeaf != 0)
		{
			return false;
		}
		if (!being.CanSee(dungeonLevel, grog))
		{
			return false;
		}
		if (Game.Instance.Random(100) >= being.FirstAbilityValue * probabilityPercentage / 100)
		{
			return false;
		}
		dungeonLevel.Message(being, " sings a weird song full of melancholy!");
		if (!grog.ResistsSleep && Game.Instance.Random(100) < 48 - grog.Level * 3)
		{
			if (grog.IsSleeping == 0)
			{
				dungeonLevel.Message(" falls asleep!");
			}
			if (grog.IsSleeping >= 0)
			{
				grog.IsSleeping += new Roll(1, being.SecondAbilityValue).GetDieResult();
			}
		}
		return true;
	}

	private bool FightAtRange(DungeonLevel dungeonLevel, Being being)
	{
		if (!being.CanSee(dungeonLevel, dungeonLevel.Grog))
		{
			return false;
		}
		if (being.IsBlind != 0 || being.IsConfused != 0 || being.IsDrunk != 0 || being.IsParalyzed != 0 || being.IsSleeping != 0)
		{
			return false;
		}
		int num = Math.Abs(being.X - dungeonLevel.Grog.X);
		int num2 = Math.Abs(being.Y - dungeonLevel.Grog.Y);
		if (num != 0 && num2 != 0 && num != num2)
		{
			return false;
		}
		int num3 = Math.Sign(dungeonLevel.Grog.X - being.X);
		int num4 = Math.Sign(dungeonLevel.Grog.Y - being.Y);
		int num5 = being.X + num3;
		for (int i = being.Y + num4; num5 != dungeonLevel.Grog.X || i != dungeonLevel.Grog.Y; i += num4)
		{
			if (dungeonLevel.GetThingAt(num5, i) != null)
			{
				return false;
			}
			num5 += num3;
		}
		int num6 = ((num == 0 || num == num2) ? num2 : num);
		if (being.SpecialAbility == SpecialAbility.ShootStings)
		{
			if (num6 > being.FirstAbilityValue)
			{
				return false;
			}
			if (Game.Instance.Random(100) >= 30)
			{
				return false;
			}
			dungeonLevel.Message(being, " fires an iron sting from its fearsome tail at you!");
			dungeonLevel.DrawShot('/', being.X + num3, being.Y + num4, num3, num4, dungeonLevel.Grog.X, dungeonLevel.Grog.Y);
			MissileAttack(dungeonLevel, being, dungeonLevel.Grog, () => new Roll(1, being.SecondAbilityValue).GetDieResult());
			return true;
		}
		if (being.SpecialAbility == SpecialAbility.ThrowRocks)
		{
			if (num6 > being.FirstAbilityValue)
			{
				return false;
			}
			if (Game.Instance.Random(100) >= 20)
			{
				return false;
			}
			dungeonLevel.Message(being, " throws a huge rock at you!");
			dungeonLevel.DrawShot('*', being.X + num3, being.Y + num4, num3, num4, dungeonLevel.Grog.X, dungeonLevel.Grog.Y);
			MissileAttack(dungeonLevel, being, dungeonLevel.Grog, () => new Roll(1, being.SecondAbilityValue).GetDieResult());
			return true;
		}
		if (being.SpecialAbility == SpecialAbility.LichPowers)
		{
			if (num6 > 12)
			{
				return false;
			}
			if (Game.Instance.Random(100) >= 30)
			{
				return false;
			}
			dungeonLevel.Message(being, " casts a vile spell causing glowing energy balls to hit you painfully!");
			dungeonLevel.DrawShot('%', being.X + num3, being.Y + num4, num3, num4, dungeonLevel.Grog.X, dungeonLevel.Grog.Y);
			dungeonLevel.Grog.SufferDamage(dungeonLevel, being, new Roll(3, 8).GetDieResult());
			being.ExtraExperienceValue++;
			return true;
		}
		if (being.SpecialAbility == SpecialAbility.FireBreath)
		{
			if (num6 > being.FirstAbilityValue)
			{
				return false;
			}
			if (Game.Instance.Random(100) >= 15)
			{
				return false;
			}
			dungeonLevel.Message(being, " breathes a cone of fire at " + dungeonLevel.Grog.Name + "!");
			dungeonLevel.DrawRay(being.X + num3, being.Y + num4, num3, num4, Math.Max(num, num2), (int x, int y, bool first, Tile t, Feature f, Being b, List<Item> list) =>
			{
				if (b != null)
				{
					int num7 = Game.Instance.Roll(1, being.SecondAbilityValue);
					if (b is Player player)
					{
						num7 /= player.Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.ResistFire) + 1;
						if (num7 == 0)
						{
							dungeonLevel.Message(" resists!");
						}
						else
						{
							player.SufferDamage(dungeonLevel, being, num7);
							being.ExtraExperienceValue++;
						}
						return false;
					}
					if (!b.IsImmuneToFire)
					{
						dungeonLevel.Message(b, " is badly burned!");
						b.SufferDamage(dungeonLevel, being, num7);
					}
				}
				return true;
			});
			return true;
		}
		return false;
	}

	public void MissileAttack(DungeonLevel dungeonLevel, Being attacker, Being defender, Func<int> getDamage, Action<bool> handleAttackEffects = null)
	{
		int num = attacker.GetMissileCombatRollToHit();
		if (defender.AggravatesMonsters)
		{
			num += 4;
		}
		if (num < 19 + Math.Max(0, attacker.ToHitBonus - defender.Level) && num < defender.ArmorClass)
		{
			dungeonLevel.Message(attacker, " misses " + defender.Name + ".");
			handleAttackEffects?.Invoke(obj: false);
			return;
		}
		int num2 = getDamage();
		if (attacker is Player player)
		{
			Game.Instance.MonsterPool.LearnArmorClass(defender);
			num2 += player.Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.DamagePlusTwo) * 2 + player.Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.DamagePlusFour) * 4 + player.Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.DamagePlusSix) * 6;
		}
		if (defender.AggravatesMonsters)
		{
			num2 += 2;
		}
		if (!dungeonLevel.Message(defender, attacker.Name + " hits " + defender.Name + " for " + num2 + " damage!"))
		{
			attacker.Message(" hits something!");
		}
		defender.SufferDamage(dungeonLevel, attacker, num2);
		attacker.ExtraExperienceValue++;
		handleAttackEffects?.Invoke(obj: true);
	}

	protected virtual int GetMissileCombatRollToHit()
	{
		return Game.Instance.Random(20 + ToHitBonus);
	}

	private void FightMelee(DungeonLevel dungeonLevel, Being being)
	{
		int xm = Math.Sign(dungeonLevel.Grog.X - being.X);
		int ym = Math.Sign(dungeonLevel.Grog.Y - being.Y);
		if (dungeonLevel.Grog.IsInvisible && !being.CanSeeInvisible && being.Mood != Mood.Hunting)
		{
			GetRandomMovementModifier(being, out xm, out ym);
		}
		if (Math.Abs(dungeonLevel.Grog.X - being.X) <= 1 && Math.Abs(dungeonLevel.Grog.Y - being.Y) <= 1 && (SpecialAbility == SpecialAbility.MoveErratically || HasRevengePower(RevengeMonsterPower.MoveErratically)) && (xm == 0 || ym == 0) && Game.Instance.Random(100) < 30 + Level * 3)
		{
			if (xm == 0)
			{
				xm = ((Game.Instance.Random(2) != 0) ? 1 : (-1));
			}
			else
			{
				ym = ((Game.Instance.Random(2) != 0) ? 1 : (-1));
			}
		}
		if (xm != 0 && ym != 0 && being.SpecialAbility != SpecialAbility.MoveErratically && !HasRevengePower(RevengeMonsterPower.MoveErratically))
		{
			if (Game.Instance.Random(2) == 0)
			{
				if (dungeonLevel.IsOpenForMonster(this, being.X + xm, being.Y))
				{
					ym = 0;
				}
				else if (dungeonLevel.IsOpenForMonster(this, being.X, being.Y + ym))
				{
					xm = 0;
				}
				else
				{
					GetRandomMovementModifier(being, out xm, out ym);
				}
			}
			else if (dungeonLevel.IsOpenForMonster(this, being.X, being.Y + ym))
			{
				xm = 0;
			}
			else if (dungeonLevel.IsOpenForMonster(this, being.X + xm, being.Y))
			{
				ym = 0;
			}
			else
			{
				GetRandomMovementModifier(being, out xm, out ym);
			}
		}
		else if (!dungeonLevel.IsOpenForMonster(this, being.X + xm, being.Y + ym))
		{
			GetRandomMovementModifier(being, out xm, out ym);
		}
		if (dungeonLevel.IsOpenForMonster(this, being.X + xm, being.Y + ym))
		{
			dungeonLevel.MoveThing(being, being.X, being.Y, being.X + xm, being.Y + ym);
		}
	}

	public void GetRandomMovementModifier(Being being, out int xm, out int ym)
	{
		xm = Game.Instance.Random(3) - 1;
		if (being.SpecialAbility == SpecialAbility.MoveErratically || HasRevengePower(RevengeMonsterPower.MoveErratically))
		{
			ym = ((xm != 0) ? (Game.Instance.Random(3) - 1) : ((Game.Instance.Random(2) != 0) ? 1 : (-1)));
		}
		else
		{
			ym = ((xm == 0) ? ((Game.Instance.Random(2) != 0) ? 1 : (-1)) : 0);
		}
	}

	private Mood GetRandomMood()
	{
		int num = Game.Instance.Random(100) + 1;
		if (num < 10)
		{
			return Mood.Sleeping;
		}
		if (num < 40)
		{
			return Mood.Ambushing;
		}
		if (num < 80)
		{
			return Mood.Moving;
		}
		return Mood.Hunting;
	}

	protected virtual bool WillAttack(Being attacker, Thing thing)
	{
		return thing is Player;
	}

	private void MeleeAttack(DungeonLevel dungeonLevel, Being attacker, Being defender)
	{
		if (attacker.SpecialAbility == SpecialAbility.Imitate && Game.Instance.Random(100) < attacker.FirstAbilityValue - defender.Level * 2 && defender.IsConfused == 0)
		{
			dungeonLevel.Message(attacker, " suddenly looks like " + defender.Name + ". " + defender.Name + " is confused!");
			if (defender == Game.Instance.Grog)
			{
				dungeonLevel.More();
			}
			defender.IsConfused += Game.Instance.Random(attacker.SecondAbilityValue) + 1;
		}
		int num = ((!attacker.CanSeeInvisible && defender.IsInvisible) ? 4 : 0);
		int numberOfMeleeAttacks = NumberOfMeleeAttacks;
		for (int i = 0; i < numberOfMeleeAttacks; i++)
		{
			if (!defender.IsAlive)
			{
				break;
			}
			int num2 = attacker.GetMeleeRollToHit(numberOfMeleeAttacks);
			if (attacker.Race == Race.Undead && attacker.IsAffectedBySpecialRoomPower(SpecialRoomPower.MightyUndead))
			{
				num2 += 6;
			}
			if (defender.AggravatesMonsters)
			{
				num2 += 4;
			}
			bool flag = num2 >= 19 + Math.Max(0, attacker.ToHitBonus - defender.Level);
			if (attacker.IsAffectedBySpecialRoomPower(SpecialRoomPower.NeverHit, SpecialRoomPower.VeryRarelyHit, SpecialRoomPower.NeverHitByPlayer, SpecialRoomPower.VeryRarelyHitByPlayer))
			{
				flag = false;
				bool flag2 = attacker is Player;
				switch (Game.Instance.DungeonMaster.CurrentDungeonLevel.GetRoomAt(attacker.X, attacker.Y).SpecialPower)
				{
				case SpecialRoomPower.NeverHit:
					num2 = 0;
					break;
				case SpecialRoomPower.VeryRarelyHit:
					if (Game.Instance.Probability(95))
					{
						num2 = 0;
					}
					break;
				case SpecialRoomPower.NeverHitByPlayer:
					if (flag2)
					{
						num2 = 0;
					}
					break;
				case SpecialRoomPower.VeryRarelyHitByPlayer:
					if (flag2 && Game.Instance.Probability(95))
					{
						num2 = 0;
					}
					break;
				}
			}
			int num3 = 0;
			if (defender.Race == Race.Undead && defender.IsAffectedBySpecialRoomPower(SpecialRoomPower.MightyUndead))
			{
				num3 += 6;
			}
			if (!flag && num2 < defender.ArmorClass + num + num3)
			{
				if (!DamagesEquipment(dungeonLevel, attacker, defender))
				{
					dungeonLevel.Message(attacker, " misses " + defender.Name + ".");
				}
				continue;
			}
			if (attacker is Player)
			{
				Game.Instance.MonsterPool.LearnArmorClass(defender);
			}
			CauseDamage(dungeonLevel, attacker, defender, i);
			attacker.ExtraExperienceValue++;
		}
		if (defender is Player)
		{
			Game.Instance.MonsterPool.LearnAttackRoutine(attacker);
		}
		if (!defender.IsAlive)
		{
			return;
		}
		defender.Mood = Mood.Hunting;
		defender.ResetAutomaticAction();
		if (defender.SpecialAbility == SpecialAbility.SummonAmbush || defender.HasRevengePower(RevengeMonsterPower.SummonAmbush))
		{
			int num4 = ((defender.SpecialAbility == SpecialAbility.SummonAmbush) ? defender.SecondAbilityValue : 0);
			if (defender.HasRevengePower(RevengeMonsterPower.SummonAmbush))
			{
				num4 = Math.Max(30, num4);
			}
			if (Game.Instance.Probability(num4))
			{
				int num5 = ((defender.SpecialAbility == SpecialAbility.SummonAmbush) ? defender.FirstAbilityValue : 0);
				if (defender.HasRevengePower(RevengeMonsterPower.SummonAmbush))
				{
					num5 += 2;
				}
				if (dungeonLevel.SummonMonsters(dungeonLevel, defender.X, defender.Y, num5, num4, () => CreateMonster(Game.Instance.MonsterPool.GetMonsterDefinition(defender.MonsterType).Template)) > 0)
				{
					dungeonLevel.Message(defender, " calls forth hidden brethren!", more: true);
					defender.SpecialAbility = SpecialAbility.None;
				}
			}
		}
		else if (defender.SpecialAbility == SpecialAbility.Confuse && !ResistsConfusion)
		{
			if (IsConfused != 0)
			{
				dungeonLevel.Message(this, Name + " feels even more dizzy!");
			}
			else
			{
				dungeonLevel.Message(this, Name + " is totally confused by the scintillating lights of " + defender.Name.ToLower() + "!");
			}
			if (IsConfused >= 0)
			{
				IsConfused += Game.Instance.Roll(2, 2);
			}
		}
	}

	protected virtual int GetMeleeRollToHit(int numberOfMeleeAttacks)
	{
		return Game.Instance.Random(20 + ToHitBonus + GetMultiAttackBonus(numberOfMeleeAttacks));
	}

	protected virtual int GetMultiAttackBonus(int numberOfMeleeAttacks)
	{
		return 0;
	}

	private Being CreateMonster(Being summoner)
	{
		Being being = new Being(summoner);
		being.HitPoints = being.MaxHitPoints;
		being.SpecialAbility = SpecialAbility.None;
		being.Mood = Mood.Hunting;
		return being;
	}

	public void ResetAutomaticAction()
	{
		if (AutomaticAction != null)
		{
			AutomaticAction = null;
		}
	}

	public virtual void SufferDamage(DungeonLevel dungeonLevel, Being attacker, int total, string deathCause = null)
	{
		if (total > 0) global::Grog.Kernel.GCurses.Term.Sound(this is Player ? "hurt" : attacker is Player ? "hit" : null); //RVIP 6b
		if (IsAffectedBySpecialRoomPower(SpecialRoomPower.DoubleDamage, SpecialRoomPower.DoubleDamageByMonsters, SpecialRoomPower.TripleDamage, SpecialRoomPower.TripleDamageByMonsters) && attacker != null)
		{
			bool flag = !(attacker is Player);
			switch (dungeonLevel.GetRoomAt(X, Y).SpecialPower)
			{
			case SpecialRoomPower.DoubleDamage:
				total *= 2;
				break;
			case SpecialRoomPower.TripleDamage:
				total *= 3;
				break;
			case SpecialRoomPower.DoubleDamageByMonsters:
				if (flag)
				{
					total *= 2;
				}
				break;
			case SpecialRoomPower.TripleDamageByMonsters:
				if (flag)
				{
					total *= 3;
				}
				break;
			}
		}
		HitPoints -= total;
		if (HitPoints < 1)
		{
			dungeonLevel.Message(this, Name + " is " + ((Race == Race.Undead) ? "destroyed" : "killed") + "!", this is Player, renderMap: true, "['/' to continue]", '/');
			Game.Instance.RevengeSystem.UpliftRevengeMonster(dungeonLevel, attacker, this);
			Game.Instance.MonsterPool.NoteKill(MonsterType);
			attacker?.GainExperience(dungeonLevel, ExperienceValue);
			WhenKilled(dungeonLevel, attacker);
			LeaveMap(dungeonLevel);
		}
	}

	protected virtual void WhenKilled(DungeonLevel dungeonLevel, Being attacker)
	{
	}

	protected virtual void LeaveMap(DungeonLevel dungeonLevel)
	{
		dungeonLevel.RemoveThing(X, Y);
	}

	public void DrainStrength(DungeonLevel dungeonLevel, Being attacker, int amount = 1, bool isSilent = false)
	{
		if (amount > 0)
		{
			Strength -= amount;
			if (!isSilent)
			{
				dungeonLevel.Message(this, Name + " is drained of some strength!");
			}
			if (Strength < 1)
			{
				DieOfStrengthDrain(dungeonLevel, attacker);
				LeaveMap(dungeonLevel);
			}
		}
	}

	protected virtual void DieOfStrengthDrain(DungeonLevel dungeonLevel, Being attacker)
	{
		Game.Instance.MonsterPool.NoteKill(MonsterType);
		attacker?.GainExperience(dungeonLevel, ExperienceValue);
	}

	public void DrainConstitution(DungeonLevel dungeonLevel, Being attacker, int amount = 1, bool isSilent = false)
	{
		if (amount > 0)
		{
			Constitution -= amount;
			if (!isSilent)
			{
				dungeonLevel.Message(this, Name + " is partially drained of his vigor!");
			}
			if (Constitution < 1)
			{
				DieOfConstitutionDrain(dungeonLevel, attacker);
				LeaveMap(dungeonLevel);
			}
		}
	}

	protected virtual void DieOfConstitutionDrain(DungeonLevel dungeonLevel, Being attacker)
	{
		Game.Instance.MonsterPool.NoteKill(MonsterType);
		attacker?.GainExperience(dungeonLevel, ExperienceValue);
	}

	public void DrainDexterity(DungeonLevel dungeonLevel, Being attacker, int amount = 1, bool isSilent = false)
	{
		if (amount > 0)
		{
			Dexterity -= amount;
			if (!isSilent)
			{
				dungeonLevel.Message(this, Name + " is drained of some agility!");
			}
			if (Dexterity < 1)
			{
				DieOfDexterityDrain(dungeonLevel, attacker);
				LeaveMap(dungeonLevel);
			}
		}
	}

	protected virtual void DieOfDexterityDrain(DungeonLevel dungeonLevel, Being attacker)
	{
		Game.Instance.MonsterPool.NoteKill(MonsterType);
		attacker?.GainExperience(dungeonLevel, ExperienceValue);
	}

	public virtual void GainExperience(DungeonLevel dungeonLevel, int xp)
	{
	}

	public override void HandleEffects(DungeonLevel dungeonLevel)
	{
		base.HandleEffects(dungeonLevel);
		if (TurnsOfInvisibility > 0)
		{
			TurnsOfInvisibility--;
			if (TurnsOfInvisibility == 0 && !IsInvisible)
			{
				dungeonLevel.Message(this, Name + " suddenly is visible again!");
				dungeonLevel.More();
			}
		}
		if (IsPoisoned > 0)
		{
			SufferDamage(dungeonLevel, null, 1 + IsPoisoned / 10, "by poison");
			IsPoisoned--;
			if (IsPoisoned == 0)
			{
				dungeonLevel.Message(this, Name + " recovers from the poisoning.");
				dungeonLevel.More();
			}
		}
		if (IsSick > 0)
		{
			IsSick--;
			if (IsSick == 0)
			{
				dungeonLevel.Message(this, Name + " recovers from the rotting disease...");
				dungeonLevel.More();
			}
		}
		if (IsConfused > 0)
		{
			IsConfused--;
			if (IsConfused == 0)
			{
				dungeonLevel.Message(this, Name + " can think clearly once more!");
				dungeonLevel.More();
			}
		}
		if (IsParalyzed > 0)
		{
			IsParalyzed--;
			if (IsParalyzed == 0)
			{
				dungeonLevel.Message(this, Name + " can move again!");
				dungeonLevel.More();
			}
		}
		if (IsBlind > 0)
		{
			IsBlind--;
			if (IsBlind == 0)
			{
				dungeonLevel.Message(this, Name + " can see again!");
				dungeonLevel.More();
			}
		}
		if (IsHastened > 0)
		{
			IsHastened--;
			if (IsHastened == 0)
			{
				dungeonLevel.Message(this, Name + " slows down!", more: true);
			}
		}
		else if (IsHastened < 0)
		{
			IsHastened++;
			if (IsHastened == 0)
			{
				dungeonLevel.Message(this, Name + " speeds up!", more: true);
			}
		}
		if (IsDrunk > 0)
		{
			IsDrunk--;
			if (IsDrunk == 0)
			{
				dungeonLevel.Message(this, Name + " sobers up!");
				dungeonLevel.More();
			}
		}
		if (IsSleeping > 0)
		{
			IsSleeping--;
			if (IsSleeping == 0)
			{
				dungeonLevel.Message(this, Name + " wakes up again!");
				dungeonLevel.More();
			}
		}
		if (IsStunned > 0)
		{
			IsStunned--;
			if (IsStunned == 0)
			{
				dungeonLevel.Message(this, Name + " no longer is stunned!");
				dungeonLevel.More();
			}
		}
		if (IsLevitating > 0)
		{
			IsLevitating--;
			if (IsLevitating == 0)
			{
				dungeonLevel.Message(this, Name + " floats to the ground again!");
				dungeonLevel.More();
			}
		}
		Heal(dungeonLevel);
	}

	protected virtual void Heal(DungeonLevel dungeonLevel)
	{
		if (IsSick == 0)
		{
			if (HitPoints < MaxHitPoints && Moves % 20 == 0)
			{
				HitPoints++;
			}
			if (HitPoints < MaxHitPoints && (SpecialAbility == SpecialAbility.Regeneration || HasRevengePower(RevengeMonsterPower.Regeneration)))
			{
				HitPoints = Math.Min(MaxHitPoints, HitPoints + FirstAbilityValue + (HasRevengePower(RevengeMonsterPower.Regeneration) ? Math.Max(Level / 6, 1) : 0));
			}
		}
	}

	public virtual bool CanSee(DungeonLevel dungeonLevel, int x, int y)
	{
		if (IsBlind != 0)
		{
			return false;
		}
		IRoom roomAt = dungeonLevel.GetRoomAt(X, Y);
		if (roomAt != null)
		{
			IRoom roomAt2 = dungeonLevel.GetRoomAt(x, y);
			if (roomAt2 != null)
			{
				if (roomAt.IsDark && (!roomAt.IsWallOfRoom(x, y) || Math.Abs(x - X) > 1 || Math.Abs(y - Y) > 1))
				{
					return false;
				}
				return roomAt2 == roomAt;
			}
		}
		if (Math.Abs(x - X) <= 1)
		{
			return Math.Abs(y - Y) <= 1;
		}
		return false;
	}

	public bool CanSee(DungeonLevel dungeonLevel, Thing thing)
	{
		if (thing == this)
		{
			return true;
		}
		if (thing is Being { IsInvisible: not false } && !CanSeeInvisible)
		{
			return false;
		}
		if (thing is Player { IsAlive: false })
		{
			return true;
		}
		return CanSee(dungeonLevel, thing.X, thing.Y);
	}

	protected virtual void CauseDamage(DungeonLevel dungeonLevel, Being attacker, Being target, int damageIndex)
	{
		int num = NaturalDamage[damageIndex].GetDieResult();
		if (attacker.Race == Race.Undead && attacker.IsAffectedBySpecialRoomPower(SpecialRoomPower.MightyUndead))
		{
			num += 6;
		}
		if (target.AggravatesMonsters)
		{
			num += 2;
		}
		num = Math.Max(1, num);
		if (!dungeonLevel.Message(target, attacker.Name + " hits " + target.Name + " for " + num + " damage!"))
		{
			attacker.Message(" hits something!");
		}
		target.SufferDamage(dungeonLevel, attacker, num);
		if (target.IsAlive && (attacker.SpecialAbility == SpecialAbility.DrainStrength || attacker.HasRevengePower(RevengeMonsterPower.DrainStrength)) && Game.Instance.Probability(Math.Max((attacker.SpecialAbility == SpecialAbility.DrainStrength) ? attacker.FirstAbilityValue : 0, attacker.HasRevengePower(RevengeMonsterPower.DrainStrength) ? 40 : 0) - target.Level))
		{
			dungeonLevel.Message(" feels weak!");
			target.DrainStrength(dungeonLevel, attacker);
		}
		if (target.IsAlive && (attacker.SpecialAbility == SpecialAbility.DrainConstitution || attacker.HasRevengePower(RevengeMonsterPower.DrainConstitution)) && Game.Instance.Probability(Math.Max((attacker.SpecialAbility == SpecialAbility.DrainConstitution) ? attacker.FirstAbilityValue : 0, attacker.HasRevengePower(RevengeMonsterPower.DrainConstitution) ? 40 : 0) - target.Level))
		{
			dungeonLevel.Message(" feels exhausted!");
			target.DrainConstitution(dungeonLevel, attacker);
		}
		if (target.IsAlive && (attacker.SpecialAbility == SpecialAbility.Poison || attacker.SpecialAbility == SpecialAbility.PoisonAndBurrow || attacker.SpecialAbility == SpecialAbility.SpiderWebAndPoison || attacker.HasRevengePower(RevengeMonsterPower.DrainConstitution)) && !target.ResistsPoison)
		{
			int probability = Math.Max(attacker.HasRevengePower(RevengeMonsterPower.DrainConstitution) ? 20 : 0, (attacker.SpecialAbility == SpecialAbility.Poison || attacker.SpecialAbility == SpecialAbility.PoisonAndBurrow || attacker.SpecialAbility == SpecialAbility.SpiderWebAndPoison) ? attacker.FirstAbilityValue : 0);
			int num2 = (attacker.HasRevengePower(RevengeMonsterPower.DrainConstitution) ? Math.Max(Level / 6, 1) : 0);
			if (attacker.SpecialAbility == SpecialAbility.Poison || attacker.SpecialAbility == SpecialAbility.PoisonAndBurrow || attacker.SpecialAbility == SpecialAbility.SpiderWebAndPoison)
			{
				num2 += attacker.SecondAbilityValue;
			}
			if (Game.Instance.Probability(probability))
			{
				dungeonLevel.Message(" is poisoned!");
				target.IsPoisoned += Game.Instance.Random(num2) + 1;
			}
		}
		if (target.IsAlive && (attacker.SpecialAbility == SpecialAbility.ParalyzeOnHitOrBeingHit || attacker.HasRevengePower(RevengeMonsterPower.ParalyzeOnTouch)) && !target.ResistsParalyzation)
		{
			int num3 = Math.Max((attacker.SpecialAbility == SpecialAbility.ParalyzeOnHitOrBeingHit) ? attacker.FirstAbilityValue : 0, attacker.HasRevengePower(RevengeMonsterPower.ParalyzeOnTouch) ? 20 : 0);
			if (Game.Instance.Probability(Math.Max(3, num3 - target.Level * 3)))
			{
				if (!dungeonLevel.Message(attacker, "The touch of " + attacker.Name + " paralyzes " + target.Name + "!"))
				{
					dungeonLevel.Message("Something touches and paralyzes " + target.Name + "!");
				}
				target.IsParalyzed += Game.Instance.Roll(2, 4);
			}
		}
		if (target.IsAlive && attacker.SpecialAbility == SpecialAbility.CauseDisease && Game.Instance.Random(100) < attacker.FirstAbilityValue - (target.Constitution - 10 >> 1))
		{
			dungeonLevel.Message(target, " is infected with a rotting disease!");
			target.IsSick += attacker.SecondAbilityValue - target.Constitution * 2;
			dungeonLevel.More();
		}
		if (target.IsAlive && attacker.SpecialAbility == SpecialAbility.StealGold && Game.Instance.Random(100) > target.Dexterity * 5 && target.Gold > 0)
		{
			int num4 = Math.Max(1, target.Gold * (attacker.FirstAbilityValue + Game.Instance.Random(attacker.SecondAbilityValue)) / 100);
			dungeonLevel.Message(target, " is robbed by the " + attacker.Name + "!");
			target.Gold -= num4;
			attacker.Gold += num4;
			int num5 = Math.Max(1, num4 / 3);
			attacker.MaxHitPoints += Math.Min(attacker.HitDice * attacker.HitDieSides + attacker.HitDieBonus, num5);
			attacker.HitPoints = Math.Min(attacker.MaxHitPoints, attacker.HitPoints + num5);
			dungeonLevel.Message(attacker, " suddenly disappears with a mischievous chuckle.");
			dungeonLevel.Teleport(attacker);
			dungeonLevel.Message(attacker, " reappears looking surprised!");
		}
		if (target.IsAlive && attacker.SpecialAbility == SpecialAbility.RustOnHit && target is Player player && Game.Instance.Random(100) < attacker.FirstAbilityValue)
		{
			List<Item> equippedItems = player.Inventory.GetEquippedItems();
			List<Item> list = new List<Item>();
			foreach (Item item2 in equippedItems)
			{
				if (item2.IsMetallic)
				{
					list.Add(item2);
				}
			}
			if (list.Count > 0)
			{
				Item item = list[Game.Instance.Random(list.Count)];
				if (player.Inventory.HasEquippedItemWithAbility(ItemAbility.ResistRust))
				{
					dungeonLevel.Message(target, attacker.Name + " touches the " + item.Name + " but nothing happens!");
				}
				else
				{
					dungeonLevel.Message(target, attacker.Name + " touches the " + item.Name + " turning it into rusty dust!");
					player.Inventory.Remove(item);
				}
			}
		}
		if (target.IsAlive)
		{
			DamagesEquipment(dungeonLevel, attacker, target);
		}
	}

	private bool DamagesEquipment(DungeonLevel dungeonLevel, Being attacker, Being defender)
	{
		bool result = false;
		if (defender is Player player)
		{
			if (attacker.HasRevengePower(RevengeMonsterPower.SmashArmor))
			{
				Item equippedOrRandomItemOfType = player.Inventory.GetEquippedOrRandomItemOfType(ItemType.Armor);
				if (equippedOrRandomItemOfType != null && Game.Instance.Random(equippedOrRandomItemOfType.Value + 5) > equippedOrRandomItemOfType.Value)
				{
					if (!dungeonLevel.Message(attacker, "'s " + equippedOrRandomItemOfType.ShortDescription + " is shattered!", more: true))
					{
						dungeonLevel.Message("A mighty force shatters the " + equippedOrRandomItemOfType.ShortDescription + " of " + player.Name + "!", more: true);
					}
					player.Inventory.Remove(equippedOrRandomItemOfType);
				}
			}
			if (attacker.HasRevengePower(RevengeMonsterPower.SmashShield))
			{
				Item equippedOrRandomItemOfType2 = player.Inventory.GetEquippedOrRandomItemOfType(ItemType.Shield);
				if (equippedOrRandomItemOfType2 != null && Game.Instance.Random(equippedOrRandomItemOfType2.Value + 20) > equippedOrRandomItemOfType2.Value)
				{
					if (!dungeonLevel.Message(attacker, "'s " + equippedOrRandomItemOfType2.ShortDescription + " is shattered!", more: true))
					{
						dungeonLevel.Message("A mighty blow shatters the " + equippedOrRandomItemOfType2.ShortDescription + " of " + player.Name + "!", more: true);
					}
					player.Inventory.Remove(equippedOrRandomItemOfType2);
				}
			}
			if (attacker.HasRevengePower(RevengeMonsterPower.SmashWeapon))
			{
				Item equippedOrRandomItemOfType3 = player.Inventory.GetEquippedOrRandomItemOfType(ItemType.MeleeWeapon);
				if (equippedOrRandomItemOfType3 != null && Game.Instance.Random(equippedOrRandomItemOfType3.Value + 10) > equippedOrRandomItemOfType3.Value)
				{
					if (!dungeonLevel.Message(attacker, "'s " + equippedOrRandomItemOfType3.ShortDescription + " breaks in a futile parry attempt!", more: true))
					{
						dungeonLevel.Message("'s " + equippedOrRandomItemOfType3.ShortDescription + " is swept aside and breaks!", more: true);
					}
					player.Inventory.Remove(equippedOrRandomItemOfType3);
				}
			}
			if (attacker.HasRevengePower(RevengeMonsterPower.Disarm))
			{
				Item equippedOrRandomItemOfType4 = player.Inventory.GetEquippedOrRandomItemOfType(ItemType.MeleeWeapon);
				if (equippedOrRandomItemOfType4 != null && Game.Instance.Probability(Math.Max(5, defender.Level * 4 - (attacker.Level + ((RevengeMonster)attacker).RevengeLevel) * 3)))
				{
					if (!dungeonLevel.Message(attacker, " disarms the " + equippedOrRandomItemOfType4.ShortDescription + " of " + player.Name + ". It drops to the ground!", more: true))
					{
						dungeonLevel.Message(" is disarmed and drops the " + equippedOrRandomItemOfType4.ShortDescription + "!", more: true);
					}
					player.Inventory.Remove(equippedOrRandomItemOfType4);
					dungeonLevel.AddItem(player.X, player.Y, equippedOrRandomItemOfType4);
				}
			}
		}
		return result;
	}

	protected virtual void Message(string message)
	{
	}

	public bool IsAffectedBySpecialRoomPower(params SpecialRoomPower[] specialRoomPower)
	{
		if (Game.Instance.DungeonMaster.CurrentDungeonLevel == null)
		{
			return false;
		}
		if (!Game.Instance.DungeonMaster.CurrentDungeonLevel.IsValid(X, Y))
		{
			return false;
		}
		IRoom roomAt = Game.Instance.DungeonMaster.CurrentDungeonLevel.GetRoomAt(X, Y);
		if (roomAt == null)
		{
			return false;
		}
		if (!roomAt.IsInsideOfRoom(X, Y))
		{
			return false;
		}
		for (int i = 0; i < specialRoomPower.Length; i++)
		{
			if (specialRoomPower[i] == roomAt.SpecialPower)
			{
				return true;
			}
		}
		return false;
	}
}
