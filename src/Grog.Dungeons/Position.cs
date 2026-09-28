using System;

namespace Grog.Dungeons;

[Serializable]
public struct Position
{
	public static readonly Position Undefined = new Position(-1, -1);

	public int X { get; }

	public int Y { get; }

	public bool IsUndefined
	{
		get
		{
			if (X >= 0)
			{
				return Y < 0;
			}
			return true;
		}
	}

	public Position(int x, int y)
	{
		X = x;
		Y = y;
	}

	public override string ToString()
	{
		return base.ToString() + ": X=" + X + ", Y=" + Y;
	}

	public override int GetHashCode()
	{
		return Y * 127507 + X;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is Position position)
		{
			if (X == position.X)
			{
				return Y == position.Y;
			}
			return false;
		}
		return false;
	}
}
