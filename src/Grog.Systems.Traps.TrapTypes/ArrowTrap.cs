using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Systems.Traps.TrapTypes;

[Serializable]
public class ArrowTrap : TrapBase
{
	public ArrowTrap()
		: base("arrow trap")
	{
	}

	public override void Activate(DungeonLevel dungeonLevel, Being being, int x, int y, bool willfulActivation)
	{
		if (willfulActivation || being.IsLevitating == 0)
		{
			if (Game.Instance.Probability(70))
			{
				dungeonLevel.Message(being, " is hit by an arrow flitting from a slit in the wall!");
				being.SufferDamage(dungeonLevel, null, Game.Instance.Roll(1, 6, dungeonLevel.Level / 3), "by an arrow trap");
			}
			else
			{
				dungeonLevel.Message(being, " is almost is hit by an arrow flitting from a slit in the wall!");
			}
			if (Game.Instance.Probability(30))
			{
				dungeonLevel.RemoveTrapAt(x, y);
			}
		}
	}
}
