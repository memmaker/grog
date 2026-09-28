using System;
using System.Collections.Generic;
using Grog.Dressings.Features;
using Grog.Dressings.Items;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Rooms;
using Grog.Kernel;
using Grog.Systems.SpecialRooms.Modifiers;

namespace Grog.Systems.SpecialRooms;

[Serializable]
public class SpecialRoomSystem
{
	[Serializable]
	private class SingleRollDamageProvider : INumberConverter
	{
		private readonly int _dieSides;

		private readonly int _levelDivider;

		private readonly int _bonus;

		public SingleRollDamageProvider(int dieSides, int levelDivider, int bonus)
		{
			_dieSides = dieSides;
			_levelDivider = levelDivider;
			_bonus = bonus;
		}

		public int GetConvertedNumber(int number)
		{
			return Game.Instance.Random(_dieSides + number / _levelDivider) + _bonus;
		}
	}

	[Serializable]
	private class ConstantDamageProvider : INumberConverter
	{
		private readonly int _damage;

		public ConstantDamageProvider(int damage)
		{
			_damage = damage;
		}

		public int GetConvertedNumber(int number)
		{
			return _damage;
		}
	}

	private static readonly IRoomModifier[] RoomModifiers = new IRoomModifier[60]
	{
		new DarkRoomModifier(3, 50),
		new TeleportRoomModifier(5, 10),
		new SomeTrapsRoomModifier(2, 15),
		new SomeTrapsRoomModifier(5, 10, 75),
		new SomeTrapsRoomModifier(10, 20, 100),
		new MonsterPitRoomModifier(3, 30, singleMonsterType: true, (int level) => Math.Max(1, Math.Min(level - 2, level / 2)), 5, " is greeted by wild screams!", allowRevengeMonsters: false),
		new MonsterPitRoomModifier(4, 20, singleMonsterType: true, (int level) => Math.Max(level - 3, level / 2), 5, " is greeted by terrifying scream!", allowRevengeMonsters: false),
		new MonsterPitRoomModifier(5, 15, singleMonsterType: false, (int level) => Math.Max(1, Math.Min(level - 2, level / 2)), 15, " is welcomed by a cacophony of war cries!", allowRevengeMonsters: false),
		new MonsterPitRoomModifier(6, 10, singleMonsterType: false, (int level) => Math.Max(level - 3, level / 2), 15, " is welcomed by a cacophony of terrifying war cries!", allowRevengeMonsters: false),
		new MonsterPitRoomModifier(12, 5, singleMonsterType: false, (int level) => level - 1, 75, " is ambushed by a horde of nightmares!", allowRevengeMonsters: false),
		new MonsterPitRoomModifier(9, 50, singleMonsterType: true, (int level) => Math.Max(1, Math.Min(level - 2, level / 2)), 5, " is greeted by wild screams!", allowRevengeMonsters: true),
		new MonsterPitRoomModifier(10, 40, singleMonsterType: true, (int level) => Math.Max(level - 3, level / 2), 5, " is greeted by terrifying scream!", allowRevengeMonsters: true),
		new MonsterPitRoomModifier(11, 35, singleMonsterType: false, (int level) => Math.Max(1, Math.Min(level - 2, level / 2)), 15, " is welcomed by a cacophony of war cries!", allowRevengeMonsters: true),
		new MonsterPitRoomModifier(12, 30, singleMonsterType: false, (int level) => Math.Max(level - 3, level / 2), 15, " is welcomed by a cacophony of terrifying war cries!", allowRevengeMonsters: true),
		new MonsterPitRoomModifier(18, 25, singleMonsterType: false, (int level) => level - 1, 75, " is ambushed by a horde of nightmares!", allowRevengeMonsters: true),
		new MonsterPitRoomModifier(14, 70, singleMonsterType: true, (int level) => Math.Max(1, Math.Min(level - 2, level / 2)), 5, " is greeted by wild screams!", allowRevengeMonsters: true),
		new MonsterPitRoomModifier(15, 60, singleMonsterType: true, (int level) => Math.Max(level - 3, level / 2), 5, " is greeted by terrifying scream!", allowRevengeMonsters: true),
		new MonsterPitRoomModifier(16, 55, singleMonsterType: false, (int level) => Math.Max(1, Math.Min(level - 2, level / 2)), 15, " is welcomed by a cacophony of war cries!", allowRevengeMonsters: true),
		new MonsterPitRoomModifier(17, 50, singleMonsterType: false, (int level) => Math.Max(level - 3, level / 2), 15, " is welcomed by a cacophony of terrifying war cries!", allowRevengeMonsters: true),
		new MonsterPitRoomModifier(23, 45, singleMonsterType: false, (int level) => level - 1, 75, " is ambushed by a horde of nightmares!", allowRevengeMonsters: true),
		new DamageRoomModifier(10, 10, "This room exhibits is chillingly cold!", "is badly frozen by the extreme temperatures of this location!", "resists the biting cold!", "by freezing to death", new ConstantDamageProvider(1), ItemAbility.ResistCold),
		new DamageRoomModifier(8, 10, "This room is a glowing hell!", "is badly burned by the extreme heat in this location!", "resists the scorching heat!", "by being boiled alive", new ConstantDamageProvider(1), ItemAbility.ResistFire),
		new DamageRoomModifier(16, 10, "This room exhibits a deadly cold!", "is terribly frozen by the extreme temperatures of this location!", "resists the biting cold!", "by being turned into an ice cube", new SingleRollDamageProvider(6, 6, 1), ItemAbility.ResistCold),
		new DamageRoomModifier(14, 10, "This room is a fiery hell!", "is horribly burned by the extreme heat in this location!", "resists the scorching heat!", "by being turned into a heap of ash", new SingleRollDamageProvider(4, 6, 1), ItemAbility.ResistFire),
		new FeatureRoomModifier(3, 80, typeof(MagicLiquidFeature), "notices a bubbling sound", () => new MagicLiquidFeature("magic fountain", Game.Instance.Random(2) + 1)),
		new FeatureRoomModifier(7, 85, typeof(MagicLiquidFeature), "notices a rushing sound", () => new MagicLiquidFeature("magic pool", Game.Instance.Random(6) + 3)),
		new TombRoomModifier(6, 20),
		new WaterRoomModifier(4, 10),
		new MudRoomModifier(5, 20),
		new WebbedRoomModifier(5, 10),
		new SpecialPowerRoomModifier(3, 20, "This room seems to be devoid of life.", SpecialRoomPower.NoHealing),
		new SpecialPowerRoomModifier(4, 5, "This room somehow seems to have very weird angles and all sorts of mirror effects.", SpecialRoomPower.InvertedMovement),
		new SpecialPowerRoomModifier(6, 5, "The floor of this room sports occasional blood splatters.", SpecialRoomPower.DoubleDamage),
		new SpecialPowerRoomModifier(8, 3, "This room is all over splattered with blood and entrails.", SpecialRoomPower.TripleDamage),
		new SpecialPowerRoomModifier(8, 3, "The floor of this room sports occasional blood splatters and bone heaps.", SpecialRoomPower.DoubleDamageByMonsters),
		new SpecialPowerRoomModifier(10, 3, "This room is all over splattered with blood, bones and entrails.", SpecialRoomPower.TripleDamageByMonsters),
		new SpecialPowerRoomModifier(9, 30, "The rubble in this room looks as if it might be a great nuisance.", SpecialRoomPower.NeverHitByPlayer),
		new SpecialPowerRoomModifier(6, 30, "The rubble in this room looks as if it might be a nuisance.", SpecialRoomPower.VeryRarelyHitByPlayer),
		new SpecialPowerRoomModifier(5, 30, "This room appears shifty.", SpecialRoomPower.NeverHit),
		new SpecialPowerRoomModifier(3, 30, "This room appears very shifty.", SpecialRoomPower.VeryRarelyHit),
		new SpecialPowerRoomModifier(3, 10, "This room appears bleak and empty.", SpecialRoomPower.DoubleHunger),
		new SpecialPowerRoomModifier(5, 10, "This room appears completely devoid of any living things.", SpecialRoomPower.TripleHunger),
		new SpecialPowerRoomModifier(7, 10, "This room is the epitome of bleakness.", SpecialRoomPower.QuadrulpleHunger),
		new SpecialPowerRoomModifier(12, 15, "The walls of this room are covered with many skulls and bone symbols.", SpecialRoomPower.MightyUndead),
		new SpecialPowerRoomModifier(6, 30, "This room feels like a gateway to many worlds.", SpecialRoomPower.RandomSummonings),
		new SpecialPowerRoomModifier(8, 20, "This room seems to have a terribly weak structure.", SpecialRoomPower.HalvedStrength),
		new SpecialPowerRoomModifier(8, 20, "This room seems extremely hard to navigate.", SpecialRoomPower.HalvedDexterity),
		new SpecialPowerRoomModifier(15, 10, "The floor of this room is covered with shattered equipment.", SpecialRoomPower.ItemRot),
		new SpecialPowerRoomModifier(3, 10, "One wall of this room has a damaged mural of the mighty lord Midas.", SpecialRoomPower.GoldRot),
		new SpecialPowerRoomModifier(5, 8, "The air of this room is extremely moist. Each and every surface appears to be drenched!", SpecialRoomPower.Humid),
		new SpecialPowerRoomModifier(7, 4, "The air of this room is filled with a smell of poisonous rot.", SpecialRoomPower.Rotting),
		new SpecialPowerRoomModifier(12, 40, "This room seems to be made from metal... rusted metal.", SpecialRoomPower.Rusting),
		new SpecialPowerRoomModifier(3, 10, "This room glows in a holy light.", SpecialRoomPower.SatiationByMana),
		new SpecialPowerRoomModifier(3, 50, "The floor of this rooms appears to be very slippery...", SpecialRoomPower.Slippery),
		new SpecialPowerRoomModifier(3, 20, "This room contains lots of broken pottery.", SpecialRoomPower.Fumbling),
		new SpecialPowerRoomModifier(16, 5, "This room is brimming with magic energy!", SpecialRoomPower.ManaCharged),
		new SpecialPowerRoomModifier(12, 5, "This room is humming with dangerous magic energy!", SpecialRoomPower.PowerfulManaCharged),
		new SpecialPowerRoomModifier(3, 20, "This room is seems to be magnetically charged!", SpecialRoomPower.Magnetic),
		new SpecialPowerRoomModifier(5, 10, "This room is crawling with countless bugs!", SpecialRoomPower.BugInfested),
		new SpecialPowerRoomModifier(7, 77, "Many small blades swirl through this room, obviously animated by weird magic!", SpecialRoomPower.SlashieRoom)
	};

	public bool Modify(DungeonLevel dungeonLevel, IRoom room)
	{
		List<IRoomModifier> list = new List<IRoomModifier>();
		int num = 0;
		IRoomModifier[] roomModifiers = RoomModifiers;
		foreach (IRoomModifier roomModifier in roomModifiers)
		{
			if (roomModifier.MinimumLevel >= dungeonLevel.Level)
			{
				list.Add(roomModifier);
				num += roomModifier.Rarity;
			}
		}
		if (num == 0)
		{
			return true;
		}
		int num2 = Game.Instance.Random(num);
		foreach (IRoomModifier item in list)
		{
			num2 -= item.Rarity;
			if (num2 <= 0)
			{
				return item.Modify(dungeonLevel, room);
			}
		}
		return false;
	}

	public void AddInformation(List<string> information)
	{
		information.Add("Special room types: " + RoomModifiers.Length);
	}
}
