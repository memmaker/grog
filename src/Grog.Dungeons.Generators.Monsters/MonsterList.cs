using System.Collections.Generic;
using Grog.Dressings.Beings;
using Grog.Kernel;
using Grog.Systems.Revenge;

namespace Grog.Dungeons.Generators.Monsters;

public class MonsterList
{
	private readonly int _dangerLevel;

	private readonly List<MonsterDefinition> _list = new List<MonsterDefinition>();

	private int _total;

	public MonsterList(int dangerLevel)
	{
		_dangerLevel = dangerLevel;
	}

	public void Add(int dungeonLevel, MonsterDefinition definition)
	{
		_list.Add(definition);
		_total += definition.GetRarity(dungeonLevel);
	}

	public Being CreateNewMonster(DungeonLevel dungeonLevel, bool allowRevengeMonsters = true)
	{
		if (_list.Count == 0)
		{
			return null;
		}
		MonsterDefinition definition = FindRandomAvailableMonsterDefinition();
		return CreateMonsterBasedOnDefinition(dungeonLevel, definition, allowRevengeMonsters);
	}

	public Being CreateMonsterBasedOnDefinition(DungeonLevel dungeonLevel, MonsterDefinition definition, bool allowRevengeMonsters)
	{
		if (allowRevengeMonsters)
		{
			RevengeMonster revengeMonster = Game.Instance.RevengeSystem.PullRevengeMonsterForType(dungeonLevel, definition.Template.MonsterType);
			if (revengeMonster != null)
			{
				return revengeMonster;
			}
		}
		Being result = definition.CreateMonster();
		if (!definition.IsAvailable(_dangerLevel))
		{
			_list.Remove(definition);
			_total -= definition.GetRarity(_dangerLevel);
		}
		return result;
	}

	public MonsterDefinition FindRandomAvailableMonsterDefinition()
	{
		if (_total < 1)
		{
			Game.RaiseError("No more monsters available. This should not happen.");
			return null;
		}
		int num = Game.Instance.Random(_total);
		foreach (MonsterDefinition item in _list)
		{
			num -= item.GetRarity(_dangerLevel);
			if (num <= 0)
			{
				return item;
			}
		}
		throw new GrogException("Error during monster generation!");
	}
}
