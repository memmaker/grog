using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Systems.Traps.TrapTypes;

[Serializable]
public class AcidTrap : TrapBase
{
	public AcidTrap()
		: base("acid trap")
	{
	}

	public override void Activate(DungeonLevel dungeonLevel, Being being, int x, int y, bool willfulActivation)
	{
		if (willfulActivation || being.IsLevitating == 0)
		{
			dungeonLevel.Message(being, " is badly burned by a gush of acid spouting from the ceiling!");
			being.SufferDamage(dungeonLevel, null, Game.Instance.Roll(2 + dungeonLevel.Level / 5, 4, dungeonLevel.Level / 4), "by an acid trap");
			if (Game.Instance.Probability(70))
			{
				dungeonLevel.RemoveTrapAt(x, y);
			}
		}
	}
}
