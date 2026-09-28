using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;

namespace Grog.Systems.Traps.TrapTypes;

[Serializable]
public class TeleportTrap : TrapBase
{
	public TeleportTrap()
		: base("teleportation circle")
	{
	}

	public override void Activate(DungeonLevel dungeonLevel, Being being, int x, int y, bool willfulActivation)
	{
		dungeonLevel.Message(being, " suddenly is whisked away by magic!");
		dungeonLevel.Teleport(being);
	}
}
