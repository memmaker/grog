/* RVIP: floating menu drawn by the game (Enter menu, item menus). Box sized to its content
   (longest entry + one space of padding + border), centred; scrolls only when taller than the screen.
   Keys: arrows / numpad 8 2 move, Enter / numpad 5 / numpad 6 choose, Escape / numpad 4 / 0 / . close,
   an entry's own key chooses it. Returns the chosen index, -1 = closed. */
using System;
using System.Collections.Generic;

namespace Grog.Kernel.GCurses;

public class PopupEntry
{
	public string KeyLabel, Text;
	public Func<ConsoleKeyInfo, bool> Matches; // null = header line (not selectable)
	public PopupEntry(string keyLabel, string text, Func<ConsoleKeyInfo, bool> matches) { KeyLabel = keyLabel; Text = text; Matches = matches; }
	public static PopupEntry Header(string text) => new PopupEntry(null, text, null);
	public static Func<ConsoleKeyInfo, bool> Char(char c) => k => k.KeyChar == c && (k.Modifiers & ConsoleModifiers.Control) == 0;
	public static Func<ConsoleKeyInfo, bool> Ctrl(ConsoleKey key) => k => k.Key == key && (k.Modifiers & ConsoleModifiers.Control) != 0;
}

public static class Popup
{
	public static int Choose(string title, List<PopupEntry> entries, int cursor = 0)
	{
		var c = Curses.Instance;
		int kw = 0, tw = 0;
		foreach (var e in entries) if (e.KeyLabel != null) kw = Math.Max(kw, e.KeyLabel.Length);
		var lines = new List<string>();
		foreach (var e in entries)
		{
			string s = e.KeyLabel == null ? "-- " + e.Text + " --" : e.KeyLabel.PadRight(kw) + "  " + e.Text;
			lines.Add(s);
			tw = Math.Max(tw, s.Length);
		}
		tw = Math.Max(tw, title.Length);
		int w = Math.Min(c.WindowWidth, tw + 4), rows = Math.Min(lines.Count, c.WindowHeight - 3);
		int h = rows + 3, x0 = (c.WindowWidth - w) / 2, y0 = Math.Max(0, (c.WindowHeight - h) / 2), top = 0;
		if (cursor < 0 || cursor >= entries.Count || entries[cursor].Matches == null) cursor = Next(entries, -1, 1);
		while (true)
		{
			if (cursor < top) top = cursor;
			if (cursor >= top + rows) top = cursor - rows + 1;
			if (cursor == Next(entries, -1, 1)) top = 0; // show the first group's header
			Line(x0, y0, "+" + new string('-', w - 2) + "+");
			Line(x0, y0 + 1, "| " + Fit(title, w - 4) + " |");
			for (int i = 0; i < rows; i++)
			{
				int n = top + i;
				c.SetCursorPosition(x0, y0 + 2 + i);
				c.Write(top > 0 && i == 0 ? '^' : top + rows < lines.Count && i == rows - 1 ? 'v' : '|');
				c.Write(' ');
				if (n == cursor) c.InvertColors();
				c.Write(Fit(lines[n], w - 4));
				if (n == cursor) c.InvertColors();
				c.Write(" |");
			}
			Line(x0, y0 + 2 + rows, "+" + new string('-', w - 2) + "+");
			c.SetCursorPosition(x0 + 2, y0 + 2 + cursor - top);
			ConsoleKeyInfo k = c.ReadKey(showReadKey: false, showCursor: false);
			switch (k.Key)
			{
			case ConsoleKey.UpArrow: case ConsoleKey.NumPad8: cursor = Next(entries, cursor, -1); continue;
			case ConsoleKey.DownArrow: case ConsoleKey.NumPad2: cursor = Next(entries, cursor, 1); continue;
			case ConsoleKey.PageUp: for (int i = 0; i < rows; i++) cursor = Next(entries, cursor, -1, false); continue;
			case ConsoleKey.PageDown: for (int i = 0; i < rows; i++) cursor = Next(entries, cursor, 1, false); continue;
			case ConsoleKey.Home: cursor = Next(entries, -1, 1); continue;
			case ConsoleKey.End: cursor = Next(entries, entries.Count, -1); continue;
			case ConsoleKey.Enter: case ConsoleKey.NumPad5: case ConsoleKey.NumPad6: return cursor;
			case ConsoleKey.Escape: case ConsoleKey.NumPad4: case ConsoleKey.NumPad0: return -1;
			}
			for (int i = 0; i < entries.Count; i++)
				if (entries[i].Matches != null && entries[i].Matches(k)) return i;
			if (k.KeyChar == '.' || k.KeyChar == '0') return -1; // numpad 0 / . close (when no entry uses them)
		}
	}

	static int Next(List<PopupEntry> e, int from, int dir, bool wrap = true)
	{
		for (int i = 1; i <= e.Count; i++)
		{
			int n = from + dir * i;
			if (!wrap && (n < 0 || n >= e.Count)) return from;
			n = ((n % e.Count) + e.Count) % e.Count;
			if (e[n].Matches != null) return n;
		}
		return 0;
	}

	static string Fit(string s, int w) => s.Length > w ? s.Substring(0, w) : s.PadRight(w);

	static void Line(int x, int y, string s) { Curses.Instance.SetCursorPosition(x, y); Curses.Instance.Write(s); }
}
