using System;
using System.Collections.Generic;
using Grog.Dressings;
using Grog.Dungeons.Generators.Rooms.SpecialMessages;
using Grog.Kernel;
using Grog.Kernel.Interactions;

namespace Grog.Dungeons.Generators.Rooms;

[Serializable]
public class RoomBase : IRoom
{
	private int _boundX1 = int.MaxValue;

	private int _boundY1 = int.MaxValue;

	private int _boundX2 = int.MinValue;

	private int _boundY2 = int.MinValue;

	private readonly HashSet<Position> _wallPositions = new HashSet<Position>();

	private readonly List<Position> _floorPositions = new List<Position>();

	private List<Position> _trulyInsidePositions;

	private readonly Dictionary<Direction, Position> _directionalDoorPositions = new Dictionary<Direction, Position>();

	private readonly HashSet<Position> _doorPositions = new HashSet<Position>();

	private readonly HashSet<Direction> _hasTunnelTo = new HashSet<Direction>();

	public bool IsSpecial
	{
		get
		{
			if (!IsDark && SpecialMessage == null)
			{
				return SpecialPower != SpecialRoomPower.None;
			}
			return true;
		}
		set
		{
			if (!value)
			{
				IsDark = false;
				SpecialMessage = null;
				SpecialPower = SpecialRoomPower.None;
			}
		}
	}

	public bool IsDark { get; set; }

	public ISpecialMessageProvider SpecialMessage { get; set; }

	public SpecialRoomPower SpecialPower { get; set; }

	public int Width => _boundX2 - _boundX1 + 1;

	public int Height => _boundY2 - _boundY1 + 1;

	public int BoundingX1 => _boundX1;

	public int BoundingY1 => _boundY1;

	public int BoundingX2 => _boundX2;

	public int BoundingY2 => _boundY2;

	public int DoorCount { get; }

	public IBeingInteraction WhenMovingWithinRoom { get; set; }

	protected RoomBase()
	{
		SpecialPower = SpecialRoomPower.None;
	}

	public virtual bool IsOutsideOfRoom(int x, int y)
	{
		Position item = new Position(x, y);
		if (!_wallPositions.Contains(item))
		{
			return !_floorPositions.Contains(item);
		}
		return false;
	}

	public virtual bool IsWallOfRoom(int x, int y)
	{
		Position item = new Position(x, y);
		return _wallPositions.Contains(item);
	}

	public virtual bool IsInsideOfRoom(int x, int y)
	{
		Position item = new Position(x, y);
		return _floorPositions.Contains(item);
	}

	public Position GetInsidePosition()
	{
		return _floorPositions[Game.Instance.Random(_floorPositions.Count)];
	}

	public bool IsPartOfRoom(int x, int y)
	{
		if (!IsWallOfRoom(x, y))
		{
			return IsInsideOfRoom(x, y);
		}
		return true;
	}

	public Position GetInsidePositionForThing(DungeonLevel dungeonLevel)
	{
		return GetAppropriateInsidePosition(dungeonLevel, (int xp, int yp) => dungeonLevel.IsOpenForNewThing(xp, yp));
	}

	private Position GetAppropriateInsidePosition(DungeonLevel dungeonLevel, Func<int, int, bool> isAppropriate)
	{
		int num = 5;
		while (num-- > 0)
		{
			Position insidePosition = GetInsidePosition();
			if (isAppropriate(insidePosition.X, insidePosition.Y))
			{
				return insidePosition;
			}
		}
		List<Position> insidePositions = GetInsidePositions(copy: true);
		insidePositions.Shuffle();
		foreach (Position item in insidePositions)
		{
			if (isAppropriate(item.X, item.Y))
			{
				return item;
			}
		}
		return Position.Undefined;
	}

	public Position GetInsidePositionForItem(DungeonLevel dungeonLevel)
	{
		return GetAppropriateInsidePosition(dungeonLevel, (int xp, int yp) => !dungeonLevel.HasItems(xp, yp));
	}

	public bool ForEachTrulyInsidePosition(Func<IRoom, Position, bool> isTrueCondition)
	{
		foreach (Position trulyInsidePosition in GetTrulyInsidePositions())
		{
			if (!isTrueCondition(this, trulyInsidePosition))
			{
				return false;
			}
		}
		return true;
	}

	public List<Position> GetTrulyInsidePositions(bool copy = false)
	{
		PrepareTrulyInsidePositions();
		if (!copy)
		{
			return _trulyInsidePositions;
		}
		return new List<Position>(_trulyInsidePositions);
	}

	public List<Position> GetInsidePositions(bool copy = false)
	{
		if (!copy)
		{
			return _floorPositions;
		}
		return new List<Position>(_floorPositions);
	}

	public void DefineDoor(int x, int y, Direction direction)
	{
		Position position = new Position(x, y);
		_directionalDoorPositions.Add(direction, position);
		_doorPositions.Add(position);
	}

	public bool HasDoorTo(Direction direction)
	{
		return _directionalDoorPositions.ContainsKey(direction);
	}

	public Position GetDoorLeadingTo(Direction direction)
	{
		if (!_directionalDoorPositions.ContainsKey(direction))
		{
			throw new GrogException("No door found for direction " + direction);
		}
		return _directionalDoorPositions[direction];
	}

	public void DefineTunnel(Direction direction)
	{
		_hasTunnelTo.Add(direction);
	}

