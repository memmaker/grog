using System;
using System.Collections.Generic;
using Grog.Dressings.Items;
using Grog.Dressings.Items.Implementations.Food;
using Grog.Dressings.Items.Inventory;
using Grog.Dressings.Items.Types;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Monsters;
using Grog.Dungeons.Generators.Rooms;
using Grog.Kernel;
using Grog.Systems.Revenge;

namespace Grog.Dressings.Beings;

[Serializable]
public class Player : Being
{
	public const int MaxExperienceLevel = 25;

	public const int ExperienceForLevelZero = 250;

	public const int MaxRequiredExperience = 2146483647;

	private int _strength;

	private int _dexterity;

	private int _constitution;

	private int _freePreachings;

	public int Feelings { get; set; }

	public int Experience { get; private set; }

	public bool CanGainLevel => Level < 25;

	public bool CanGainLevelNow
	{
		get
		{
			if (CanGainLevel)
			{
				return Experience >= ExperienceForNextLevel;
			}
			return false;
		}
	}

	public int ExperienceForNextLevel => 250 * Level * (Level + 1) / 2;

	public string MissingExperienceForNextLevel
	{
		get
		{
			if (Level >= 25)
			{
				return "-";
			}
			return (ExperienceForNextLevel - Experience).ToString();
		}
	}

	public int MissingExperiencePointsForNextLevel
	{
		get
		{
			if (Level >= 25)
			{
				return 0;
			}
			return ExperienceForNextLevel - Experience;
		}
	}

	public override bool IsImmuneToFire => false;

