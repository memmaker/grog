using System;
using Grog.Dressings.Beings;

namespace Grog.Dungeons.Generators.Monsters;

[Serializable]
public class MonsterDefinition
{
	private readonly int _rarity;

	private readonly Being _template;

	private readonly int _maximumOccurence;

	private int _generatedMonstersOfThisType;

	public Being Template => _template;

	public MonsterDefinition(int rarity, Being template, int maximumOccurence = int.MaxValue)
	{
		_rarity = rarity;
		_template = template;
		_maximumOccurence = maximumOccurence;
		_generatedMonstersOfThisType = 0;
	}

	public bool IsAvailable(int dungeonLevel)
	{
		if (_template.Level <= dungeonLevel)
		{
			return _generatedMonstersOfThisType < _maximumOccurence;
		}
		return false;
	}

	public Being CreateMonster()
	{
		_generatedMonstersOfThisType++;
		return new Being(_template);
	}

	public int GetRarity(int dungeonLevel)
	{
		if (dungeonLevel == _template.Level)
		{
			return _rarity;
		}
		int num = Math.Abs(dungeonLevel - _template.Level);
		if (num > 5)
		{
			return Math.Max(1, _rarity / (10 + (num - 5) * 3));
		}
		return Math.Max(1, _rarity / (2 + num));
	}
}
