using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;
using Grog.Kernel.Interactions;

namespace Grog.Dressings.Features;

[Serializable]
public abstract class WhenFindingGoldInteractionBase : IBeingInteraction
{
	private readonly int _gold;

	private int _extra;

	protected WhenFindingGoldInteractionBase(int gold)
	{
		_gold = gold;
	}

	public virtual bool Interact(DungeonLevel dungeonLevel, Being being)
	{
		if (being.SpecialAbility == SpecialAbility.StealGold && Game.Instance.Grog.CanSee(dungeonLevel, being))
		{
			int gold = GetGold(dungeonLevel);
			dungeonLevel.Message(being, "pilfers " + gold + " gold pieces from the ground!");
			Game.Instance.Grog.ResetAutomaticAction();
			being.Gold += gold;
			dungeonLevel.SetFeatureAt(being.X, being.Y, null);
		}
		return true;
	}

	protected int GetGold(DungeonLevel dungeonLevel)
	{
		if (_gold > 0)
		{
			return _gold + _extra;
		}
		return GetRandomGold(dungeonLevel, _extra);
	}

	protected abstract int GetRandomGold(DungeonLevel dungeonLevel, int extra);

	public void IncreaseInValueBy(int value)
	{
		_extra += value;
	}
}
