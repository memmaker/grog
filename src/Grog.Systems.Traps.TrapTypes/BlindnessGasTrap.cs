using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Systems.Traps.TrapTypes;

[Serializable]
public class BlindnessGasTrap : TrapBase
{
	public BlindnessGasTrap()
		: base("blindness gas trap")
	{
	}

	public override void Activate(DungeonLevel dungeonLevel, Being being, int x, int y, bool willfulActivation)
	{
		if (willfulActivation || being.IsLevitating == 0)
		{
			if (being.IsBlind == 0)
			{
				dungeonLevel.Message(being, " is blinded by green vapors blasting from tiny slits in the walls!");
			}
			being.IsBlind += Game.Instance.Roll(4, 4, 2);
			if (Game.Instance.Probability(15))
			{
				dungeonLevel.RemoveTrapAt(x, y);
			}
		}
	}
}
