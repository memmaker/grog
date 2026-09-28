using System;
using System.Collections.Generic;
using Grog.Dressings;
using Grog.Dressings.Beings;
using Grog.Dressings.Items.Types;
using Grog.Dungeons.Generators;
using Grog.Dungeons.Generators.Monsters;
using Grog.Dungeons.Generators.Rooms.Builders;
using Grog.Dungeons.Generators.Rooms.Decorators;
using Grog.Dungeons.Generators.Rooms.Decorators.DecorationGenerators;
using Grog.Dungeons.Generators.Rooms.Decorators.Features.Lib;
using Grog.Dungeons.Generators.Rooms.Decorators.Items;
using Grog.Dungeons.Generators.Rooms.Decorators.Tiles;
using Grog.Dungeons.Generators.Rooms.Generators;
using Grog.Kernel;
using Grog.Kernel.GCurses;

namespace Grog.Dungeons;

[Serializable]
public class DungeonMaster
{
	public const int MaxDungeonLevel = 25;

	public const int MazeLevel = 9;

	private DungeonLevel[] _dungeonLevels = new DungeonLevel[25];

	private int _currentDungeonLevelIndex = -1;

	public DungeonLevel CurrentDungeonLevel
	{
		get
		{
			if (_currentDungeonLevelIndex != -1)
			{
				return _dungeonLevels[_currentDungeonLevelIndex];
			}
			return null;
		}
		set
		{
			_currentDungeonLevelIndex = -1;
			for (int i = 0; i < _dungeonLevels.Length; i++)
			{
				if (_dungeonLevels[i] == value)
				{
					_currentDungeonLevelIndex = i;
					break;
				}
			}
		}
	}

	public int StandardHeightPerRoom => Curses.MinWindowHeight / 3;

	public int StandardWidthPerRoom => Curses.MinWindowWidth / 3;

	public int CrowdedHeightPerRoom => Curses.MinWindowHeight / 3;

	public int CrowdedWidthPerRoom => Curses.MinWindowWidth / 5;

	public int DeepestLevelReached { get; private set; }

