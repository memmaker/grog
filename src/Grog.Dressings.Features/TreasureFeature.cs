using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Dressings.Features;

[Serializable]
public class TreasureFeature : Feature
{
	[Serializable]
	public class WhenFindingTreasureBeingInteraction : WhenFindingGoldInteractionBase
	{
		public WhenFindingTreasureBeingInteraction(int gold)
			: base(gold)
		{
		}

		public override bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			if (being is Player player)
			{
				int gold = GetGold(dungeonLevel);
				dungeonLevel.Message(" has found a pile of gold containing " + gold + " gold pieces!");
				player.Gold += gold;
				dungeonLevel.SetFeatureAt(player.X, player.Y, null);
				return true;
			}
			return base.Interact(dungeonLevel, being);
		}

		protected override int GetRandomGold(DungeonLevel dungeonLevel, int extra)
		{
			return new Roll(dungeonLevel.Level, 100, dungeonLevel.Level * 20).GetDieResult();
		}
	}

	public TreasureFeature()
		: base('$', null, new WhenFindingTreasureBeingInteraction(-1))
	{
	}
}
