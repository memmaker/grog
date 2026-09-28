using System;
using System.Text;

namespace Grog.Kernel.GCurses;

public class Curses
{
	private static Curses _instance;

	private int _cx;

	private int _cy;

	private ConsoleColor _foregroundColor;

	private ConsoleColor _backgroundColor;

	private bool _fullRefresh;

	private readonly CursesTerminal _currentTerminal;

	private readonly CursesTerminal _nextTerminal;

	private bool _isInverted;

	private bool _wasInverted;

	private bool _inversionPaused;

	private int _lastWindowWidth = -1;

	private int _lastWindowHeight = -1;

	public static Curses Instance => _instance ?? (_instance = new Curses());

	public int WindowHeight { get; private set; }

	public int WindowWidth { get; private set; }

	public int CursorX => _cx;

	public int CursorY => _cy;

	public static int MinWindowWidth => 70;

	public static int MinWindowHeight => 24;

	public bool IsKeyAvailable => Term.KeyAvailable;

	private Curses()
	{
		Term.CursorVisible = false;
		Term.TreatControlCAsInput = true;
		WindowWidth = Term.WindowWidth;
		WindowHeight = Term.WindowHeight;
		_currentTerminal = new CursesTerminal(WindowWidth, WindowHeight);
		_nextTerminal = new CursesTerminal(WindowWidth, WindowHeight);
		_foregroundColor = ConsoleColor.Black;
		_backgroundColor = ConsoleColor.White;
		Term.ForegroundColor = _foregroundColor;
		Term.BackgroundColor = _backgroundColor;
		for (int i = 0; i < WindowWidth; i++)
		{
			for (int j = 0; j < WindowHeight; j++)
			{
				_currentTerminal.SetChar(i, j, ' ', _foregroundColor, _backgroundColor);
				_nextTerminal.SetChar(i, j, ' ', _foregroundColor, _backgroundColor);
			}
		}
		Term.Clear();
	}

	public void InitializeColorSystem(ConsoleColor foreground, ConsoleColor background)
	{
		_nextTerminal.ReplaceColorSystem(_foregroundColor, _backgroundColor, foreground, background);
		_foregroundColor = foreground;
		_backgroundColor = background;
		Term.ForegroundColor = foreground;
		Term.BackgroundColor = background;
	}

	public void SetCursorPosition(int x, int y)
	{
		if (x < 0 || y < 0 || x >= WindowWidth || y >= WindowHeight)
		{
			throw new GrogException("Invalid cursor position requested: " + x + ", " + y + " (permissible: 0, 0 - " + WindowWidth + ", " + WindowHeight + ").");
		}
		_cx = x;
		_cy = y;
	}

	public void Write(char character, bool refresh = false)
	{
		switch (character)
		{
		case '\n':
			if (_cy < WindowHeight - 1)
			{
				_cy++;
			}
			_cx = 0;
			return;
		case '\t':
		{
			int num = (_cx + 4) / 4 * 4 - _cx;
			while (num-- > 0)
			{
				Write(' ');
			}
			return;
		}
		}
		_nextTerminal.SetChar(_cx, _cy, character, _foregroundColor, _backgroundColor);
		_cx++;
		if (_cx == WindowWidth)
		{
			if (_cy == WindowHeight - 1)
			{
				_cx = WindowWidth - 1;
				return;
			}
			_cx = 0;
			_cy++;
		}
	}

	public void WriteLine(string s)
	{
		Write(s);
		Write('\n');
	}

	public void Clear()
	{
		for (int i = 0; i < WindowWidth; i++)
		{
			for (int j = 0; j < WindowHeight; j++)
			{
				_nextTerminal.SetChar(i, j, ' ', _foregroundColor, _backgroundColor);
			}
		}
		_cx = (_cy = 0);
	}

	public void Write(string s)
	{
		foreach (char character in s)
		{
			Write(character);
		}
	}

