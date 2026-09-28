using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Systems.Traps.TrapTypes;

[Serializable]
public class ParalyzationGasTrap : TrapBase
{
	public ParalyzationGasTrap()
		: base("paralyzation gas trap")
	{
	}

	public override void Activate(DungeonLevel dungeonLevel, Being being, int x, int y, bool willfulActivation)
	{
		if (willfulActivation || being.IsLevitating == 0)
		{
			dungeonLevel.Message(being, " is frozen to the spot after being engulfed by gray vapors!");
			being.IsParalyzed += Game.Instance.Roll(2, 3, dungeonLevel.Level / 2);
			if (Game.Instance.Probability(25))
			{
				dungeonLevel.RemoveTrapAt(x, y);
			}
		}
	}
}