	public override int Strength
	{
		get
		{
			int num = _strength + ((Inventory != null) ? (Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.StrengthPlusOne) + Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.StrengthPlusTwo) * 2 + Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.StrengthPlusThree) * 3) : 0) + ((IsPoisoned != 0 && !ResistsStrengthDrain) ? (-3) : 0);
			if (IsAffectedBySpecialRoomPower(SpecialRoomPower.HalvedStrength))
			{
				num /= 2;
			}
			if (IsSick != 0)
			{
				num = Math.Max(1, num - 2);
			}
			return num;
		}
		set
		{
			_strength = value;
		}
	}

	public override int Dexterity
	{
		get
		{
			int num = _dexterity + ((Inventory != null) ? (Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.DexterityPlusOne) + Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.DexterityPlusTwo) * 2 + Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.DexterityPlusThree) * 3) : 0);
			if (IsAffectedBySpecialRoomPower(SpecialRoomPower.HalvedDexterity))
			{
				num /= 2;
			}
			if (IsSick != 0)
			{
				num = Math.Max(1, num - 2);
			}
			return num;
		}
		set
		{
			_dexterity = value;
		}
	}

	public override int Constitution
	{
		get
		{
			return _constitution + ((Inventory != null) ? (Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.ConstitutionPlusOne) + Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.ConstitutionPlusTwo) * 2 + Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.ConstitutionPlusThree) * 3) : 0) + ((IsSick != 0) ? (-3) : 0);
		}
		set
		{
			_constitution = value;
		}
	}

	public override string Name => ChristenedName;

	public string Gender { get; }

	public override int NumberOfMeleeAttacks => Math.Max(1, Inventory.GetEquippedItemsOfType(ItemType.MeleeWeapon).Count);

	public bool HasLeftDungeon { get; set; }

	public bool HasGainedImmortality { get; set; }

	public Inventory Inventory { get; }

	public override int ToHitBonus => base.ToHitBonus + Inventory.EquippedToHitBonus;

	public override int ArmorClass
	{
		get
		{
			return base.ArmorClass + (Inventory?.EquippedArmorClassBonus ?? 0);
		}
		set
		{
			base.ArmorClass = value;
		}
	}

	public override bool ResistsConfusion => Inventory.HasEquippedItemWithAbility(ItemAbility.ImmunityToConfusion);

	public override bool ResistsParalyzation => Inventory.HasEquippedItemWithAbility(ItemAbility.ImmunityToParalyzation);

	public override bool ResistsSickness => Inventory.HasEquippedItemWithAbility(ItemAbility.ImmunityToSickness);

	public override bool ResistsBlindness => Inventory.HasEquippedItemWithAbility(ItemAbility.ImmunityToBlindness);

	public override bool ResistsIntoxication
	{
		get
		{
			if (!base.ResistsIntoxication)
			{
				return Inventory.HasEquippedItemWithAbility(ItemAbility.ImmunityToIntoxication);
			}
			return true;
		}
	}

	public override bool ResistsPoison
	{
		get
		{
			if (!base.ResistsPoison)
			{
				return Inventory.HasEquippedItemWithAbility(ItemAbility.ImmunityToPoison);
			}
			return true;
		}
	}

	public override bool ResistsSleep => Inventory.HasEquippedItemWithAbility(ItemAbility.ImmunityToSleep);

	public override bool CanSeeInvisible => Inventory.HasEquippedItemWithAbility(ItemAbility.SeeInvisible);

	public override bool ResistsStrengthDrain => Inventory.HasEquippedItemWithAbility(ItemAbility.SustainStrength);

	public override bool AggravatesMonsters => Inventory.HasEquippedItemWithAbility(ItemAbility.MonsterAggravation);

	public override bool CanMoveFreely => Inventory.HasEquippedItemWithAbility(ItemAbility.MoveFreely);

	public override bool IsInvisible
	{
		get
		{
			if (!base.IsInvisible)
			{
				return Inventory.HasEquippedItemWithAbility(ItemAbility.Invisibility);
			}
			return true;
		}
	}

	public override bool IsAlive
	{
		get
		{
			if (base.IsAlive)
			{
				return SatiationLevel != SatiationLevel.Starved;
			}
			return false;
		}
	}

	public bool HandsOfConfusion { get; set; }

	public int Satiation { get; set; }

	public SatiationLevel SatiationLevel
	{
		get
		{
			if (Satiation <= 0)
			{
				return SatiationLevel.Starved;
			}
			if (Satiation <= 200)
			{
				return SatiationLevel.VeryHungry;
			}
			if (Satiation <= 500)
			{
				return SatiationLevel.Hungry;
			}
			if (Satiation >= 3000)
			{
				return SatiationLevel.Overfed;
			}
			if (Satiation >= 1600)
			{
				return SatiationLevel.Satiated;
			}
			return SatiationLevel.Average;
		}
	}

	public long Piety { get; set; }

	public int NumberOfPrayersUttered { get; set; }

	public int NumberOfFreePrayers
	{
		get
		{
			int num = Level + 1 + _freePreachings;
			if (Piety >= 1500)
			{
				num += 2;
				long num2 = Piety - 1500;
				int num3 = 1500;
				int num4 = 1000 + num3;
				while (num2 >= num4)
				{
					num++;
					num2 -= num4;
					int num5 = num3;
					num3 = num4;
					num4 = num5 + num3;
				}
			}
			else if (Piety >= 500)
			{
				num++;
			}
			return num;
		}
		set
		{
			int numberOfFreePrayers = NumberOfFreePrayers;
			int num = value - numberOfFreePrayers;
			_freePreachings += num;
		}
	}

	public int PietyLevel => NumberOfFreePrayers - NumberOfPrayersUttered;

	public bool DidCheat { get; set; }

	public bool IsChampion { get; set; }

	public bool HasAccessToCheatMode
	{
		get
		{
			if ("brannalbin".Equals(ChristenedName, StringComparison.InvariantCultureIgnoreCase) && "male".Equals(Gender, StringComparison.InvariantCultureIgnoreCase))
			{
				return "wizard".Equals(Type, StringComparison.InvariantCultureIgnoreCase);
			}
			return false;
		}
	}

	public int MultiAttackBonus => GetMultiAttackBonus(NumberOfMeleeAttacks);

	public int LastPrayerTurn { get; set; }

	public long InitialScore { get; }

	public Player(string name, string gender, string type)
		: base(MonsterPool.MonsterType.Grog, Race.Player, name, type, RevengeMonsterProgression.None, '@', 1, 10, 0, 0, 14, new Roll[1]
		{
			new Roll(1, 3)
		})
	{
		Gender = gender;
		Experience = 0;
		ulong seed = (ulong)GetCharacterSeed(name, gender, type);
		ulong num = 0uL;
		Strength = 6;
		Dexterity = 4;
		Constitution = 5;
		Satiation = 1000;
		for (int i = 0; i < 27; i++)
		{
			switch (SplitMix64(seed, num++) % 3)
			{
			case 2uL:
				Strength++;
				break;
			case 1uL:
				Dexterity++;
				break;
			default:
				Constitution++;
				break;
			}
		}
		HitPoints = (MaxHitPoints += ((Constitution < 10) ? (Constitution - 10 >> 1) : (Constitution - 10)));
		Act = null;
		Inventory = new Inventory(this);
		Inventory.EquipInitially(Game.Instance.ItemPool.CreateItem(ItemId.LeatherArmor));
		Inventory.EquipInitially(Game.Instance.ItemPool.CreateItem((SplitMix64(seed, num++) % 2 == 0L) ? ItemId.BucklerShield : ItemId.RoundShield));
		Inventory.EquipInitially(Game.Instance.ItemPool.CreateItem((SplitMix64(seed, num++) % 2 == 0L) ? ItemId.Mace : ((SplitMix64(seed, num++) % 2 == 0L) ? ItemId.ShortSword : ItemId.HandAxe)));
		InitialScore = Game.Instance.HighscoreManager.GetScoreFor(this);
	}

	private ulong SplitMix64(ulong seed, ulong i, ulong gamma = 11400714819323198485uL)
	{
		ulong num = seed + i * gamma;
		long num2 = (long)(num ^ (num >> 30)) * -4658895280553007687L;
		long num3 = (num2 ^ (num2 >>> 27)) * -7723592293110705685L;
		return (ulong)(num3 ^ (num3 >>> 31));
	}

	private int GetCharacterSeed(string name, string gender, string type)
	{
		return (name + ", the " + gender + " " + type).GetHashCode();
	}

	public PlayerMeleeCapabilities GetMeleeCapabilities()
	{
		List<Item> equippedItemsOfType = Inventory.GetEquippedItemsOfType(ItemType.MeleeWeapon);
		if (equippedItemsOfType.Count == 0)
		{
			return new PlayerMeleeCapabilities(MeleeCapability.Unarmed, new Roll[1]
			{
				new Roll(1, 3, ToDamageBonus + Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.DamagePlusTwo) * 2 + Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.DamagePlusFour) * 4 + Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.DamagePlusSix) * 6)
			});
		}
		Roll[] array = new Roll[equippedItemsOfType.Count];
		for (int i = 0; i < equippedItemsOfType.Count; i++)
		{
			array[i] = new Roll(equippedItemsOfType[i].MeleeDamage.NumberOfDice, equippedItemsOfType[i].MeleeDamage.DieSides, equippedItemsOfType[i].MeleeDamage.DieBonus + ToDamageBonus + equippedItemsOfType[i].ItemBonus + Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.DamagePlusTwo) * 2 + Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.DamagePlusFour) * 4 + Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.DamagePlusSix) * 6);
		}
		return new PlayerMeleeCapabilities(MeleeCapability.Armed, array);
	}

	protected override void CauseDamage(DungeonLevel dungeonLevel, Being attacker, Being target, int damageIndex)
	{
		Game.Instance.MonsterPool.LearnHitDice(target);
		target.HitPointsKnown = true;
		List<Item> equippedItemsOfType = Inventory.GetEquippedItemsOfType(ItemType.MeleeWeapon);
		if (equippedItemsOfType.Count == 0)
		{
			int num = Math.Max(1, Game.Instance.Random(3) + 1 + attacker.ToDamageBonus);
			num += Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.DamagePlusTwo) * 2 + Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.DamagePlusFour) * 4 + Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.DamagePlusSix) * 6;
			num = Math.Max(1, num);
			dungeonLevel.Message(attacker, " punches " + target.Name + " for " + num + " damage!");
			target.SufferDamage(dungeonLevel, attacker, num);
		}
		else
		{
			int num2 = Math.Max(1, equippedItemsOfType[damageIndex].MeleeDamage.GetDieResult() + attacker.ToDamageBonus + equippedItemsOfType[damageIndex].ItemBonus);
			num2 += Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.DamagePlusTwo) * 2 + Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.DamagePlusFour) * 4 + Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.DamagePlusSix) * 6;
			equippedItemsOfType[damageIndex].IsIdentified = true;
			num2 = Math.Max(1, num2);
			string text = "";
			if ((equippedItemsOfType[damageIndex].HasItemAbility(ItemAbility.AnimalSlaying) && target.Race == Race.Animal) || (equippedItemsOfType[damageIndex].HasItemAbility(ItemAbility.DemonSlaying) && target.Race == Race.Demon) || (equippedItemsOfType[damageIndex].HasItemAbility(ItemAbility.DevilSlaying) && target.Race == Race.Devil) || (equippedItemsOfType[damageIndex].HasItemAbility(ItemAbility.DragonSlaying) && target.Race == Race.Dragon) || (equippedItemsOfType[damageIndex].HasItemAbility(ItemAbility.UndeadSlaying) && target.Race == Race.Undead))
			{
				int num3 = Game.Instance.Roll(1, 100);
				int num4 = 2;
				text = " strongly";
				if (num3 == 100)
				{
					num4 = 4;
					text = " critically";
				}
				else if (num3 > 80)
				{
					num4 = 3;
					text = " severely";
				}
				num2 *= num4;
			}
			dungeonLevel.Message(attacker, text + " hits " + target.Name + " for " + num2 + " damage!");
			target.SufferDamage(dungeonLevel, attacker, num2);
		}
		if (target.SpecialAbility == SpecialAbility.ParalyzeOnHitOrBeingHit && Game.Instance.Probability(target.SecondAbilityValue - attacker.Level * 3) && !attacker.ResistsParalyzation)
		{
			if (!dungeonLevel.Message(target, Name + " is paralyzed by the splattering blood of " + target.Name + "!"))
			{
				dungeonLevel.Message(" is covered by some liquid, being paralyzed by it!");
			}
			attacker.IsParalyzed += Game.Instance.Roll(2, 4);
		}
		if (target.HasRevengePower(RevengeMonsterPower.Freeze) && !CanMoveFreely && Game.Instance.Probability(target.Level * 4 - attacker.Level * 3))
		{
			if (!dungeonLevel.Message(target, Name + " is slowed down by the ice aura of " + target.Name + "!"))
			{
				dungeonLevel.Message(" is frozen by icy cold!");
			}
			attacker.IsFrozen += Game.Instance.Roll(2, 4 + (target.Level + ((RevengeMonster)target).RevengeLevel) / 3);
		}
		if (target.IsAlive && attacker == this && HandsOfConfusion && !target.ResistsConfusion)
		{
			dungeonLevel.Message(target.Name + " appears confused!");
			target.IsConfused += new Roll(2, 6).GetDieResult();
			HandsOfConfusion = false;
		}
	}

	public override void GainExperience(DungeonLevel dungeonLevel, int xp)
	{
		Experience = Math.Min(2146483647, Experience + xp);
		if (!CanGainLevelNow)
		{
			return;
		}
		int num = Math.Max(dungeonLevel.Level / 3, Game.Instance.Random(8) + 2 + ((Constitution < 10) ? (Constitution - 10 >> 1) : (Constitution - 10)));
		HitPoints += num;
		MaxHitPoints += num;
		dungeonLevel.Message(" gains a level (+" + num + " H)!");
		Level++;
		if (Level % 3 == 0)
		{
			switch (Game.Instance.Random(3))
			{
			case 0:
				dungeonLevel.Message(" grows stronger (+1 St)!");
				Strength++;
				break;
			case 1:
				dungeonLevel.Message(" becomes more agile (+1 Dx)!");
				Dexterity++;
				break;
			case 2:
				dungeonLevel.Message(" feels tougher (+1 Cn)!");
				Constitution++;
				break;
			}
		}
		dungeonLevel.More();
	}

	protected override int GetMissileCombatRollToHit()
	{
		return Game.Instance.Random(20) + ToHitBonus + 1;
	}

	protected override int GetMeleeRollToHit(int numberOfMeleeAttacks)
	{
		return Game.Instance.Random(20) + ToHitBonus + GetMultiAttackBonus(numberOfMeleeAttacks) + 1;
	}

	protected override bool WillAttack(Being attacker, Thing thing)
	{
		return thing is Being;
	}

	protected override int GetMultiAttackBonus(int numberOfMeleeAttacks)
	{
		if (numberOfMeleeAttacks != 1)
		{
			return -4 + Math.Max(Dexterity - 10, 0) / 5;
		}
		return 0;
	}

	public override void HandleEffects(DungeonLevel dungeonLevel)
	{
		base.HandleEffects(dungeonLevel);
		Inventory.IncrementItemOwnership(dungeonLevel, this);
		if (IsAffectedBySpecialRoomPower(SpecialRoomPower.RandomSummonings))
		{
			SummonMonsterRandomlyToTheCurrentRoom(dungeonLevel, this);
		}
		else if (IsAffectedBySpecialRoomPower(SpecialRoomPower.ItemRot))
		{
			PotentiallyDestroyItems(dungeonLevel, dungeonLevel.Level, (Item item) => true, " suddenly shatters!", (Item i) =>
			{
				Inventory.Remove(i);
			});
		}
		else if (IsAffectedBySpecialRoomPower(SpecialRoomPower.BugInfested))
		{
			PotentiallyDestroyItems(dungeonLevel, dungeonLevel.Level, (Item item) => item.ItemType == ItemType.Food, " is eaten by voracious bugs!", (Item i) =>
			{
				Inventory.Remove(i);
			});
		}
		else if (IsAffectedBySpecialRoomPower(SpecialRoomPower.Humid))
		{
			PotentiallyDestroyItems(dungeonLevel, dungeonLevel.Level + 5, (Item item) => item.ItemType == ItemType.Scroll, " is completely drenched!", (Item i) =>
			{
				Inventory.Remove(i);
			});
		}
		else if (IsAffectedBySpecialRoomPower(SpecialRoomPower.Rusting))
		{
			PotentiallyDestroyItems(dungeonLevel, Math.Max(1, dungeonLevel.Level / 3), (Item item) => item.IsMetallic, " turns into a small heap of rust!", (Item i) =>
			{
				Inventory.Remove(i);
			});
		}
		else if (IsAffectedBySpecialRoomPower(SpecialRoomPower.Rotting))
		{
			PotentiallyDestroyItems(dungeonLevel, dungeonLevel.Level + 3, (Item item) => item.ItemType == ItemType.Potion, " suddenly turns into a vile black liquid!", (Item i) =>
			{
				Inventory.Remove(i);
				Inventory.Add(Game.Instance.ItemPool.CreateItem(ItemId.PotionOfPoison));
			});
		}
		else if (IsAffectedBySpecialRoomPower(SpecialRoomPower.GoldRot) && Gold > 0)
		{
			int num = Math.Min(Game.Instance.Random(dungeonLevel.Level) + 1, Gold);
			Gold -= num;
			dungeonLevel.Message("The purse of " + Name + " suddenly feels lighter!");
			ResetAutomaticAction();
		}
		else
		{
			if (!IsAffectedBySpecialRoomPower(SpecialRoomPower.ManaCharged, SpecialRoomPower.PowerfulManaCharged))
			{
				return;
			}
			Item randomItem = Inventory.GetRandomItem((Item i) => i.CanBeModifiedWithPrefix || i.CanBeModifiedWithSuffix);
			if (randomItem == null)
			{
				return;
			}
			dungeonLevel.Message("All the energy of this rooms flows into the " + randomItem.Name + "! It glows briefly and then returns to normal.", more: true);
			int num2 = Game.Instance.Random(100) + 1;
			if (randomItem.CanBeModifiedWithPrefix || (randomItem.CanBeModifiedWithSuffix && num2 <= 50))
			{
				Game.Instance.ItemPool.ItemModifiers.AddPrefixModifier(dungeonLevel, randomItem);
			}
			else
			{
				Game.Instance.ItemPool.ItemModifiers.AddSuffixModifier(dungeonLevel, randomItem);
			}
			if (IsAffectedBySpecialRoomPower(SpecialRoomPower.PowerfulManaCharged))
			{
				dungeonLevel.Message("At the same time " + Name + " is drained and suddenly feels ");
				switch (Game.Instance.Random(3))
				{
				case 0:
					dungeonLevel.Message("very weak!");
					DrainStrength(dungeonLevel, null, 3);
					break;
				case 1:
					dungeonLevel.Message("like a clod!");
					DrainDexterity(dungeonLevel, null, 3);
					break;
				case 2:
					dungeonLevel.Message("very sick!");
					DrainConstitution(dungeonLevel, null, 3);
					break;
				}
			}
			if (IsAlive)
			{
				dungeonLevel.GetRoomAt(X, Y).IsSpecial = false;
			}
		}
	}

	public void PotentiallyDestroyItems(DungeonLevel dungeonLevel, int probability, Func<Item, bool> isApplicableItem, string destructionMessage, Action<Item> effect, int numberOfAffectedItems = 1)
	{
		List<Item> inventory = Inventory.GetInventory();
		List<Item> list = new List<Item>();
		int num = 0;
		int num2 = 0;
		foreach (Item item2 in inventory)
		{
			if (isApplicableItem(item2))
			{
				list.Add(item2);
				num += item2.Value + 5;
				num2 = Math.Max(item2.Value + 5, num2);
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < Math.Min(list.Count, numberOfAffectedItems); i++)
		{
			if (!Game.Instance.Probability(probability))
			{
				continue;
			}
			int num3 = Game.Instance.Random(num);
			Item item = null;
			foreach (Item item3 in list)
			{
				num3 -= num2 - item3.Value;
				if (num3 <= 0)
				{
					item = item3;
					break;
				}
			}
			if (item != null)
			{
				dungeonLevel.Message("The " + item.Name + destructionMessage, more: true);
				ResetAutomaticAction();
				effect(item);
				list.Remove(item);
			}
		}
	}

	private void SummonMonsterRandomlyToTheCurrentRoom(DungeonLevel dungeonLevel, Player grog)
	{
		if (!Game.Instance.Probability(10))
		{
			return;
		}
		Position insidePositionForThing = dungeonLevel.GetRoomAt(grog.X, grog.Y).GetInsidePositionForThing(dungeonLevel);
		if (insidePositionForThing.Equals(Position.Undefined))
		{
			return;
		}
		Being being = Game.Instance.MonsterPool.CreateRandomMonster(dungeonLevel);
		if (being != null)
		{
			dungeonLevel.SetBeing(insidePositionForThing.X, insidePositionForThing.Y, being);
			if (!dungeonLevel.Message(being, " jumps out of a portal that manifests as spontaneously as briefly!"))
			{
				dungeonLevel.Message(" feels a frightening manifestation of power!");
			}
		}
	}

	protected override void Heal(DungeonLevel dungeonLevel)
	{
		if (IsSick == 0 && !IsAffectedBySpecialRoomPower(SpecialRoomPower.NoHealing))
		{
			if (HitPoints < MaxHitPoints)
			{
				int num = 19 - Level * 2;
				if (num < 3)
				{
					if (Moves % 3 == 0)
					{
						HitPoints = Math.Min(MaxHitPoints, HitPoints + 1 + Game.Instance.Random(Level - 5 >> 1));
					}
				}
				else if (Moves % num == 0)
				{
					HitPoints++;
				}
			}
			int numberOfEquippedItemsWithAbility = Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.Regenerates);
			if (Moves % 2 == 0)
			{
				HitPoints = Math.Min(MaxHitPoints, HitPoints + numberOfEquippedItemsWithAbility);
			}
		}
		ConsumeFood(dungeonLevel);
	}

	private void ConsumeFood(DungeonLevel dungeonLevel)
	{
		SatiationLevel satiationLevel = SatiationLevel;
		if (IsAffectedBySpecialRoomPower(SpecialRoomPower.SatiationByMana))
		{
			dungeonLevel.Message(Name + " is satiated by divine mana!");
			Satiation += Game.Instance.Random(10) + 1;
			if (Game.Instance.Probability(1))
			{
				dungeonLevel.Message("Suddenly the special aura of this room dissipates!", more: true);
				dungeonLevel.GetRoomAt(X, Y).IsSpecial = false;
			}
		}
		else
		{
			int num = 1;
			if (IsAffectedBySpecialRoomPower(SpecialRoomPower.DoubleHunger))
			{
				num = 2;
			}
			else if (IsAffectedBySpecialRoomPower(SpecialRoomPower.TripleHunger))
			{
				num = 3;
			}
			else if (IsAffectedBySpecialRoomPower(SpecialRoomPower.QuadrulpleHunger))
			{
				num = 4;
			}
			else if (IsAffectedBySpecialRoomPower(SpecialRoomPower.Magnetic))
			{
				List<Item> filteredItems = Inventory.GetFilteredItems((Item i) => i.IsMetallic);
				if (filteredItems.Count > 0)
				{
					num = 1 + filteredItems.Count * (filteredItems.Count + 1) / 2;
					dungeonLevel.Message("The metal carried by " + Name + " is strongly dragged by magnetic forces making movement very strenous!");
				}
			}
			int numberOfEquippedItemsWithAbility = Inventory.GetNumberOfEquippedItemsWithAbility(ItemAbility.SlowDigestion);
			if (numberOfEquippedItemsWithAbility > 0)
			{
				if (Moves % (1 << numberOfEquippedItemsWithAbility) == 0)
				{
					Satiation -= num;
				}
			}
			else
			{
				Satiation -= num;
			}
		}
		SatiationLevel satiationLevel2 = SatiationLevel;
		if (satiationLevel2 != satiationLevel)
		{
			switch (satiationLevel2)
			{
			case SatiationLevel.Satiated:
				dungeonLevel.Message(this, Name + " is starting to feel less stuffed.");
				break;
			case SatiationLevel.Average:
				dungeonLevel.Message(this, Name + " no longer is satiated.");
				break;
			case SatiationLevel.Hungry:
				dungeonLevel.Message(this, Name + " becomes hungry.");
				ResetAutomaticAction();
				dungeonLevel.More();
				break;
			case SatiationLevel.VeryHungry:
				dungeonLevel.Message(this, Name + " is experiencing a sense of painful hunger.");
				ResetAutomaticAction();
				dungeonLevel.More();
				break;
			case SatiationLevel.Starving:
				dungeonLevel.Message(this, Name + " is near starvation!.");
				ResetAutomaticAction();
				dungeonLevel.More();
				break;
			case SatiationLevel.Starved:
				dungeonLevel.Message(this, Name + " dies of starvation!");
				ResetAutomaticAction();
				SufferDamage(dungeonLevel, null, HitPoints, "by starvation");
				break;
			}
		}
	}

	protected override void Message(string message)
	{
		Game.Instance.DungeonMaster.CurrentDungeonLevel.Message(message);
	}

	protected override void LeaveMap(DungeonLevel dungeonLevel)
	{
	}

	public override bool CanSee(DungeonLevel dungeonLevel, int x, int y)
	{
		if (X == -1 || Y == -1)
		{
			return true;
		}
		return base.CanSee(dungeonLevel, x, y);
	}

	public override void SufferDamage(DungeonLevel dungeonLevel, Being attacker, int total, string deathCause = null)
	{
		Game.Instance.MonsterPool.LearnDamageRoutine(attacker);
		base.SufferDamage(dungeonLevel, attacker, total, deathCause);
		if (!IsAlive)
		{
			if (attacker == null)
			{
				Game.Instance.DeathCause = deathCause;
			}
			else
			{
				Game.Instance.DeathCause = GetKillerName(attacker);
			}
			Game.Instance.Ghosts.CheckForGhostCreation(dungeonLevel, this, attacker);
		}
	}

	private string GetKillerName(Being attacker)
	{
		string text = "by " + attacker.Name;
		if (string.IsNullOrEmpty(attacker.ChristenedName))
		{
			text = text.Replace("by the ", "by a ");
		}
		return text;
	}

	protected override void DieOfStrengthDrain(DungeonLevel dungeonLevel, Being attacker)
	{
		attacker?.GainExperience(dungeonLevel, ExperienceValue);
		Game.Instance.DeathCause = ((attacker == null) ? "was drained of all strength" : ("was drained of all strength by " + GetKillerName(attacker)));
	}

	protected override void DieOfConstitutionDrain(DungeonLevel dungeonLevel, Being attacker)
	{
		Game.Instance.MonsterPool.NoteKill(MonsterType);
		attacker?.GainExperience(dungeonLevel, ExperienceValue);
		Game.Instance.DeathCause = ((attacker == null) ? "was drained of all life energy" : ("was drained of all life energy by " + GetKillerName(attacker)));
	}

	protected override void DieOfDexterityDrain(DungeonLevel dungeonLevel, Being attacker)
	{
		Game.Instance.MonsterPool.NoteKill(MonsterType);
		attacker?.GainExperience(dungeonLevel, ExperienceValue);
		Game.Instance.DeathCause = ((attacker == null) ? "was terminally paralyzed" : ("was terminally paralyzed by " + GetKillerName(attacker)));
	}

	public void GainPiety(DungeonLevel dungeonLevel, int piety)
	{
		int numberOfFreePrayers = NumberOfFreePrayers;
		Piety += piety;
		int numberOfFreePrayers2 = NumberOfFreePrayers;
		JudgePiety(dungeonLevel, numberOfFreePrayers, numberOfFreePrayers2);
	}

	public void JudgePiety(DungeonLevel dungeonLevel, int oldPrayers, int newPrayers)
	{
		if (oldPrayers < newPrayers)
		{
			dungeonLevel.Message(" feels recognized by Ta'ker'na!", more: true);
		}
		if (newPrayers > NumberOfPrayersUttered + 15)
		{
			dungeonLevel.Message(" feels favored by Ta'ker'na!", more: true);
		}
		else if (newPrayers > NumberOfPrayersUttered + 2)
		{
			dungeonLevel.Message(" feels extremely close to Ta'ker'na!", more: true);
		}
		else if (newPrayers == NumberOfPrayersUttered + 2)
		{
			dungeonLevel.Message(" feels very close to Ta'ker'na!", more: true);
		}
		else if (newPrayers == NumberOfPrayersUttered + 1)
		{
			dungeonLevel.Message(" feels close to Ta'ker'na!", more: true);
		}
		else if (newPrayers == NumberOfPrayersUttered)
		{
			dungeonLevel.Message(" feels spiritually removed from Ta'ker'na!", more: true);
		}
		else if (newPrayers == NumberOfPrayersUttered - 1)
		{
			dungeonLevel.Message(" feels Ta'ker'nas contempt!", more: true);
		}
		else if (newPrayers == NumberOfPrayersUttered - 2)
		{
			dungeonLevel.Message(" feels despised by Ta'ker'na!", more: true);
		}
		else
		{
			dungeonLevel.Message(" feels hated by Ta'ker'na!", more: true);
		}
	}

	public void ExecuteMovementBasedInteractions(DungeonLevel dungeonLevel)
	{
		if (IsAffectedBySpecialRoomPower(SpecialRoomPower.Slippery))
		{
			if (Game.Instance.Random(Dexterity + 3) > Dexterity)
			{
				dungeonLevel.Message(Name + " slips and falls down really hard! " + Name + " is stunned!");
				IsStunned += Game.Instance.Random(3) + 2;
			}
		}
		else if (IsAffectedBySpecialRoomPower(SpecialRoomPower.Fumbling) && Game.Instance.Random(Dexterity + 5) > Dexterity)
		{
			Item randomItem = Inventory.GetRandomItem((Item i) => i.IsEquipped);
			if (randomItem != null)
			{
				Inventory.Remove(randomItem);
				dungeonLevel.AddItem(X, Y, randomItem);
				dungeonLevel.Message(" fumbles and suddenly drops the " + randomItem.Name + "!", more: true);
			}
		}
	}
}
