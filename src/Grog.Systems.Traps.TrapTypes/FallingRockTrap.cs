using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Systems.Traps.TrapTypes;

[Serializable]
public class FallingRockTrap : TrapBase
{
	public FallingRockTrap()
		: base("tripwire")
	{
	}

	public override void Activate(DungeonLevel dungeonLevel, Being being, int x, int y, bool willfulActivation)
	{
		if (willfulActivation || being.IsLevitating == 0 || being.SpecialAbility != SpecialAbility.Desolid)
		{
			if (being.SpecialAbility == SpecialAbility.Desolid)
			{
				dungeonLevel.Message(being, "A massive falling rock passes through " + being.Name + "!");
				return;
			}
			dungeonLevel.Message(being, "is hit by a massive falling rock!");
			being.SufferDamage(dungeonLevel, null, Game.Instance.Roll(1, 10, dungeonLevel.Level / 2), "by a falling rock");
			dungeonLevel.RemoveTrapAt(x, y);
		}
	}
}