	public IGenerator GetDungeonLevelGenerator(DungeonLevel dungeonLevel)
	{
		List<IRoomDecorator> roomDecorators = GetRoomDecorators(dungeonLevel, dungeonLevel.Level);
		switch (dungeonLevel.Level)
		{
		case 1:
			return Standard3X3Grid(dungeonLevel, roomDecorators, allowSpecialRooms: false);
		case 2:
			return Crowded5X3Grid(dungeonLevel, roomDecorators, allowSpecialRooms: false);
		case 5:
			return new SequentialGenerator(new SingleBigRoomGenerator(dungeonLevel), new SequentialRoomDecorationGenerator(new StartRoomDecorator(), new DownStairsRoomDecorator(), roomDecorators), new MultipleMonstersPerRoomGenerator(() => 12 + Game.Instance.Random(4)), new MultipleGeneratorsPerRoomGenerator(3 + Game.Instance.Random(4), new GoldFeatureRoomDecorator()), new SingeRoomDecoratorGenerator(new MultipleItemRoomDecorator(12)), new MultipleRoomDecorationGenerator(Game.Instance.Random(2), new TrapRoomDecorator()));
		case 13:
			return new SequentialGenerator(new GridBasedRoomGenerator(dungeonLevel, Curses.Instance.WindowWidth / CrowdedWidthPerRoom, Curses.Instance.WindowHeight / CrowdedHeightPerRoom, new RectangularRoomBuilder()), new SequentialRoomDecorationGenerator(new StartRoomDecorator(), new DownStairsRoomDecorator(), roomDecorators), new SpecialRoomGenerator(2), new MultipleMonstersPerRoomGenerator(() => 1 + Game.Instance.Random(3)), new MultipleRoomDecorationGenerator(Game.Instance.Random(4) + 3, new GoldFeatureRoomDecorator()), new GenerativeDecoratorSequentialRoomProcessingGenerator(new SequenceBasedRoomDecorationGenerator(new ProbableRoomDecorator(70, new SingleItemRoomDecorator()), new ProbableRoomDecorator(2, new MultipleItemRoomDecorator(4)), new ProbableRoomDecorator(20, new MultipleItemRoomDecorator(3)), new ProbableRoomDecorator(50, new MultipleItemRoomDecorator(2)), new ProbableRoomDecorator(10, new StatueFeatureRoomDecorator()))));
		case 18:
			return new SequentialGenerator(new GridBasedRoomGenerator(dungeonLevel, Curses.Instance.WindowWidth / CrowdedWidthPerRoom, Curses.Instance.WindowHeight / CrowdedHeightPerRoom, new RectangularRoomBuilder()), new SequentialRoomDecorationGenerator(new StartRoomDecorator(), new DownStairsRoomDecorator(), roomDecorators), new SpecialRoomGenerator(Game.Instance.Random(4) + 6), new OneMonsterPerRoomGenerator(), new MultipleRoomDecorationGenerator(Game.Instance.Random(6) + 2, new GoldFeatureRoomDecorator()), new GenerativeDecoratorSequentialRoomProcessingGenerator(new SequenceBasedRoomDecorationGenerator(new ProbableRoomDecorator(70, new SingleItemRoomDecorator()), new ProbableRoomDecorator(2, new MultipleItemRoomDecorator(4)), new ProbableRoomDecorator(20, new MultipleItemRoomDecorator(3)), new ProbableRoomDecorator(50, new MultipleItemRoomDecorator(2)), new ProbableRoomDecorator(10, new StatueFeatureRoomDecorator()))), new MultipleRoomDecorationGenerator(Game.Instance.Random(6), new TrapRoomDecorator()));
		case 25:
			return new SequentialGenerator(new GridBasedRoomGenerator(dungeonLevel, 2, 2, new RectangularRoomBuilder()), new SequentialRoomDecorationGenerator(new StartRoomDecorator(), new ThroneOfImmortalityRoomDecorator(), roomDecorators), new OneMonsterPerRoomGenerator());
		default:
			if (!Game.Instance.Probability(25))
			{
				if (!Game.Instance.Probability(25))
				{
					return Intermediate5X2Grid(dungeonLevel, roomDecorators, allowSpecialRooms: true);
				}
				return Crowded5X3Grid(dungeonLevel, roomDecorators, allowSpecialRooms: true);
			}
			return Standard3X3Grid(dungeonLevel, roomDecorators, allowSpecialRooms: true);
		}
	}

	private IGenerator Intermediate5X2Grid(DungeonLevel dungeonLevel, List<IRoomDecorator> decorators, bool allowSpecialRooms)
	{
		IGenerator[] array = new IGenerator[7]
		{
			new GridBasedRoomGenerator(dungeonLevel, Curses.Instance.WindowWidth / CrowdedWidthPerRoom, 2, new RectangularRoomBuilder()),
			new SequentialRoomDecorationGenerator(new StartRoomDecorator(), new DownStairsRoomDecorator(), decorators),
			null,
			null,
			null,
			null,
			null
		};
		IGenerator generator2;
		if (!allowSpecialRooms)
		{
			IGenerator generator = new NoGenerator();
			generator2 = generator;
		}
		else
		{
			IGenerator generator = new SpecialRoomGenerator(1 + ((dungeonLevel.Level >= 10) ? Game.Instance.Random(1 + dungeonLevel.Level / 10) : 0));
			generator2 = generator;
		}
		array[2] = generator2;
		array[3] = new OneMonsterPerRoomGenerator();
		array[4] = new MultipleRoomDecorationGenerator(Game.Instance.Random(6) + 2, new GoldFeatureRoomDecorator());
		array[5] = new GenerativeDecoratorSequentialRoomProcessingGenerator(new SequenceBasedRoomDecorationGenerator(new ProbableRoomDecorator(70, new SingleItemRoomDecorator()), new ProbableRoomDecorator(2, new MultipleItemRoomDecorator(4)), new ProbableRoomDecorator(20, new MultipleItemRoomDecorator(3)), new ProbableRoomDecorator(50, new MultipleItemRoomDecorator(2)), new ProbableRoomDecorator(10, new StatueFeatureRoomDecorator())));
		array[6] = new MultipleRoomDecorationGenerator(Game.Instance.Random(4), new TrapRoomDecorator());
		return new SequentialGenerator(array);
	}

