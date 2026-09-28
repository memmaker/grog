using Grog.Dressings.Beings;
using Grog.Dungeons;

namespace Grog.Kernel;

public class AutomaticWaitUntilHealedAction : IAutomaticAction
{
	private readonly Player _grog;

	private readonly DungeonLevel _dungeonLevel;

	private readonly int _startTurn;

	public AutomaticWaitUntilHealedAction(Player grog, DungeonLevel dungeonLevel)
	{
		_grog = grog;
		_dungeonLevel = dungeonLevel;
		_startTurn = _grog.Moves;
	}

	public bool Execute()
	{
		if (_grog.HitPoints == _grog.MaxHitPoints)
		{
			int num = _grog.Moves - _startTurn;
			_dungeonLevel.Message(" fully recovers after " + num + " turn" + ((num == 1) ? "" : "s") + ".");
			return false;
		}
		_dungeonLevel.InteractWithSurroundingMapElements(_dungeonLevel, _grog);
		_grog.Moves++;
		return true;
	}
}
