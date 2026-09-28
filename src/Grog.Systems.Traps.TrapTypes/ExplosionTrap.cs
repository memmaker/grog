using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Systems.Traps.TrapTypes;

[Serializable]
public class ExplosionTrap : TrapBase
{
	public ExplosionTrap()
		: base("fire runes")
	{
	}

	public override void Activate(DungeonLevel dungeonLevel, Being being, int x, int y, bool willfulActivation)
	{
		dungeonLevel.Message(being, "Runes etched into the ground explode in a fiery inferno!", more: true);
		for (int i = x - 1; i <= x + 1; i++)
		{
			for (int j = y - 1; j <= y + 1; j++)
			{
				if (dungeonLevel.IsValid(i, j) && dungeonLevel.GetThingAt(i, j) is Being being2)
				{
					dungeonLevel.Message(being2, " is badly burned!");
					being2.SufferDamage(dungeonLevel, null, Game.Instance.Roll(1 + dungeonLevel.Level / 5, 6), "by exploding runes");
				}
			}
		}
		dungeonLevel.RemoveTrapAt(x, y);
	}
}