	public string ReadLine(int maxLength)
	{
		Refresh();
		Term.CursorVisible = true;
		if (Term.Backend == null && maxLength > WindowWidth - CursorX)
		{
			int cursorX = CursorX;
			int cursorY = CursorY;
			string text = Console.ReadLine();
			Term.CursorVisible = false;
			SetCursorPosition(cursorX, cursorY);
			WriteLine(text);
			Refresh();
			return text;
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		do
		{
			ConsoleKeyInfo consoleKeyInfo = ReadKey();
			if (char.IsLetter(consoleKeyInfo.KeyChar) || char.IsDigit(consoleKeyInfo.KeyChar) || consoleKeyInfo.KeyChar == ' ' || consoleKeyInfo.KeyChar == '-' || consoleKeyInfo.KeyChar == '\'')
			{
				if (CursorX < WindowWidth - 1 && stringBuilder.Length < maxLength)
				{
					char c = consoleKeyInfo.KeyChar;
					if ((consoleKeyInfo.Modifiers & ConsoleModifiers.Shift) != 0)
					{
						c = char.ToUpper(c);
					}
					Write(c);
					stringBuilder.Append(c);
				}
				else
				{
					Term.Beep();
				}
			}
			else if (consoleKeyInfo.Key == ConsoleKey.Enter)
			{
				flag = true;
			}
			else if (consoleKeyInfo.Key == ConsoleKey.Backspace)
			{
				if (stringBuilder.Length > 0)
				{
					SetCursorPosition(CursorX - 1, CursorY);
					Write(' ');
					SetCursorPosition(CursorX - 1, CursorY);
					stringBuilder.Remove(stringBuilder.Length - 1, 1);
				}
				else
				{
					Term.Beep();
				}
			}
		}
		while (!flag);
		Term.CursorVisible = false;
		Write('\n');
		Refresh();
		return stringBuilder.ToString().Trim();
	}

	public void Refresh()
	{
		if (Term.Backend != null)
		{
			Term.Present(_nextTerminal, _cx, _cy);
			_fullRefresh = false;
			return;
		}
		if (_lastWindowWidth != Console.WindowWidth || _lastWindowHeight != Console.WindowHeight)
		{
			try
			{
				Console.WindowWidth = _nextTerminal.WindowWidth;
				Console.WindowHeight = _nextTerminal.WindowHeight;
			}
			catch (Exception)
			{
			}
			Term.ForegroundColor = _foregroundColor;
			Term.BackgroundColor = _backgroundColor;
			Console.Clear();
			_lastWindowWidth = Console.WindowWidth;
			_lastWindowHeight = Console.WindowHeight;
			_fullRefresh = true;
			Grog.SetConsoleWindowTitle();
		}
		StringBuilder stringBuilder = null;
		int num = Math.Min(_lastWindowHeight, _nextTerminal.WindowHeight);
		int num2 = Math.Min(_lastWindowWidth, _nextTerminal.WindowWidth);
		for (int i = 0; i < num; i++)
		{
			int num3 = ((!_fullRefresh) ? _nextTerminal.GetLeftDeltaX(i) : 0);
			int num4 = (_fullRefresh ? (num2 - 1) : Math.Min(num2, _nextTerminal.GetRightDeltaX(i)));
			if (!_fullRefresh && num3 > num4)
			{
				continue;
			}
			int num5;
			for (num5 = num3; num5 <= num4; num5++)
			{
				char character = _nextTerminal.GetCharacter(num5, i);
				ConsoleColor foregroundColor = _nextTerminal.GetForegroundColor(num5, i);
				ConsoleColor backgroundColor = _nextTerminal.GetBackgroundColor(num5, i);
				while (num5 <= num4 && !_fullRefresh && character == _currentTerminal.GetCharacter(num5, i) && foregroundColor.Equals(_currentTerminal.GetForegroundColor(num5, i)) && backgroundColor.Equals(_currentTerminal.GetBackgroundColor(num5, i)))
				{
					num5++;
					if (num5 <= num4)
					{
						character = _nextTerminal.GetCharacter(num5, i);
						foregroundColor = _nextTerminal.GetForegroundColor(num5, i);
						backgroundColor = _nextTerminal.GetBackgroundColor(num5, i);
					}
				}
				if (num5 > num4)
				{
					break;
				}
				int num6 = num5;
				if (stringBuilder == null)
				{
					stringBuilder = new StringBuilder();
				}
				else
				{
					stringBuilder.Clear();
				}
				while (num5 <= num4 && foregroundColor.Equals(_nextTerminal.GetForegroundColor(num5, i)) && backgroundColor.Equals(_nextTerminal.GetBackgroundColor(num5, i)))
				{
					stringBuilder.Append(character);
					_currentTerminal.SetChar(num5, i, character, foregroundColor, backgroundColor);
					num5++;
					if (num5 <= num4)
					{
						character = _nextTerminal.GetCharacter(num5, i);
					}
				}
				if (Console.CursorLeft != num6 || Console.CursorTop != i)
				{
					Console.CursorLeft = num6;
					Console.CursorTop = i;
				}
				if (!foregroundColor.Equals(Console.ForegroundColor) || !backgroundColor.Equals(Console.BackgroundColor))
				{
					Console.ForegroundColor = foregroundColor;
					Console.BackgroundColor = backgroundColor;
				}
				if (stringBuilder.Length > 0)
				{
					if (stringBuilder.Length == 1)
					{
						Console.Write(stringBuilder[0]);
					}
					else
					{
						Console.Write(stringBuilder.ToString());
					}
				}
				num5--;
			}
			if (WindowWidth > _nextTerminal.WindowWidth && Console.CursorTop < WindowHeight)
			{
				Console.SetCursorPosition(0, Console.CursorTop + 1);
			}
			_nextTerminal.ClearDeltaX(i);
		}
		_cx = Math.Min(WindowWidth - 1, _cx);
		_cy = Math.Min(WindowHeight - 1, _cy);
		Console.SetCursorPosition(_cx, _cy);
		_fullRefresh = false;
	}

	public ConsoleKeyInfo ReadKey(bool showReadKey = false, bool showCursor = true)
	{
		Refresh();
		if (showCursor)
		{
			Term.CursorVisible = true;
		}
		ConsoleKeyInfo result = Term.ReadKey();
		if (showCursor)
		{
			Term.CursorVisible = false;
		}
		if (showReadKey)
		{
			Write(result.KeyChar, refresh: true);
			Refresh();
		}
		return result;
	}

	public void WriteFormatted(string text, int leftColumn = 0)
	{
		string[] array = text.Split(new char[1] { ' ' });
		foreach (string text2 in array)
		{
			while (CursorX < leftColumn)
			{
				Write(' ');
			}
			if (CursorX + text2.Length < WindowWidth - 1 || CursorX == leftColumn)
			{
				Write(text2);
			}
			else
			{
				Write('\n');
				while (CursorX < leftColumn)
				{
					Write(' ');
				}
				Write(text2);
			}
			if (CursorX + text2.Length < WindowWidth - 1)
			{
				Write(' ');
				continue;
			}
			ClearToEndOfLine();
			Write('\n');
		}
	}

	public void ClearToEndOfLine()
	{
		int cursorX = CursorX;
		int cursorY = CursorY;
		while (CursorX < WindowWidth - 1)
		{
			Write(' ');
		}
		Write(' ');
		SetCursorPosition(cursorX, cursorY);
	}

	public void InvertColors()
	{
		ConsoleColor backgroundColor = _backgroundColor;
		ConsoleColor foregroundColor = _foregroundColor;
		_foregroundColor = backgroundColor;
		_backgroundColor = foregroundColor;
		_isInverted = !_isInverted;
	}

	public void PauseInversion()
	{
		if (_inversionPaused)
		{
			Game.RaiseError("Trying to pause inversion repeatedly!");
			return;
		}
		_inversionPaused = true;
		_wasInverted = _isInverted;
		if (_isInverted)
		{
			InvertColors();
		}
	}

	public void UnpauseInversion()
	{
		if (!_inversionPaused)
		{
			Game.RaiseError("Trying to unpause non-paused inversion (could be a follow-up error to trying to pause an already paused inversion).");
			return;
		}
		_inversionPaused = false;
		if (_wasInverted)
		{
			InvertColors();
		}
	}

	public void RedrawScreen()
	{
		_fullRefresh = true;
		Refresh();
	}

	public void Center(string text)
	{
		SetCursorPosition(0, CursorY);
		ClearToEndOfLine();
		int x = Math.Max(0, (WindowWidth - text.Length) / 2);
		SetCursorPosition(x, CursorY);
		WriteLine(text);
	}

	public int GetCharacterAt(int x, int y)
	{
		return _nextTerminal.GetCharacter(x, y);
	}
}
