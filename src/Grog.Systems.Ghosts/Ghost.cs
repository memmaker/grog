using System;
using System.Collections.Generic;
using Grog.Dressings.Beings;
using Grog.Dressings.Items;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Monsters;
using Grog.Kernel;
using Grog.Systems.Revenge;

namespace Grog.Systems.Ghosts;

[Serializable]
public class Ghost : Being
{
	private List<Item> _treasures = new List<Item>();

	public override string Name => "the " + Type + " of " + ChristenedName;

	public Ghost(Player player)
		: base(MonsterPool.MonsterType.Ghost, Race.Undead, player.Name + " the " + player.Gender + " " + player.Type, "ghost", RevengeMonsterProgression.GreaterUndead, '@', player.Level, player.ArmorClass + 4, 0, 0, player.MaxHitPoints + player.MaxHitPoints / 2, player.GetMeleeCapabilities().DamageRolls, SpecialAbility.DrainConstitution, player.Strength + player.Constitution + player.Level)
	{
		IsInverted = true;
		foreach (Item item2 in player.Inventory.GetInventory())
		{
			if ((item2.IsEquipped && Game.Instance.Probability(50)) || (!item2.IsEquipped && Game.Instance.Probability(10)))
			{
				Item item = new Item(item2);
				if (item.IsEquippable)
				{
					item.IsCursed = true;
				}
				_treasures.Add(item);
			}
		}
		HitPoints = (MaxHitPoints += _treasures.Count * (10 + player.Level));
	}

	protected override void WhenKilled(DungeonLevel dungeonLevel, Being attacker)
	{
		base.WhenKilled(dungeonLevel, attacker);
		foreach (Item treasure in _treasures)
		{
			dungeonLevel.Message(this, Name + " drops " + treasure.a() + ".");
			dungeonLevel.PhysicallyDropItem(dungeonLevel, Game.Instance.Grog, X, Y, treasure);
		}
		Game.Instance.MonsterPool.ForgetMonsterKnowledge(this);
	}

	public void Activate()
	{
		UID = Game.Instance.NextUID;
	}
}
