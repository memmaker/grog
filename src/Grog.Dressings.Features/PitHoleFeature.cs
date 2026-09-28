using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;
using Grog.Kernel.Interactions;
using Grog.Systems.Traps.TrapTypes;

namespace Grog.Dressings.Features;

[Serializable]
public class PitHoleFeature : Feature
{
	[Serializable]
	public class TryToNavigatePitHoleBeingInteraction : IBeingInteraction
	{
		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			if (being.IsLevitating != 0)
			{
				dungeonLevel.Message(being, " hovers across a gloomy hole in the ground.");
			}
			else if (Game.Instance.Random(being.Dexterity + 3) > being.Dexterity)
			{
				dungeonLevel.Message(being, " slips and painfully crashes into the hard ground of a gloomy pit!");
				PitTrap.SufferPitFallDamage(dungeonLevel, being);
				if (being is Player player)
				{
					player.Feelings |= 8;
				}
			}
			else
			{
				dungeonLevel.Message(being, " carefully slips past a gloomy pit.");
			}
			return true;
		}
	}

	[Serializable]
	public class ClimbOutOfPitHoleBeingInteraction : ClimbOutOfPitHoleBaseInteraction, IBeingInteraction
	{
		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			return ClimbOutOfThePitHole(dungeonLevel, being);
		}
	}

	[Serializable]
	public class ClimbOutOfPitHoleInteraction : ClimbOutOfPitHoleBaseInteraction, IInteractionInteraction
	{
		public void Interact(DungeonLevel dungeonLevel, Player grog, int x, int y)
		{
			ClimbOutOfThePitHole(dungeonLevel, grog);
		}
	}

	[Serializable]
	public abstract class ClimbOutOfPitHoleBaseInteraction
	{
		protected bool ClimbOutOfThePitHole(DungeonLevel dungeonLevel, Being being)
		{
			if (being.IsLevitating != 0)
			{
				dungeonLevel.Message(being, "hovers past the gloomy pit hole.");
			}
			if (Game.Instance.Random(being.Strength + 10) > being.Strength)
			{
				dungeonLevel.Message(being, " is too weak to pull out of the pit!");
				if (being is Player player)
				{
					player.Feelings |= 8;
				}
				return false;
			}
			if (Game.Instance.Random(being.Dexterity + 5) > being.Dexterity)
			{
				dungeonLevel.Message(being, " slips once more and painfully crashed back into the pit!");
				being.SufferDamage(dungeonLevel, null, Game.Instance.Roll(1, 4), "by slipping into a gloomy pit");
				if (being is Player player2)
				{
					player2.Feelings |= 8;
				}
				return false;
			}
			dungeonLevel.Message(being, " climbs out of the pit!");
			dungeonLevel.SetFeatureAt(being.X, being.Y, new KnownTrapFeature());
			return true;
		}
	}

	public PitHoleFeature()
		: base('*', new ClimbOutOfPitHoleInteraction(), new TryToNavigatePitHoleBeingInteraction(), new ClimbOutOfPitHoleBeingInteraction())
	{
	}
}
