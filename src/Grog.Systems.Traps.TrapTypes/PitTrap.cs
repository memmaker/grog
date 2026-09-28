using System;
using Grog.Dressings.Beings;
using Grog.Dressings.Features;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Systems.Traps.TrapTypes;

[Serializable]
public class PitTrap : TrapBase
{
	public PitTrap()
		: base("pit", isAutoIdentifying: false)
	{
	}

	public override void Activate(DungeonLevel dungeonLevel, Being being, int x, int y, bool willfulActivation)
	{
		if (willfulActivation || (being.IsLevitating == 0 && Game.Instance.Random(being.Dexterity + 15) > being.Dexterity))
		{
			if (being is Player player)
			{
				if (player.IsLevitating != 0)
				{
					dungeonLevel.Message(" can't enter the pit while hovering above it!");
					return;
				}
				dungeonLevel.Say("*Aaargh!*");
				dungeonLevel.Message(" falls into a gloomy pit and painfully smashes into the rocky ground!");
			}
			else
			{
				dungeonLevel.Message(being, " falls into a gloomy pit!");
			}
			SufferPitFallDamage(dungeonLevel, being);
			dungeonLevel.SetFeatureAt(x, y, new PitHoleFeature());
		}
		else if (being.IsLevitating == 0)
		{
			dungeonLevel.Message(being, " carefully moves past the edge of a gloomy pit that suddenly opens in the ground!");
			dungeonLevel.SetFeatureAt(x, y, new PitHoleFeature());
		}
	}

	public static void SufferPitFallDamage(DungeonLevel dungeonLevel, Being being)
	{
		being.SufferDamage(dungeonLevel, null, Game.Instance.Roll(1 + dungeonLevel.Level / 3, 6), "by falling into a gloomy pit");
	}
}
