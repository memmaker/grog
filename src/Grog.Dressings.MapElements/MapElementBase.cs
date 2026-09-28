using System;
using System.Collections.Generic;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel.Interactions;

namespace Grog.Dressings.MapElements;

[Serializable]
public class MapElementBase : IMapElement
{
	private List<IMapElementInteraction> _adjacentInteractions;

	private List<IMapElementInteraction> _diagonalInteractions;

	public IBeingInteraction WhenSpendingTurnOnMapElement { get; }

	protected MapElementBase(IBeingInteraction whenSpendingTurnOnMapElement)
	{
		WhenSpendingTurnOnMapElement = whenSpendingTurnOnMapElement;
	}

	public void AddAdjacentInteraction(IMapElementInteraction interaction)
	{
		if (_adjacentInteractions == null)
		{
			_adjacentInteractions = new List<IMapElementInteraction>();
		}
		_adjacentInteractions.Add(interaction);
	}

	public void InteractWithAdjacentElement(DungeonLevel dungeonLevel, Player grog, int x, int y, IMapElement element, InteractionContext context)
	{
		if (_adjacentInteractions == null)
		{
			return;
		}
		foreach (IMapElementInteraction adjacentInteraction in _adjacentInteractions)
		{
			adjacentInteraction.Interact(dungeonLevel, grog, x, y, element, context);
		}
	}

	public void AddDiagonalInteraction(IMapElementInteraction interaction)
	{
		if (_diagonalInteractions == null)
		{
			_diagonalInteractions = new List<IMapElementInteraction>();
		}
		_diagonalInteractions.Add(interaction);
	}

	public void InteractWithDiagonalElement(DungeonLevel dungeonLevel, Player grog, int x, int y, IMapElement element, InteractionContext context)
	{
		if (_diagonalInteractions == null)
		{
			return;
		}
		foreach (IMapElementInteraction diagonalInteraction in _diagonalInteractions)
		{
			diagonalInteraction.Interact(dungeonLevel, grog, x, y, element, context);
		}
	}
}
