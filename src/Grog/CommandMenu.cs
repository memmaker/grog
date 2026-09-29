/* RVIP: the Enter menu. Every command of the help screen (Constants.HelpText), grouped like it,
   plus explore and the stairs walks. Choosing queues the command's key (Term.Push); the main loop runs it. */
using System;
using System.Collections.Generic;
using Grog.Kernel.GCurses;

namespace Grog;

public static class CommandMenu
{
	static int _cursor;

	static ConsoleKeyInfo K(char ch, ConsoleKey key, bool shift = false, bool ctrl = false) => new ConsoleKeyInfo(ch, key, shift, alt: false, ctrl);

	static readonly (string group, string label, string text, ConsoleKeyInfo key)[] Commands =
	{
		("Game", "^q", "Quit the game", K('\x11', ConsoleKey.Q, ctrl: true)),
		("Game", "Q", "Save & quit", K('Q', ConsoleKey.Q, shift: true)),
		("Game", "o", "Configure options", K('o', ConsoleKey.O)),
		("Movement", "w", "Move north", K('w', ConsoleKey.W)),
		("Movement", "a", "Move west", K('a', ConsoleKey.A)),
		("Movement", "s", "Move south", K('s', ConsoleKey.S)),
		("Movement", "d", "Move east", K('d', ConsoleKey.D)),
		("Movement", "W A S D", "Move quickly (asks direction)", default),
		("Resting", "SPACE", "Wait for one turn", K(' ', ConsoleKey.Spacebar)),
		("Resting", ".", "Wait until fully healed", K('.', ConsoleKey.OemPeriod)),
		("Dungeon", "r", "Interact (e.g. use stairs)", K('r', ConsoleKey.R)),
		("Dungeon", "g", "Explore (stops on danger/news)", K('g', ConsoleKey.G)),
		("Dungeon", "<", "Walk to known up stairs (again: take them)", K('<', ConsoleKey.OemComma, shift: true)),
		("Dungeon", ">", "Walk to known down stairs (again: take them)", K('>', ConsoleKey.OemPeriod, shift: true)),
		("Items", "i", "Manage your inventory", K('i', ConsoleKey.I)),
		("Items", "c", "Pick up items", K('c', ConsoleKey.C)),
		("Items", "u", "Use item", K('u', ConsoleKey.U)),
		("Items", "z", "Zap wand", K('z', ConsoleKey.Z)),
		("Items", "t", "Throw an item", K('t', ConsoleKey.T)),
		("Items", "p", "Pray for help", K('p', ConsoleKey.P)),
		("Information", "m", "Enemy (monster) list", K('m', ConsoleKey.M)),
		("Information", "^r", "Redraw the screen", K('\x12', ConsoleKey.R, ctrl: true)),
		("Information", "?", "Show the help", K('?', (ConsoleKey)0)),
		("Information", "v", "Display game version", K('v', ConsoleKey.V)),
	};

	public static void Show()
	{
		var entries = new List<PopupEntry>();
		var map = new List<int>();
		string group = null;
		for (int i = 0; i < Commands.Length; i++)
		{
			var c = Commands[i];
			if (c.group != group) { group = c.group; entries.Add(PopupEntry.Header(group)); map.Add(-1); }
			var key = c.key;
			Func<ConsoleKeyInfo, bool> match = c.key.Key == 0 && c.key.KeyChar == 0
				? (k => k.KeyChar == 'W' || k.KeyChar == 'A' || k.KeyChar == 'S' || k.KeyChar == 'D')
				: key.Modifiers.HasFlag(ConsoleModifiers.Control) ? PopupEntry.Ctrl(key.Key) : PopupEntry.Char(key.KeyChar);
			entries.Add(new PopupEntry(c.label, c.text, match));
			map.Add(i);
		}
		int n = Popup.Choose("Commands (Enter: run, Esc: close)", entries, _cursor);
		if (n < 0) return;
		_cursor = n;
		var cmd = Commands[map[n]];
		if (cmd.key.Key != 0 || cmd.key.KeyChar != 0) { Term.Push(cmd.key); return; }
		// "Move quickly": a direction, then the shifted movement key
		Curses.Instance.SetCursorPosition(0, 0);
		Curses.Instance.Write("Move quickly to which direction [wasd]? ");
		Curses.Instance.ClearToEndOfLine();
		ConsoleKeyInfo d = Curses.Instance.ReadKey();
		ConsoleKey dk = d.Key switch { ConsoleKey.W or ConsoleKey.UpArrow or ConsoleKey.NumPad8 => ConsoleKey.W, ConsoleKey.A or ConsoleKey.LeftArrow or ConsoleKey.NumPad4 => ConsoleKey.A, ConsoleKey.S or ConsoleKey.DownArrow or ConsoleKey.NumPad2 => ConsoleKey.S, ConsoleKey.D or ConsoleKey.RightArrow or ConsoleKey.NumPad6 => ConsoleKey.D, _ => 0 };
		if (dk != 0) Term.Push(K((char)('A' + (dk - ConsoleKey.A)), dk, shift: true));
	}
}