	public bool HasTunnelTo(Direction direction)
	{
		return _hasTunnelTo.Contains(direction);
	}

	public Position GetTrulyInsidePositionForThing(DungeonLevel dungeonLevel)
	{
		int num = 1000;
		while (num-- > 0)
		{
			Position trulyInsidePosition = GetTrulyInsidePosition();
			if (trulyInsidePosition.Equals(Position.Undefined))
			{
				return trulyInsidePosition;
			}
			if (dungeonLevel.IsOpenForNewThing(trulyInsidePosition.X, trulyInsidePosition.Y))
			{
				return trulyInsidePosition;
			}
		}
		return Position.Undefined;
	}

	public Position GetTrulyInsidePosition()
	{
		PrepareTrulyInsidePositions();
		if (_trulyInsidePositions.Count != 0)
		{
			return _trulyInsidePositions[Game.Instance.Random(_trulyInsidePositions.Count)];
		}
		return GetInsidePosition();
	}

	public Position GetUnusedTrulyInsidePosition(DungeonLevel dungeonLevel)
	{
		PrepareTrulyInsidePositions();
		List<Position> list = new List<Position>();
		foreach (Position trulyInsidePosition in _trulyInsidePositions)
		{
			if (!dungeonLevel.HasFeatureAt(trulyInsidePosition) && dungeonLevel.GetTileAt(trulyInsidePosition) == Tile.Floor && !dungeonLevel.HasItems(trulyInsidePosition) && !dungeonLevel.IsBeingAt(trulyInsidePosition))
			{
				list.Add(trulyInsidePosition);
			}
		}
		if (list.Count == 0)
		{
			return Position.Undefined;
		}
		return list[Game.Instance.Random(list.Count)];
	}

	public bool IsBoring(DungeonLevel dungeonLevel)
	{
		if (IsSpecial)
		{
			return false;
		}
		foreach (Position insidePosition in GetInsidePositions())
		{
			if (dungeonLevel.IsItemAt(insidePosition.X, insidePosition.Y) || dungeonLevel.IsBeingAt(insidePosition.X, insidePosition.Y) || dungeonLevel.IsTrapAt(insidePosition.X, insidePosition.Y) || dungeonLevel.HasFeatureAt(insidePosition.X, insidePosition.Y) || dungeonLevel.GetThingAt(insidePosition.X, insidePosition.Y) != null)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsDoorAt(int x, int y)
	{
		return _doorPositions.Contains(new Position(x, y));
	}

	public List<Position> GetDoorPositions()
	{
		return new List<Position>(_doorPositions);
	}

	public Position GetPositionForThingAround(DungeonLevel dungeonLevel, int x, int y)
	{
		List<Position> list = new List<Position>();
		for (int i = x - 1; i <= x + 1; i++)
		{
			for (int j = y - 1; j <= y + 1; j++)
			{
				if ((x != i || y != j) && dungeonLevel.IsOpenForNewThing(i, j))
				{
					list.Add(new Position(i, j));
				}
			}
		}
		if (list.Count == 0)
		{
			return Position.Undefined;
		}
		return list[Game.Instance.Random(list.Count)];
	}

	public Position GetInsidePositionForFeature(DungeonLevel dungeonLevel)
	{
		int num = 5;
		while (num-- > 0)
		{
			Position insidePosition = GetInsidePosition();
			if (!dungeonLevel.HasFeatureAt(insidePosition.X, insidePosition.Y))
			{
				return insidePosition;
			}
		}
		List<Position> insidePositions = GetInsidePositions(copy: true);
		insidePositions.Shuffle();
		foreach (Position item in insidePositions)
		{
			if (!dungeonLevel.HasFeatureAt(item.X, item.Y))
			{
				return item;
			}
		}
		return Position.Undefined;
	}

	private void PrepareTrulyInsidePositions()
	{
		if (_trulyInsidePositions != null)
		{
			return;
		}
		_trulyInsidePositions = new List<Position>();
		foreach (Position floorPosition in _floorPositions)
		{
			if (!IsWallOfRoom(floorPosition.X - 1, floorPosition.Y) && !IsWallOfRoom(floorPosition.X + 1, floorPosition.Y) && !IsWallOfRoom(floorPosition.X, floorPosition.Y - 1) && !IsWallOfRoom(floorPosition.X, floorPosition.Y + 1))
			{
				_trulyInsidePositions.Add(floorPosition);
			}
		}
	}

	protected void AddWall(int x, int y)
	{
		_wallPositions.Add(new Position(x, y));
		_boundX1 = Math.Min(x, _boundX1);
		_boundY1 = Math.Min(y, _boundY1);
		_boundX2 = Math.Max(x, _boundX2);
		_boundY2 = Math.Max(y, _boundY2);
	}

	protected void AddFloor(int x, int y)
	{
		_floorPositions.Add(new Position(x, y));
		_boundX1 = Math.Min(x, _boundX1);
		_boundY1 = Math.Min(y, _boundY1);
		_boundX2 = Math.Max(x, _boundX2);
		_boundY2 = Math.Max(y, _boundY2);
	}

	public override string ToString()
	{
		return "Room/" + base.ToString() + " (bounding box: " + BoundingX1 + ", " + BoundingY1 + " - " + BoundingX2 + ", " + BoundingY2 + ")";
	}
}
