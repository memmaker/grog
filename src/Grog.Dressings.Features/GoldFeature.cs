using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Dressings.Features;

[Serializable]
public class GoldFeature : Feature
{
	[Serializable]
	public class WhenFindingGoldBeingInteraction : WhenFindingGoldInteractionBase
	{
		public WhenFindingGoldBeingInteraction(int gold)
			: base(gold)
		{
		}

		public override bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			if (being is Player player)
			{
				int gold = GetGold(dungeonLevel);
				dungeonLevel.Message(" picks up " + gold + " gold pieces!");
				player.Gold += gold;
				dungeonLevel.SetFeatureAt(player.X, player.Y, null);
				return true;
			}
			return base.Interact(dungeonLevel, being);
		}

		protected override int GetRandomGold(DungeonLevel dungeonLevel, int extra)
		{
			return new Roll(dungeonLevel.Level, 10, dungeonLevel.Level * (Game.Instance.Random(5) + 2)).GetDieResult() + extra;
		}
	}

	public GoldFeature(int gold = -1)
		: base('$', null, new WhenFindingGoldBeingInteraction(gold))
	{
	}

	public void IncreaseInValueBy(int value)
	{
		(WhenEntering as WhenFindingGoldBeingInteraction)?.IncreaseInValueBy(value);
	}
}
