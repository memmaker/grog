using Grog.Dressings;
using Grog.Dressings.Beings;
using Grog.Dressings.Items;
using Grog.Dressings.Items.Types;
using Grog.Kernel;

namespace Grog.Dungeons.Generators.Rooms.Generators;

public class SpiralDungeonGenerator : RoomGeneratorBase
{
	public SpiralDungeonGenerator(DungeonLevel dungeonLevel)
		: base(dungeonLevel)
	{
	}

	public override void Generate(DungeonLevel dungeonLevel)
	{
		int num = Game.Instance.Random(2);
		Direction[] array = new Direction[0];
		int num2;
		int num3;
		switch (Game.Instance.Random(4))
		{
		case 0:
			num2 = (num3 = 1);
			array = ((num != 0) ? new Direction[4]
			{
				Direction.East,
				Direction.South,
				Direction.West,
				Direction.MinDirection
			} : new Direction[4]
			{
				Direction.South,
				Direction.East,
				Direction.MinDirection,
				Direction.West
			});
			break;
		case 1:
			num2 = 1;
			num3 = dungeonLevel.Height - 2;
			array = ((num != 0) ? new Direction[4]
			{
				Direction.East,
				Direction.MinDirection,
				Direction.West,
				Direction.South
			} : new Direction[4]
			{
				Direction.MinDirection,
				Direction.East,
				Direction.South,
				Direction.West
			});
			break;
		case 2:
			num2 = dungeonLevel.Width - 2;
			num3 = dungeonLevel.Height - 2;
			array = ((num != 0) ? new Direction[4]
			{
				Direction.West,
				Direction.MinDirection,
				Direction.East,
				Direction.South
			} : new Direction[4]
			{
				Direction.MinDirection,
				Direction.West,
				Direction.South,
				Direction.East
			});
			break;
		default:
			num2 = dungeonLevel.Width - 2;
			num3 = 1;
			array = ((num != 0) ? new Direction[4]
			{
				Direction.West,
				Direction.South,
				Direction.East,
				Direction.MinDirection
			} : new Direction[4]
			{
				Direction.South,
				Direction.West,
				Direction.MinDirection,
				Direction.East
			});
			break;
		}
		dungeonLevel.SetTile(num2, num3, Tile.StairUp);
		int num4 = 0;
		int num5 = 0;
		do
		{
			(int, int) directionalModifiers = array[num4].GetDirectionalModifiers();
			int item = directionalModifiers.Item1;
			int item2 = directionalModifiers.Item2;
			int num6 = num2 + 2 * item;
			int num7 = num3 + 2 * item2;
			if (dungeonLevel.IsValid(num6, num7) && (array[num4] != Direction.MinDirection || num7 > 2) && (array[num4] != Direction.South || num7 < dungeonLevel.Height - 2) && (array[num4] != Direction.West || num6 > 2) && (array[num4] != Direction.East || num7 < dungeonLevel.Width - 2) && dungeonLevel.GetTileAt(num6, num7) == Tile.Wall)
			{
				num2 += item;
				num3 += item2;
				dungeonLevel.SetTile(num2, num3, Tile.Tunnel);
				num2 += item;
				num3 += item2;
				dungeonLevel.SetTile(num2, num3, Tile.Tunnel);
				num5 = 0;
			}
			else
			{
				num4 = (num4 + 1) % array.Length;
				num5++;
			}
		}
		while (num5 < 2);
		dungeonLevel.SetTile(num2, num3, Tile.StairDown);
		Distribute(dungeonLevel, 16, (int xp, int yp) => dungeonLevel.IsValid(xp, yp) && dungeonLevel.GetTileAt(xp, yp) == Tile.Tunnel && !dungeonLevel.IsBeingAt(xp, yp), () => Game.Instance.MonsterPool.CreateRandomMonster(dungeonLevel), (int xp, int yp, Being m) =>
		{
			dungeonLevel.SetBeing(xp, yp, m);
		});
		Distribute(dungeonLevel, 2, (int xp, int yp) => dungeonLevel.IsValid(xp, yp) && dungeonLevel.GetTileAt(xp, yp) == Tile.Tunnel, () => Game.Instance.ItemPool.CreateRandomItemOfType(dungeonLevel, ItemType.Food), (int xp, int yp, Item i) =>
		{
			dungeonLevel.AddItem(xp, yp, i);
		});
		Distribute(dungeonLevel, 6 + Game.Instance.Random(4), (int xp, int yp) => dungeonLevel.IsValid(xp, yp) && dungeonLevel.GetTileAt(xp, yp) == Tile.Tunnel, () => Game.Instance.ItemPool.CreateRandomItem(dungeonLevel), (int xp, int yp, Item i) =>
		{
			dungeonLevel.AddItem(xp, yp, i);
		});
	}
}
