using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Systems.Traps.TrapTypes;

[Serializable]
public class SqueakyBoardTrap : TrapBase
{
	public SqueakyBoardTrap()
		: base("squeaky board")
	{
	}

	public override void Activate(DungeonLevel dungeonLevel, Being being, int x, int y, bool willfulActivation)
	{
		if (!willfulActivation && being.IsLevitating != 0)
		{
			return;
		}
		if (Game.Instance.Probability(70))
		{
			dungeonLevel.Message(being, " steps on a squeaky board!");
			if (being is Player grog)
			{
				dungeonLevel.AggravateMonsters(grog);
			}
		}
		else if (being is Player)
		{
			dungeonLevel.Message(" narrowly avoids a squeaky board.");
		}
	}
}
