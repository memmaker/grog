using System.Collections.Generic;
using Grog.Dressings;
using Grog.Dressings.Beings;
using Grog.Dressings.Items;
using Grog.Dressings.Items.Types;
using Grog.Kernel;

namespace Grog.Dungeons.Generators.Rooms.Generators;

public class MazeGenerator : RoomGeneratorBase
{
	public MazeGenerator(DungeonLevel dungeonLevel)
		: base(dungeonLevel)
	{
	}

	public override void Generate(DungeonLevel dungeonLevel)
	{
		Stack<Position> stack = new Stack<Position>();
		Position position = new Position(dungeonLevel.Width / 2, dungeonLevel.Height / 2);
		dungeonLevel.SetTile(position.X, position.Y, Tile.StairUp);
		stack.Push(position);
		List<Direction> list = new List<Direction>();
		Position position2 = position;
		while (stack.Count > 0)
		{
			position = stack.Pop();
			list.Clear();
			for (Direction direction = Direction.MinDirection; direction <= Direction.West; direction++)
			{
				(int, int) directionalModifiers = direction.GetDirectionalModifiers();
				int item = directionalModifiers.Item1;
				int item2 = directionalModifiers.Item2;
				Position position3 = new Position(position.X + item * 2, position.Y + item2 * 2);
				if (position3.X > 0 && position3.X < dungeonLevel.Width - 1 && position3.Y > 0 && position3.Y < dungeonLevel.Height - 1 && dungeonLevel.GetTileAt(position3) == Tile.Wall)
				{
					list.Add(direction);
				}
			}
			if (list.Count > 0)
			{
				stack.Push(position);
				(int, int) directionalModifiers2 = list[Game.Instance.Random(list.Count)].GetDirectionalModifiers();
				int item = directionalModifiers2.Item1;
				int item2 = directionalModifiers2.Item2;
				position = new Position(position.X + item, position.Y + item2);
				dungeonLevel.SetTile(position, Tile.Tunnel);
				position = new Position(position.X + item, position.Y + item2);
				dungeonLevel.SetTile(position, Tile.Tunnel);
				stack.Push(position);
				position2 = position;
			}
		}
		dungeonLevel.SetTile(position2, Tile.StairDown);
		Distribute(dungeonLevel, 8, (int xp, int yp) => dungeonLevel.IsValid(xp, yp) && dungeonLevel.GetTileAt(xp, yp) == Tile.Wall && ((dungeonLevel.IsValid(xp - 1, yp) && dungeonLevel.IsValid(xp + 1, yp) && dungeonLevel.GetTileAt(xp - 1, yp) == Tile.Tunnel && dungeonLevel.GetTileAt(xp + 1, yp) == Tile.Tunnel) || (dungeonLevel.IsValid(xp, yp - 1) && dungeonLevel.IsValid(xp, yp + 1) && dungeonLevel.GetTileAt(xp, yp - 1) == Tile.Tunnel && dungeonLevel.GetTileAt(xp, yp + 1) == Tile.Tunnel)), () => Tile.Tunnel, (int xp, int yp, Tile t) =>
		{
			dungeonLevel.SetTile(xp, yp, t);
		});
		Distribute(dungeonLevel, 32, (int xp, int yp) => dungeonLevel.IsValid(xp, yp) && dungeonLevel.GetTileAt(xp, yp) == Tile.Tunnel && !dungeonLevel.IsBeingAt(xp, yp), () => Game.Instance.MonsterPool.CreateRandomMonster(dungeonLevel), (int xp, int yp, Being m) =>
		{
			dungeonLevel.SetBeing(xp, yp, m);
		});
		Distribute(dungeonLevel, 6, (int xp, int yp) => dungeonLevel.IsValid(xp, yp) && dungeonLevel.GetTileAt(xp, yp) == Tile.Tunnel, () => Game.Instance.ItemPool.CreateRandomItemOfType(dungeonLevel, ItemType.Food), (int xp, int yp, Item i) =>
		{
			dungeonLevel.AddItem(xp, yp, i);
		});
		Distribute(dungeonLevel, 8 + Game.Instance.Random(8), (int xp, int yp) => dungeonLevel.IsValid(xp, yp) && dungeonLevel.GetTileAt(xp, yp) == Tile.Tunnel, () => Game.Instance.ItemPool.CreateRandomItem(dungeonLevel), (int xp, int yp, Item i) =>
		{
			dungeonLevel.AddItem(xp, yp, i);
		});
	}
}
