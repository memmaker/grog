using System;
using Grog.Dressings.MapElements;
using Grog.Dungeons;
using Grog.Kernel.Interactions;

namespace Grog.Dressings;

[Serializable]
public class Thing : MapElementBase
{
	private readonly char _character;

	public IThingAction Act;

	public IMoveIntoInteraction MoveInto;

	public int X { get; set; }

	public int Y { get; set; }

	public bool Moved { get; set; }

	public virtual bool IsActive { get; private set; }

	protected Thing(char character, IBeingInteraction whenSpendingTurnOnMapElement)
		: base(whenSpendingTurnOnMapElement)
	{
		_character = character;
	}

	public virtual char Character(DungeonLevel dungeonLevel, int x, int y)
	{
		return _character;
	}

	public virtual void HandleEffects(DungeonLevel dungeonLevel)
	{
	}
}