	private IGenerator Crowded5X3Grid(DungeonLevel dungeonLevel, List<IRoomDecorator> decorators, bool allowSpecialRooms)
	{
		IGenerator[] array = new IGenerator[7]
		{
			new GridBasedRoomGenerator(dungeonLevel, Curses.Instance.WindowWidth / CrowdedWidthPerRoom, Curses.Instance.WindowHeight / CrowdedHeightPerRoom, new RectangularRoomBuilder()),
			new SequentialRoomDecorationGenerator(new StartRoomDecorator(), new DownStairsRoomDecorator(), decorators),
			null,
			null,
			null,
			null,
			null
		};
		IGenerator generator2;
		if (!allowSpecialRooms)
		{
			IGenerator generator = new NoGenerator();
			generator2 = generator;
		}
		else
		{
			IGenerator generator = new SpecialRoomGenerator(1 + ((dungeonLevel.Level >= 10) ? Game.Instance.Random(1 + dungeonLevel.Level / 10) : 0));
			generator2 = generator;
		}
		array[2] = generator2;
		array[3] = new OneMonsterPerRoomGenerator();
		array[4] = new MultipleRoomDecorationGenerator(Game.Instance.Random(6) + 2, new GoldFeatureRoomDecorator());
		array[5] = new GenerativeDecoratorSequentialRoomProcessingGenerator(new SequenceBasedRoomDecorationGenerator(new ProbableRoomDecorator(70, new SingleItemRoomDecorator()), new ProbableRoomDecorator(2, new MultipleItemRoomDecorator(4)), new ProbableRoomDecorator(20, new MultipleItemRoomDecorator(3)), new ProbableRoomDecorator(50, new MultipleItemRoomDecorator(2)), new ProbableRoomDecorator(10, new StatueFeatureRoomDecorator())));
		array[6] = new MultipleRoomDecorationGenerator(Game.Instance.Random(4), new TrapRoomDecorator());
		return new SequentialGenerator(array);
	}

	private IGenerator Standard3X3Grid(DungeonLevel dungeonLevel, List<IRoomDecorator> decorators, bool allowSpecialRooms)
	{
		IGenerator[] array = new IGenerator[7]
		{
			new GridBasedRoomGenerator(dungeonLevel, Curses.Instance.WindowWidth / StandardWidthPerRoom, Curses.Instance.WindowHeight / StandardHeightPerRoom, new RectangularRoomBuilder()),
			new SequentialRoomDecorationGenerator(new StartRoomDecorator(), new DownStairsRoomDecorator(), decorators),
			null,
			null,
			null,
			null,
			null
		};
		IGenerator generator2;
		if (!allowSpecialRooms)
		{
			IGenerator generator = new NoGenerator();
			generator2 = generator;
		}
		else
		{
			IGenerator generator = new SpecialRoomGenerator();
			generator2 = generator;
		}
		array[2] = generator2;
		array[3] = new OneMonsterPerRoomGenerator();
		array[4] = new MultipleRoomDecorationGenerator(Game.Instance.Random(4) + 1, new GoldFeatureRoomDecorator());
		array[5] = new MultipleRoomDecorationGenerator(Game.Instance.Random(2), new TrapRoomDecorator());
		array[6] = new GenerativeDecoratorSequentialRoomProcessingGenerator(new SequenceBasedRoomDecorationGenerator(new ProbableRoomDecorator(70, new SingleItemRoomDecorator()), new ProbableRoomDecorator(2, new MultipleItemRoomDecorator(4)), new ProbableRoomDecorator(20, new MultipleItemRoomDecorator(3)), new ProbableRoomDecorator(50, new MultipleItemRoomDecorator(2))));
		return new SequentialGenerator(array);
	}

	public List<IRoomDecorator> GetRoomDecorators(DungeonLevel dungeonLevel, int level)
	{
		List<IRoomDecorator> list = new List<IRoomDecorator>();
		if (level == 1)
		{
			list.Add(new StatueFeatureRoomDecorator());
		}
		if (level < 23)
		{
			list.Add(new LimitedNumberOfTypedItemsRoomDecorator(ItemType.Food, Game.Instance.Random(2) + 1));
		}
		if (level < 25)
		{
			list.Add(new ProbableRoomDecorator(10, new AltarFeatureRoomDecorator()));
		}
		list.Add(new GhostRoomDecorator());
		return list;
	}

