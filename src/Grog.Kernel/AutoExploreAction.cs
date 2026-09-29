using System.Collections.Generic;
using Grog.Dressings;
using Grog.Dressings.Beings;
using Grog.Dressings.Features;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Rooms;

namespace Grog.Kernel;

// RVIP: auto-explore ('g') and walk-to-stairs ('<' / '>'), one step per turn.
// Known grid = DungeonLevel.MemoryOf(x, y) != ' ' (what the map shows).
public class AutoExploreAction : IAutomaticAction
{
	private readonly Player _grog;
	private readonly DungeonLevel _level;
	private readonly Tile _stairs; // null = explore
	private int _serial;

	public AutoExploreAction(Player grog, DungeonLevel level, Tile stairs)
	{
		_grog = grog;
		_level = level;
		_stairs = stairs;
		_serial = DungeonLevel.MessageSerial;
	}

	// Called on the key press. False = nothing to do (message given).
	public static bool Start(Player grog, DungeonLevel level, Tile stairs)
	{
		Note(level, grog);
		if (stairs != null && level.GetTileAt(grog.X, grog.Y) == stairs)
		{
			stairs.Interaction.Interact(level, grog);
			return false;
		}
		if (VisibleMonster(grog, level))
		{
			level.Message("Not with an enemy in sight!");
			return false;
		}
		if (NextStep(grog, level, stairs) == null)
		{
			level.Message(stairs == null ? "Nothing left to explore." : ("No known stairs " + (stairs == Tile.StairUp ? "up" : "down") + "."));
			return false;
		}
		grog.AutomaticAction = new AutoExploreAction(grog, level, stairs);
		return true;
	}

	public bool Execute()
	{
		if (_level != Game.Instance.DungeonMaster.CurrentDungeonLevel)
		{
			return false;
		}
		if (_stairs != null && _level.GetTileAt(_grog.X, _grog.Y) == _stairs)
		{
			_grog.ResetAutomaticAction(); // RVIP: only walk there; the key again takes the stairs
			return false;
		}
		if ((DungeonLevel.MessageSerial != _serial && !Game.Instance.IsFirstTurnWithAutomaticAction) || VisibleMonster(_grog, _level))
		{
			return false;
		}
		Direction? d = NextStep(_grog, _level, _stairs);
		if (d == null)
		{
			if (_stairs == null)
			{
				_level.Message("Nothing left to explore.");
			}
			return false;
		}
		Note(_level, _grog);
		if (global::Grog.Kernel.GCurses.Term.Backend != null && !Game.Instance.IsFirstTurnWithAutomaticAction)
		{
			global::Grog.Kernel.GCurses.Curses.Instance.Refresh(); // the loop rendered this turn; show it
			global::Grog.Kernel.GCurses.Term.Sleep(40); // RVIP: each explore / stairs step gets painted
		}
		int moves = _grog.Moves;
		_serial = DungeonLevel.MessageSerial; // messages of this step stop the next one
		switch (d.Value)
		{
		case Direction.East: _level.MovePlayerEast(); break;
		case Direction.West: _level.MovePlayerWest(); break;
		case Direction.MinDirection: _level.MovePlayerNorth(); break;
		default: _level.MovePlayerSouth(); break;
		}
		Note(_level, _grog);
		if (_grog.Moves != moves && _grog.AutomaticAction == null)
		{
			_grog.AutomaticAction = this; // MoveThing stops runs at room changes and doors; not this walk
		}
		return _grog.Moves != moves;
	}

	private static bool VisibleMonster(Player grog, DungeonLevel level)
	{
		foreach (Being b in level.GetAllBeingsVisibleTo(grog))
		{
			if (b != grog)
			{
				return true;
			}
		}
		return false;
	}

	// Known grid: remembered map character, in sight, or inside a lit room whose corner wall is
	// remembered (Grog shows a lit room's floor live and keeps only its walls in memory).
	// Cells next to where Grog has stood (tunnel sides are never drawn, so they would stay
	// unknown forever). Per level, not saved.
	private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<DungeonLevel, bool[]> Felt = new();

	public static void Note(DungeonLevel l, Player grog)
	{
		bool[] f = Felt.GetValue(l, k => new bool[k.Width * k.Height]);
		for (int dx = -1; dx <= 1; dx++)
			for (int dy = -1; dy <= 1; dy++)
				if (l.IsValid(grog.X + dx, grog.Y + dy)) f[grog.X + dx + (grog.Y + dy) * l.Width] = true;
	}

	private static bool Known(DungeonLevel l, int x, int y)
	{
		if (!l.IsValid(x, y)) return false;
		if (Felt.TryGetValue(l, out bool[] f) && f[x + y * l.Width]) return true;
		if (l.MemoryOf(x, y) != ' ' || l.Grog.CanSee(l, x, y)) return true;
		var r = l.GetRoomAt(x, y);
		return r != null && !r.IsDark && r.IsInsideOfRoom(x, y) && l.MemoryOf(r.BoundingX1, r.BoundingY1) != ' ';
	}

	private static bool Walkable(DungeonLevel l, int x, int y) =>
		Known(l, x, y) && l.GetTileAt(x, y).IsOpen && !(l.GetFeatureAt(x, y) is KnownTrapFeature);

	private static readonly (int dx, int dy, Direction d)[] Dirs =
	{
		(0, -1, Direction.MinDirection), (0, 1, Direction.South), (-1, 0, Direction.West), (1, 0, Direction.East)
	};

	private static bool IsGoal(DungeonLevel l, int x, int y, Tile stairs)
	{
		if (stairs != null)
		{
			return l.GetTileAt(x, y) == stairs;
		}
		foreach (var (dx, dy, _) in Dirs)
		{
			if (l.IsValid(x + dx, y + dy) && !Known(l, x + dx, y + dy))
			{
				return true;
			}
		}
		return false;
	}

	// BFS over the known grid (4-way, as the game moves); first step towards the nearest goal.
	private static Direction? NextStep(Player grog, DungeonLevel l, Tile stairs)
	{
		int w = l.Width, h = l.Height;
		var first = new int[w * h];
		for (int i = 0; i < first.Length; i++) first[i] = -1;
		var q = new Queue<(int, int)>();
		first[grog.X + grog.Y * w] = 4;
		q.Enqueue((grog.X, grog.Y));
		while (q.Count > 0)
		{
			var (x, y) = q.Dequeue();
			int f = first[x + y * w];
			if (f != 4 && IsGoal(l, x, y, stairs))
			{
				return Dirs[f].d;
			}
			for (int k = 0; k < 4; k++)
			{
				int nx = x + Dirs[k].dx, ny = y + Dirs[k].dy;
				if (!Walkable(l, nx, ny) || first[nx + ny * w] != -1) continue;
				first[nx + ny * w] = f == 4 ? k : f;
				q.Enqueue((nx, ny));
			}
		}
		return null;
	}
}
