using System;
using Grog.Dressings.Beings;
using Grog.Dressings.Items;
using Grog.Dungeons;
using Grog.Kernel;
using Grog.Kernel.Interactions;
using Grog.Kernel.Representations;
using Grog.Systems.Traps;

namespace Grog.Dressings.Features;

[Serializable]
public class KnownTrapFeature : Feature
{
	[Serializable]
	public class ActivateKnownTrapInteraction : IInteractionInteraction
	{
		public void Interact(DungeonLevel dungeonLevel, Player grog, int x, int y)
		{
			ITrap orCreateTrapAt = dungeonLevel.GetOrCreateTrapAt(grog.X, grog.Y);
			if (dungeonLevel.Sure("active the " + orCreateTrapAt.Name))
			{
				orCreateTrapAt.Activate(dungeonLevel, grog, grog.X, grog.Y, willfulActivation: true);
				grog.Moves++;
			}
		}
	}

	[Serializable]
	public class ActivateKnownTrapWhenEnteredBeingInteraction : IBeingInteraction
	{
		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			ITrap orCreateTrapAt = dungeonLevel.GetOrCreateTrapAt(being.X, being.Y);
			if (being is Player player)
			{
				if ((Game.Instance.Random(player.Dexterity + 5) <= player.Dexterity || player.Inventory.HasEquippedItemWithAbility(ItemAbility.TrapEvasion)) && player.IsBlind == 0 && player.IsConfused == 0)
				{
					dungeonLevel.Message(" nimbly evades the " + orCreateTrapAt.Name + ".");
				}
				else
				{
					orCreateTrapAt.Activate(dungeonLevel, player, player.X, player.Y, willfulActivation: false);
				}
			}
			else
			{
				dungeonLevel.Message(being, " carefully evades the " + orCreateTrapAt.Name + ".");
			}
			return true;
		}
	}

	[Serializable]
	public class HiddenTrapCharacterRepresentation : ICharacterRepresentation
	{
		public char GetCharacterRepresentation(DungeonLevel dungeonLevel, int x, int y)
		{
			if (dungeonLevel.GetRoomAt(x, y) == null)
			{
				return Tile.Tunnel.Character(dungeonLevel, x, y);
			}
			return Tile.Floor.Character(dungeonLevel, x, y);
		}
	}

	public KnownTrapFeature()
		: base('^', new ActivateKnownTrapInteraction(), new ActivateKnownTrapWhenEnteredBeingInteraction())
	{
	}
}
