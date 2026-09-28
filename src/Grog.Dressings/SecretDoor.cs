using System;
using Grog.Dressings.Beings;
using Grog.Dressings.MapElements;
using Grog.Dungeons;
using Grog.Kernel;
using Grog.Kernel.Interactions;
using Grog.Kernel.Options;
using Grog.Kernel.Representations;

namespace Grog.Dressings;

[Serializable]
public class SecretDoor : Tile
{
	[Serializable]
	private class DiscoverSecretDoorDiagonalMapElementInteraction : IMapElementInteraction
	{
		public void Interact(DungeonLevel dungeonLevel, Player grog, int x, int y, IMapElement element, InteractionContext context)
		{
			context.AddInteraction(() =>
			{
				if (grog.IsBlind == 0)
				{
					dungeonLevel.Message(" discovers a secret door!");
					dungeonLevel.SetTile(x, y, Tile.Door);
					grog.ResetAutomaticAction();
				}
			});
		}
	}

	[Serializable]
	private class SenseSecretDoorDiagonalMapElementInteraction : IMapElementInteraction
	{
		public void Interact(DungeonLevel dungeonLevel, Player grog, int x, int y, IMapElement element, InteractionContext context)
		{
			context.AddInteraction(() =>
			{
				dungeonLevel.Message("'s back tingles...", Game.Instance.Options.SixthSense == SixthSense.Pausing);
				grog.ResetAutomaticAction();
				grog.Feelings |= 4;
			});
		}
	}

	public SecretDoor(int index, ICharacterRepresentation getCharacter, bool isSolid, IMoveIntoInteraction moveInto)
		: base(index, getCharacter, isSolid, null, null, null, null, null, moveInto)
	{
		AddDiagonalInteraction(new SenseSecretDoorDiagonalMapElementInteraction());
		AddAdjacentInteraction(new DiscoverSecretDoorDiagonalMapElementInteraction());
	}
}
