using System;
using System.Text;

namespace Grog.Kernel.GCurses;

public class CursesTerminal
{
	public readonly int WindowWidth;

	public readonly int WindowHeight;

	private readonly char[][] _c;

	private readonly ConsoleColor[][] _fc;

	private readonly ConsoleColor[][] _bc;

	private readonly int[] _ldx;

	private readonly int[] _rdx;

	public CursesTerminal(int windowWidth, int windowHeight)
	{
		WindowWidth = windowWidth;
		WindowHeight = windowHeight;
		_c = new char[WindowWidth][];
		_fc = new ConsoleColor[WindowWidth][];
		_bc = new ConsoleColor[WindowWidth][];
		_ldx = new int[WindowHeight];
		_rdx = new int[WindowHeight];
		for (int i = 0; i < WindowHeight; i++)
		{
			ClearDeltaX(i);
		}
		for (int j = 0; j < WindowWidth; j++)
		{
			_c[j] = new char[WindowHeight];
			_fc[j] = new ConsoleColor[WindowHeight];
			_bc[j] = new ConsoleColor[WindowHeight];
		}
	}

	public void ClearDeltaX(int y)
	{
		if (y < WindowHeight)
		{
			_ldx[y] = int.MaxValue;
			_rdx[y] = 0;
		}
	}

	public void SetChar(int x, int y, char c, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
	{
		if (x < WindowWidth && y < WindowHeight)
		{
			_c[x][y] = c;
			_fc[x][y] = foregroundColor;
			_bc[x][y] = backgroundColor;
			_ldx[y] = Math.Min(x, _ldx[y]);
			_rdx[y] = Math.Max(x, _rdx[y]);
		}
	}

	public char GetCharacter(int x, int y)
	{
		if (x >= WindowWidth || y >= WindowHeight)
		{
			return ' ';
		}
		return _c[x][y];
	}

	public ConsoleColor GetForegroundColor(int x, int y)
	{
		if (x >= WindowWidth || y >= WindowHeight)
		{
			return ConsoleColor.Black;
		}
		return _fc[x][y];
	}

	public ConsoleColor GetBackgroundColor(int x, int y)
	{
		if (x >= WindowWidth || y >= WindowHeight)
		{
			return ConsoleColor.Black;
		}
		return _bc[x][y];
	}

	public int GetLeftDeltaX(int y)
	{
		if (y >= WindowHeight)
		{
			return 0;
		}
		return _ldx[y];
	}

	public int GetRightDeltaX(int y)
	{
		if (y >= WindowHeight)
		{
			return 0;
		}
		return _rdx[y];
	}

	public string GetLine(int y)
	{
		if (y >= WindowHeight)
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < WindowWidth; i++)
		{
			stringBuilder.Append(_c[i][y]);
		}
		return stringBuilder.ToString();
	}

	public void ReplaceColorSystem(ConsoleColor oldForeground, ConsoleColor oldBackground, ConsoleColor newForeground, ConsoleColor newBackground)
	{
		for (int i = 0; i < WindowWidth; i++)
		{
			for (int j = 0; j < WindowHeight; j++)
			{
				_fc[i][j] = (_fc[i][j].Equals(oldForeground) ? newForeground : newBackground);
				_bc[i][j] = (_bc[i][j].Equals(oldBackground) ? newBackground : newForeground);
			}
		}
	}
}
