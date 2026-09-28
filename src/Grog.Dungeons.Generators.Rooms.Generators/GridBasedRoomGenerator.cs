using System;
using System.Collections.Generic;
using Grog.Dressings;
using Grog.Dungeons.Generators.Rooms.Builders;
using Grog.Kernel;

namespace Grog.Dungeons.Generators.Rooms.Generators;

public class GridBasedRoomGenerator : RoomGeneratorBase
{
	private const int SecretTunnelProbability = 12;

	private readonly int _horizontalGrid;

	private readonly int _verticalGrid;

	private readonly IRoomBuilder _roomBuilder;

	private static int[] _xm = new int[4] { 0, 1, 0, -1 };

	private static int[] _ym = new int[4] { -1, 0, 1, 0 };

	public GridBasedRoomGenerator(DungeonLevel dungeonLevel, int horizontalGrid, int verticalGrid, IRoomBuilder roomBuilder)
		: base(dungeonLevel)
	{
		_horizontalGrid = horizontalGrid;
		_verticalGrid = verticalGrid;
		_roomBuilder = roomBuilder;
	}

	private void ContinueConnecting(DungeonLevel dungeonLevel, IRoom[][] gridRoom, bool[,] connected, int gridWidth, int gridHeight, int x, int y, bool createExtraConnections)
	{
		if (connected[x, y] && Game.Instance.Random(100) < 80 && !createExtraConnections)
		{
			return;
		}
		connected[x, y] = true;
		List<Direction> list = new List<Direction>();
		for (Direction direction = Direction.MinDirection; direction <= Direction.West; direction++)
		{
			list.Add(direction);
		}
		for (int i = 0; i < list.Count; i++)
		{
			int num = Game.Instance.Random(list.Count);
			int num2;
			do
			{
				num2 = Game.Instance.Random(list.Count);
			}
			while (num == num2);
			Direction value = list[num];
			list[num] = list[num2];
			list[num2] = value;
		}
		int num3 = 100;
		foreach (Direction item in list)
		{
			int num4 = x + _xm[(int)item];
			int num5 = y + _ym[(int)item];
			if (num4 >= 0 && num5 >= 0 && num4 < gridWidth && num5 < gridHeight && !gridRoom[x][y].HasTunnelTo(item) && (!connected[num4, num5] || ((Game.Instance.Random(100) >= 60 || createExtraConnections) && !((Game.Instance.Random(100) >= num3) & createExtraConnections))))
			{
				DigTunnel(dungeonLevel, item, x, y, num4, num5, gridRoom);
				if (!createExtraConnections)
				{
					ContinueConnecting(dungeonLevel, gridRoom, connected, gridWidth, gridHeight, num4, num5, createExtraConnections: false);
				}
				else
				{
					num3 /= 3;
				}
			}
		}
	}

	private void DigTunnel(DungeonLevel dungeonLevel, Direction direction, int x, int y, int nx, int ny, IRoom[][] gridRoom)
	{
		if (direction == Direction.MinDirection || direction == Direction.South)
		{
			DigVerticalTunnel(dungeonLevel, direction, x, y, nx, ny, gridRoom);
		}
		else
		{
			DigHorizontalTunnel(dungeonLevel, direction, x, y, nx, ny, gridRoom);
		}
	}

	private void DigHorizontalTunnel(DungeonLevel dungeonLevel, Direction direction, int x, int y, int nx, int ny, IRoom[][] gridRoom)
	{
		IRoom room = gridRoom[x][y];
		IRoom room2 = gridRoom[nx][ny];
		Direction direction2 = ((direction == Direction.West) ? Direction.East : Direction.West);
		if (room.BoundingX1 > room2.BoundingX1)
		{
			IRoom room3 = room;
			room = room2;
			room2 = room3;
			direction = Direction.East;
			direction2 = Direction.West;
		}
		Position position = (room.HasDoorTo(direction) ? room.GetDoorLeadingTo(direction) : room.GetInsidePosition());
		Position position2 = (room2.HasDoorTo(direction2) ? room2.GetDoorLeadingTo(direction) : room2.GetInsidePosition());
		int num = position.X;
		int i = position.Y;
		bool flag = Game.Instance.Probability(12);
		for (; !room.IsWallOfRoom(num, i); i += _ym[(int)direction])
		{
			num += _xm[(int)direction];
		}
		if (!room.HasDoorTo(direction))
		{
			dungeonLevel.SetTile(num, i, flag ? Tile.SecretDoor : Tile.Door);
			room.DefineDoor(num, i, direction);
		}
		num += _xm[(int)direction];
		i += _ym[(int)direction];
		dungeonLevel.SetTile(num, i, Tile.Tunnel);
		for (int num2 = Game.Instance.Random(room2.BoundingX1 - num); num2 > 0; num2--)
		{
			num += _xm[(int)direction];
			i += _ym[(int)direction];
			dungeonLevel.SetTile(num, i, Tile.Tunnel);
		}
		while (i != position2.Y)
		{
			i += Math.Sign(position2.Y - i);
			dungeonLevel.SetTile(num, i, Tile.Tunnel);
		}
		while (true)
		{
			num += _xm[(int)direction];
			i += _ym[(int)direction];
			if (room2.IsPartOfRoom(num, i))
			{
				break;
			}
			dungeonLevel.SetTile(num, i, Tile.Tunnel);
		}
		dungeonLevel.SetTile(num, i, flag ? Tile.SecretDoor : Tile.Door);
		room2.DefineDoor(num, i, direction2);
		room2.DefineTunnel(direction2);
		room.DefineTunnel(direction);
	}

