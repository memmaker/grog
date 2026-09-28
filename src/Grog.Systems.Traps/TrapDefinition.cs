using System;

namespace Grog.Systems.Traps;

public class TrapDefinition
{
	private readonly Func<ITrap> _createTrap;

	public int MinimumLevel { get; }

	public int MaximumLevel { get; }

	public int Rarity { get; }

	public TrapDefinition(int minimumLevel, int maximumLevel, int rarity, Func<ITrap> createTrap)
	{
		_createTrap = createTrap;
		MinimumLevel = minimumLevel;
		MaximumLevel = maximumLevel;
		Rarity = rarity;
	}

	public ITrap Instantiate()
	{
		return _createTrap();
	}
}
