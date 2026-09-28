using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Systems.Traps.TrapTypes;

[Serializable]
public class SpearTrap : TrapBase
{
	public SpearTrap()
		: base("spear trap")
	{
	}

	public override void Activate(DungeonLevel dungeonLevel, Being being, int x, int y, bool willfulActivation)
	{
		if (willfulActivation || being.IsLevitating == 0)
		{
			if (Game.Instance.Probability(70))
			{
				dungeonLevel.Message(being, " is hit by a spear fired from a hole in the wall!");
				being.SufferDamage(dungeonLevel, null, Game.Instance.Roll(1, 8, dungeonLevel.Level / 2), "by a spear trap");
			}
			else
			{
				dungeonLevel.Message(being, " almost is hit by a spear fired from a hole in the wall!");
			}
			if (Game.Instance.Probability(50))
			{
				dungeonLevel.RemoveTrapAt(x, y);
			}
		}
	}
}
