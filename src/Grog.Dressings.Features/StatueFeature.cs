using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;
using Grog.Kernel.Interactions;

namespace Grog.Dressings.Features;

[Serializable]
public class StatueFeature : Feature
{
	[Serializable]
	public class EncounterStatueBeingInteraction : IBeingInteraction
	{
		private readonly int _demon;

		public EncounterStatueBeingInteraction(int demon)
		{
			_demon = demon;
		}

		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			if (being is Player)
			{
				dungeonLevel.Message(ShortTexts[_demon]);
			}
			return true;
		}
	}

	[Serializable]
	public class InteractWithStatueInteractiveInteraction : IInteractionInteraction
	{
		private readonly int _demon;

		public InteractWithStatueInteractiveInteraction(int demon)
		{
			_demon = demon;
		}

		public void Interact(DungeonLevel dungeonLevel, Player grog, int x, int y)
		{
			dungeonLevel.InitiateComplexInteraction(LongTexts[_demon]);
		}
	}

	private static readonly string[] ShortTexts = new string[2] { "encounter the statue of a bulbous goat demon.", "encounter the statue of a hideous frog monster." };

	private static readonly string[] LongTexts = new string[2] { "This grotesque statue depicts a huge goat-like humanoid with a bulbous belly, muscles arms ending in massiv five-fingered hands topped by sharp talons. The figure is sitting on the ground, cross-legged, and holds a massive scepter topped with a skull across its legs.\n\nIt extrudes a foreboding aura.", "This statue chiseled from black obsidian shows a massively bulbous toad with weirdly faceted eyes. It gazes at you with an obscene almost leering stare. Your mind reels when looking at the eyes for too long." };

	public StatueFeature(DungeonLevel dungeonLevel)
		: this(dungeonLevel, Game.Instance.Random(ShortTexts.Length))
	{
	}

	public StatueFeature(DungeonLevel dungeonLevel, int demon)
		: base('&', new InteractWithStatueInteractiveInteraction(demon), new EncounterStatueBeingInteraction(demon))
	{
	}
}