	private void DigVerticalTunnel(DungeonLevel dungeonLevel, Direction direction, int x, int y, int nx, int ny, IRoom[][] gridRoom)
	{
		IRoom room = gridRoom[x][y];
		IRoom room2 = gridRoom[nx][ny];
		Direction direction2 = ((direction == Direction.MinDirection) ? Direction.South : Direction.MinDirection);
		if (room.BoundingY1 > room2.BoundingY1)
		{
			IRoom room3 = room;
			room = room2;
			room2 = room3;
			direction = Direction.South;
			direction2 = Direction.MinDirection;
		}
		Position position = (room.HasDoorTo(direction) ? room.GetDoorLeadingTo(direction) : room.GetInsidePosition());
		Position position2 = (room2.HasDoorTo(direction2) ? room2.GetDoorLeadingTo(direction) : room2.GetInsidePosition());
		int num = position.X;
		int i = position.Y;
		bool flag = Game.Instance.Probability(12);
		for (; !room.IsWallOfRoom(num, i); i += _ym[(int)direction])
		{
			num += _xm[(int)direction];
		}
		if (!room.HasDoorTo(direction))
		{
			dungeonLevel.SetTile(num, i, flag ? Tile.SecretDoor : Tile.Door);
			room.DefineDoor(num, i, direction);
		}
		num += _xm[(int)direction];
		i += _ym[(int)direction];
		dungeonLevel.SetTile(num, i, Tile.Tunnel);
		int num2 = Game.Instance.Random(room2.BoundingY1 - i);
		while (num2 > 0)
		{
			num += _xm[(int)direction];
			i += _ym[(int)direction];
			num2--;
			dungeonLevel.SetTile(num, i, Tile.Tunnel);
		}
		while (num != position2.X)
		{
			num += Math.Sign(position2.X - num);
			dungeonLevel.SetTile(num, i, Tile.Tunnel);
		}
		while (true)
		{
			num += _xm[(int)direction];
			i += _ym[(int)direction];
			if (room2.IsPartOfRoom(num, i))
			{
				break;
			}
			dungeonLevel.SetTile(num, i, Tile.Tunnel);
		}
		dungeonLevel.SetTile(num, i, flag ? Tile.SecretDoor : Tile.Door);
		room2.DefineDoor(num, i, direction2);
		room2.DefineTunnel(direction2);
		room.DefineTunnel(direction);
	}

	public override void Generate(DungeonLevel dungeonLevel)
	{
		int num = dungeonLevel.Width / _horizontalGrid;
		int num2 = dungeonLevel.Height / _verticalGrid;
		IRoom[][] array = new IRoom[num][];
		for (int i = 0; i < _horizontalGrid; i++)
		{
			array[i] = new IRoom[num2];
			for (int j = 0; j < _verticalGrid; j++)
			{
				array[i][j] = _roomBuilder.DigRoom(dungeonLevel, i * num, j * num2, num, num2);
			}
		}
		bool[,] connected = new bool[_horizontalGrid, _verticalGrid];
		int x = Game.Instance.Random(_horizontalGrid);
		int y = Game.Instance.Random(_verticalGrid);
		ContinueConnecting(dungeonLevel, array, connected, _horizontalGrid, _verticalGrid, x, y, createExtraConnections: false);
		for (int k = 0; k < Game.Instance.Random(3) + 1; k++)
		{
			x = Game.Instance.Random(_horizontalGrid);
			y = Game.Instance.Random(_verticalGrid);
			ContinueConnecting(dungeonLevel, array, connected, _horizontalGrid, _verticalGrid, x, y, createExtraConnections: true);
		}
	}
}
