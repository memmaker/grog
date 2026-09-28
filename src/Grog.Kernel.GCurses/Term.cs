/* RVIP web port: the one place Grog touches System.Console.
   With no backend (native build) everything goes to System.Console as before.
   With a backend (browser: web/wasm/WebBackend.cs, headless test feed:
   web/native/Headless.cs) Curses hands it its finished cell buffer
   (Curses.Refresh), keys come from the backend, and files live under DataRoot. */
using System;
using System.Threading;

namespace Grog.Kernel.GCurses;

public interface ITermBackend
{
	bool KeyAvailable();
	ConsoleKeyInfo ReadKey(); // blocks until a key arrives
	void Sleep(int ms);
	void Present(int[] cells, int cols, int rows, int cursorX, int cursorY, bool cursorVisible); // 3 ints per cell: char, fg 0xRRGGBB, bg 0xRRGGBB
	void Beep();
}

public static class Term
{
	public static ITermBackend Backend;
	public const int Cols = 80, Rows = 26; // the size upstream asks for (Console.SetWindowSize(80, 26))
	public static string DataRoot; // backend: every game file (saves, options, highscores, ghosts, revenge monsters) goes here

	// Windows console palette (conhost defaults), game colours are ConsoleColor values.
	public static readonly int[] Palette = {
		0x000000, 0x000080, 0x008000, 0x008080, 0x800000, 0x800080, 0x808000, 0xC0C0C0,
		0x808080, 0x0000FF, 0x00FF00, 0x00FFFF, 0xFF0000, 0xFF00FF, 0xFFFF00, 0xFFFFFF };

	static bool cursorVisible;
	public static bool CursorVisible { get => Backend != null ? cursorVisible : Console.CursorVisible; set { if (Backend != null) cursorVisible = value; else Console.CursorVisible = value; } }
	public static bool TreatControlCAsInput { set { if (Backend == null) Console.TreatControlCAsInput = value; } }
	public static int WindowWidth { get => Backend != null ? Cols : Console.WindowWidth; set { if (Backend == null) Console.WindowWidth = value; } }
	public static int WindowHeight { get => Backend != null ? Rows : Console.WindowHeight; set { if (Backend == null) Console.WindowHeight = value; } }
	public static int BufferWidth { get => Backend != null ? Cols : Console.BufferWidth; set { if (Backend == null) Console.BufferWidth = value; } }
	public static int BufferHeight { get => Backend != null ? Rows : Console.BufferHeight; set { if (Backend == null) Console.BufferHeight = value; } }
	public static string Title { set { if (Backend == null) Console.Title = value; } }
	public static ConsoleColor ForegroundColor { set { if (Backend == null) Console.ForegroundColor = value; } }
	public static ConsoleColor BackgroundColor { set { if (Backend == null) Console.BackgroundColor = value; } }
	public static void SetWindowSize(int w, int h) { if (Backend == null) Console.SetWindowSize(w, h); }
	public static void Clear() { if (Backend == null) Console.Clear(); }
	public static bool KeyAvailable => Backend != null ? Backend.KeyAvailable() : Console.KeyAvailable;
	public static ConsoleKeyInfo ReadKey() => Backend != null ? Backend.ReadKey() : Console.ReadKey(intercept: true);
	public static void Beep() { if (Backend != null) Backend.Beep(); else Console.Beep(); }
	public static void Sleep(int ms) { if (Backend != null) Backend.Sleep(ms); else Thread.Sleep(ms); }
	public static void WriteLine(string s) { if (Backend == null) Console.WriteLine(s); }
	public static void ReadLine() { if (Backend == null) Console.ReadLine(); }

	static int[] cells;
	internal static void Present(CursesTerminal t, int cx, int cy)
	{
		int w = t.WindowWidth, h = t.WindowHeight, n = 0;
		if (cells == null || cells.Length != w * h * 3) cells = new int[w * h * 3];
		for (int y = 0; y < h; y++)
			for (int x = 0; x < w; x++)
			{
				cells[n++] = t.GetCharacter(x, y);
				cells[n++] = Palette[(int)t.GetForegroundColor(x, y) & 15];
				cells[n++] = Palette[(int)t.GetBackgroundColor(x, y) & 15];
			}
		Backend.Present(cells, w, h, cx, cy, cursorVisible);
	}

	// Browser KeyboardEvent (code, key) -> ConsoleKeyInfo
	public static bool FromBrowser(string code, string key, bool shift, bool ctrl, bool alt, out ConsoleKeyInfo k)
	{
		k = default;
		ConsoleKey ck;
		char ch = '\0';
		switch (code)
		{
		case "Enter": case "NumpadEnter": ck = ConsoleKey.Enter; ch = '\r'; break;
		case "Escape": ck = ConsoleKey.Escape; ch = '\x1b'; break;
		case "Backspace": ck = ConsoleKey.Backspace; ch = '\b'; break;
		case "Tab": ck = ConsoleKey.Tab; ch = '\t'; break;
		case "ArrowUp": ck = ConsoleKey.UpArrow; break;
		case "ArrowDown": ck = ConsoleKey.DownArrow; break;
		case "ArrowLeft": ck = ConsoleKey.LeftArrow; break;
		case "ArrowRight": ck = ConsoleKey.RightArrow; break;
		case "Home": ck = ConsoleKey.Home; break;
		case "End": ck = ConsoleKey.End; break;
		case "PageUp": ck = ConsoleKey.PageUp; break;
		case "PageDown": ck = ConsoleKey.PageDown; break;
		case "Delete": ck = ConsoleKey.Delete; break;
		case "Insert": ck = ConsoleKey.Insert; break;
		default:
			if (code.StartsWith("Numpad") && code.Length == 7 && char.IsDigit(code[6]))
			{
				ck = ConsoleKey.NumPad0 + (code[6] - '0'); ch = code[6]; break;
			}
			if (code.Length >= 2 && code.Length <= 3 && code[0] == 'F' && int.TryParse(code.Substring(1), out int f) && f >= 1 && f <= 12)
			{
				ck = ConsoleKey.F1 + (f - 1); break;
			}
			if (key == null || key.Length != 1) return false;
			ch = key[0];
			if (ch >= 'a' && ch <= 'z') ck = ConsoleKey.A + (ch - 'a');
			else if (ch >= 'A' && ch <= 'Z') { ck = ConsoleKey.A + (ch - 'A'); shift = true; }
			else if (ch >= '0' && ch <= '9') ck = ConsoleKey.D0 + (ch - '0');
			else if (ch == ' ') ck = ConsoleKey.Spacebar;
			else ck = ch switch { ',' or '<' => ConsoleKey.OemComma, '.' or '>' => ConsoleKey.OemPeriod, '-' or '_' => ConsoleKey.OemMinus, '+' or '=' => ConsoleKey.OemPlus, _ => (ConsoleKey)0 };
			if (ctrl && ck >= ConsoleKey.A && ck <= ConsoleKey.Z) ch = (char)(ck - ConsoleKey.A + 1);
			break;
		}
		k = new ConsoleKeyInfo(ch, ck, shift, alt, ctrl);
		return true;
	}
}