	public void SetUpDungeonLevel(int level, Tile stairTile = null)
	{
		DeepestLevelReached = Math.Max(DeepestLevelReached, level);
		CurrentDungeonLevel?.RemoveThing(Game.Instance.Grog.X, Game.Instance.Grog.Y);
		if (_dungeonLevels[level - 1] == null)
		{
			CurrentDungeonLevel = (_dungeonLevels[level - 1] = new DungeonLevel(level));
		}
		else
		{
			if (stairTile == null)
			{
				throw new GrogException("Undefined stair tile!");
			}
			CurrentDungeonLevel = _dungeonLevels[level - 1];
		}
		Position position = CurrentDungeonLevel.FindPositionOfTile(stairTile);
		CurrentDungeonLevel.SetBeing(position.X, position.Y, Game.Instance.Grog);
	}

	public void DescendLevel()
	{
		try
		{
			Game.Save(42);
		}
		catch (Exception)
		{
			Game.RaiseError("Failed to generate an auto-save file. Now you truly are endangered.");
		}
		SetUpDungeonLevel(CurrentDungeonLevel.Level + 1, Tile.StairUp);
	}

	public void AscendLevel()
	{
		try
		{
			Game.Save(42);
		}
		catch (Exception)
		{
			Game.RaiseError("Failed to generate an auto-save file. Now you truly are endangered.");
		}
		if (CurrentDungeonLevel.Level == 1)
		{
			if (CurrentDungeonLevel.Sure("leave the dungeons of despair never to return"))
			{
				Game.Instance.Grog.HasLeftDungeon = true;
			}
		}
		else
		{
			SetUpDungeonLevel(CurrentDungeonLevel.Level - 1, Tile.StairDown);
		}
	}

	public void MakeCopiesUniqueAgain()
	{
		Dictionary<long, Being> dictionary = new Dictionary<long, Being>();
		DungeonLevel[] dungeonLevels = _dungeonLevels;
		foreach (DungeonLevel dungeonLevel in dungeonLevels)
		{
			if (dungeonLevel == null)
			{
				continue;
			}
			foreach (Being allBeing in dungeonLevel.GetAllBeings())
			{
				if (dictionary.ContainsKey(allBeing.UID))
				{
					Being being = dictionary[allBeing.UID];
					Game.RaiseError("Duplicate being while trying to make copies unique again:\n\tOld: " + being.UID + " -> " + being.Name + " (" + being.Type + ":" + being.X + ", " + being.Y + ")\n\tNew: " + allBeing.UID + " -> " + allBeing.Name + " (" + allBeing.Type + ": " + allBeing.X + ", " + allBeing.Y + ")\n");
				}
				dictionary.Add(allBeing.UID, allBeing);
			}
		}
		Game.Instance.Ghosts.MakeGhostsUniqueAgain(dictionary);
		Game.Instance.RevengeSystem.MakeRevengeMonstersUniqueAgain(dictionary);
	}

	public bool RestoreWorkableDungeonLevel()
	{
		_currentDungeonLevelIndex = 24;
		while (_currentDungeonLevelIndex >= 0 && _dungeonLevels[_currentDungeonLevelIndex] == null)
		{
			_currentDungeonLevelIndex--;
		}
		if (_currentDungeonLevelIndex == -1)
		{
			Game.RaiseError("No valid dungeon levels found.");
			return false;
		}
		return RestoreGrogPosition();
	}

	public bool RestoreGrogPosition()
	{
		Position position = CurrentDungeonLevel.FindPositionOfTile(Tile.StairUp);
		if (!position.Equals(Position.Undefined))
		{
			Thing thingAt = CurrentDungeonLevel.GetThingAt(position.X, position.Y);
			if (thingAt != null && !(thingAt is Player))
			{
				Game.RaiseError("Destroying " + thingAt.Character(CurrentDungeonLevel, thingAt.X, thingAt.Y) + "/" + thingAt.GetType().FullName + " at the entry position in order to make room for the player.");
			}
			CurrentDungeonLevel.SetBeing(position.X, position.Y, Game.Instance.Grog);
			return true;
		}
		return false;
	}
}
