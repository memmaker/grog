namespace Grog.Systems.Revenge;

public class RevengeMonsterProgressionTable
{
	private readonly RevengeMonsterNameGenerator _generator;

	private readonly RevengeLevelBonus _defaultBonus;

	private readonly RevengeLevelBonus[] _levelBonus;

	public int MaximumDynamicRevengeProgressionLevel => _levelBonus?.Length ?? 0;

	public RevengeMonsterProgressionTable(RevengeMonsterNameGenerator generator, RevengeLevelBonus defaultBonus, params RevengeLevelBonus[] levelBonus)
	{
		_generator = generator;
		_defaultBonus = defaultBonus;
		_levelBonus = levelBonus;
	}

	public RevengeLevelBonus GetRevengeLevelBonusFor(RevengeMonster revengeMonster)
	{
		if (_levelBonus == null || _levelBonus.Length < revengeMonster.RevengeLevel)
		{
			return _defaultBonus;
		}
		return _levelBonus[revengeMonster.RevengeLevel - 1];
	}

	public string GetRandomRevengeMonsterName()
	{
		return _generator?.GenerateRandomName();
	}
}
