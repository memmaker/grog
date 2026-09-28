using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Dressings.Items.Implementations;

[Serializable]
public class RingImplementations
{
	public bool UseTeleportation(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		dungeonLevel.Teleport(user);
		if (user is Player player)
		{
			player.Moves++;
			player.Satiation = player.Satiation - 100 - 50;
		}
		return Game.Instance.Probability(14);
	}
}
