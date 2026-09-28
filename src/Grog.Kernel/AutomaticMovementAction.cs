using Grog.Dressings;
using Grog.Dressings.Beings;
using Grog.Dungeons;

namespace Grog.Kernel;

public class AutomaticMovementAction : IAutomaticAction
{
	private readonly Player _grog;

	private readonly DungeonLevel _dungeonLevel;

	private Direction _direction;

	public AutomaticMovementAction(Player grog, DungeonLevel dungeonLevel, Direction direction)
	{
		_grog = grog;
		_dungeonLevel = dungeonLevel;
		_direction = direction;
	}

	public bool Execute()
	{
		int moves = _grog.Moves;
		if (!_dungeonLevel.CanMoveTo(_grog, _direction))
		{
			switch (_direction)
			{
			case Direction.East:
			case Direction.West:
				if (_dungeonLevel.CanMoveTo(_grog, Direction.MinDirection) && _dungeonLevel.CanMoveTo(_grog, Direction.South))
				{
					return false;
				}
				if (_dungeonLevel.CanMoveTo(_grog, Direction.MinDirection))
				{
					_direction = Direction.MinDirection;
					break;
				}
				if (_dungeonLevel.CanMoveTo(_grog, Direction.South))
				{
					_direction = Direction.South;
					break;
				}
				return false;
			case Direction.MinDirection:
			case Direction.South:
				if (_dungeonLevel.CanMoveTo(_grog, Direction.West) && _dungeonLevel.CanMoveTo(_grog, Direction.East))
				{
					return false;
				}
				if (_dungeonLevel.CanMoveTo(_grog, Direction.West))
				{
					_direction = Direction.West;
					break;
				}
				if (_dungeonLevel.CanMoveTo(_grog, Direction.East))
				{
					_direction = Direction.East;
					break;
				}
				return false;
			}
		}
		switch (_direction)
		{
		case Direction.East:
			_dungeonLevel.MovePlayerEast();
			break;
		case Direction.West:
			_dungeonLevel.MovePlayerWest();
			break;
		case Direction.MinDirection:
			_dungeonLevel.MovePlayerNorth();
			break;
		case Direction.South:
			_dungeonLevel.MovePlayerSouth();
			break;
		}
		if (_grog.Moves == moves || _dungeonLevel.IsItemAt(_grog.X, _grog.Y) || _dungeonLevel.HasFeatureAt(_grog.X, _grog.Y) || _dungeonLevel.IsMonsterAround(_grog.X, _grog.Y) || (_dungeonLevel.IsRoomAt(_grog.X, _grog.Y) && _dungeonLevel.GetTileAt(_grog.X, _grog.Y) != Tile.Floor))
		{
			return false;
		}
		return true;
	}
}
