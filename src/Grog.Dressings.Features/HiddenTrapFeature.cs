using System;
using Grog.Dressings.Beings;
using Grog.Dressings.Items;
using Grog.Dressings.MapElements;
using Grog.Dungeons;
using Grog.Kernel;
using Grog.Kernel.Interactions;
using Grog.Kernel.Options;
using Grog.Kernel.Representations;
using Grog.Systems.Traps;

namespace Grog.Dressings.Features;

[Serializable]
public class HiddenTrapFeature : Feature
{
	[Serializable]
	public class NearbyThreatMapElementInteraction : IMapElementInteraction
	{
		public void Interact(DungeonLevel dungeonLevel, Player grog, int x, int y, IMapElement element, InteractionContext context)
		{
			dungeonLevel.Message(" feels threatened.", Game.Instance.Options.SixthSense == SixthSense.Pausing);
			grog.Feelings |= 1;
		}
	}

	[Serializable]
	public class AdjacentThreatMapElementInteraction : IMapElementInteraction
	{
		public void Interact(DungeonLevel dungeonLevel, Player grog, int x, int y, IMapElement element, InteractionContext context)
		{
			dungeonLevel.Message(" feels greatly endangered!", Game.Instance.Options.SixthSense == SixthSense.Pausing);
			grog.Feelings |= 2;
		}
	}

	[Serializable]
	public class DiscoverAndActivateTrapBeingInteraction : IBeingInteraction
	{
		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			if (being is Player player)
			{
				ITrap orCreateTrapAt = dungeonLevel.GetOrCreateTrapAt(being.X, being.Y);
				dungeonLevel.Message("Trap!", more: true);
				player.ResetAutomaticAction();
				bool flag = false;
				if ((Game.Instance.Random(player.Dexterity + 100) > player.Dexterity && !player.Inventory.HasEquippedItemWithAbility(ItemAbility.TrapEvasion)) || player.IsBlind != 0 || player.IsConfused != 0)
				{
					orCreateTrapAt.Activate(dungeonLevel, player, player.X, player.Y, willfulActivation: false);
				}
				else
				{
					dungeonLevel.Message(" evades the " + orCreateTrapAt.Name + (player.Inventory.HasEquippedItemWithAbility(ItemAbility.TrapEvasion) ? " due to amazing reflexes!" : " with sheer luck!"));
					flag = true;
				}
				if (dungeonLevel.IsTrapAt(player.X, player.Y) && (orCreateTrapAt.IsAutoIdentifying | flag))
				{
					dungeonLevel.SetFeatureAt(player.X, player.Y, new KnownTrapFeature());
				}
			}
			else
			{
				dungeonLevel.Message(being, " suddenly moves very carefully!");
				if (Game.Instance.Grog.CanSee(dungeonLevel, being.X, being.Y))
				{
					Game.Instance.Grog.ResetAutomaticAction();
				}
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

	public HiddenTrapFeature()
		: base(new HiddenTrapCharacterRepresentation(), null, new DiscoverAndActivateTrapBeingInteraction())
	{
		AddAdjacentInteraction(new AdjacentThreatMapElementInteraction());
		AddDiagonalInteraction(new NearbyThreatMapElementInteraction());
	}

	public static bool IsSuitablePosition(DungeonLevel dungeonLevel, Position position)
	{
		if (!dungeonLevel.IsBeingAt(position) && !dungeonLevel.IsTrapAt(position.X, position.Y) && !dungeonLevel.HasFeatureAt(position.X, position.Y))
		{
			if (dungeonLevel.GetTileAt(position) != Tile.Tunnel)
			{
				return dungeonLevel.GetTileAt(position) == Tile.Floor;
			}
			return true;
		}
		return false;
	}
}
