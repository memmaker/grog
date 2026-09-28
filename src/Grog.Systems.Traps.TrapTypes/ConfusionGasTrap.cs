using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Systems.Traps.TrapTypes;

[Serializable]
public class ConfusionGasTrap : TrapBase
{
	public ConfusionGasTrap()
		: base("confusion gas trap")
	{
	}

	public override void Activate(DungeonLevel dungeonLevel, Being being, int x, int y, bool willfulActivation)
	{
		if (willfulActivation || being.IsLevitating == 0)
		{
			dungeonLevel.Message(being, " is engulfed by purple vapors that confuse!");
			being.IsConfused += Game.Instance.Roll(2, 20);
			if (Game.Instance.Probability(15))
			{
				dungeonLevel.RemoveTrapAt(x, y);
			}
		}
	}
}
