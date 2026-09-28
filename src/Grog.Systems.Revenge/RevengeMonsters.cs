using System;
using System.Collections.Generic;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Monsters;
using Grog.Kernel;

namespace Grog.Systems.Revenge;

[Serializable]
public class RevengeMonsters
{
	private Dictionary<string, RevengeMonster> _revengeMonsters = new Dictionary<string, RevengeMonster>();

	private Dictionary<MonsterPool.MonsterType, List<RevengeMonster>> _revengeMonstersByType = new Dictionary<MonsterPool.MonsterType, List<RevengeMonster>>();

	public int Count => _revengeMonsters.Count;

	public void Add(RevengeMonster revengeMonster)
	{
		_revengeMonsters.Add(revengeMonster.ChristenedName, revengeMonster);
		if (_revengeMonstersByType.ContainsKey(revengeMonster.MonsterType))
		{
			_revengeMonstersByType[revengeMonster.MonsterType].Add(revengeMonster);
			return;
		}
		_revengeMonstersByType[revengeMonster.MonsterType] = new List<RevengeMonster> { revengeMonster };
	}

	public HashSet<string> GetExistingRevengeMonsterNames()
	{
		return new HashSet<string>(_revengeMonsters.Keys);
	}

	public RevengeMonster PullRevengeMonsterForMonsterType(DungeonLevel dungeonLevel, MonsterPool.MonsterType monsterType)
	{
		if (!_revengeMonstersByType.ContainsKey(monsterType))
		{
			return null;
		}
		List<RevengeMonster> list = new List<RevengeMonster>();
		foreach (RevengeMonster item in _revengeMonstersByType[monsterType])
		{
			if (item.Level >= dungeonLevel.Level && item.Level + item.RevengeLevel <= dungeonLevel.Level + 3)
			{
				list.Add(item);
			}
		}
		if (list.Count == 0)
		{
			return null;
		}
		if (!Game.Instance.Probability(list.Count))
		{
			return null;
		}
		RevengeMonster revengeMonster = list[Game.Instance.Random(list.Count)];
		if (_revengeMonstersByType[monsterType].Count == 1)
		{
			_revengeMonstersByType.Remove(monsterType);
		}
		else
		{
			_revengeMonstersByType[monsterType].Remove(revengeMonster);
		}
		return revengeMonster;
	}
}
