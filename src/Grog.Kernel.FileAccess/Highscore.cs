using System;
using System.Collections.Generic;
using Grog.Dressings.Beings;
using Grog.Kernel.GCurses;

namespace Grog.Kernel.FileAccess;

[Serializable]
public class Highscore
{
	private const int MaxHighScoreEntry = 1000;

	private List<HighscoreEntry> entries = new List<HighscoreEntry>();

	public HighscoreEntry AddEntry(Player grog)
	{
		HighscoreEntry highscoreEntry = new HighscoreEntry(grog);
		Curses.Instance.Clear();
		if (highscoreEntry.Score < 0)
		{
			Curses.Instance.WriteLine("You actually managed to score negatively (" + highscoreEntry.Score + " points)!");
			Curses.Instance.WriteLine("The gods mock your feeble attempts and laugh at you for all your afterlife!");
			Curses.Instance.WriteLine("You are not allowed to enter the holy halls of Hi'Skorr!");
			Curses.Instance.InvertColors();
			Curses.Instance.Write("---more---");
			Curses.Instance.InvertColors();
			Curses.Instance.ReadKey(showReadKey: false, showCursor: false);
			Curses.Instance.Clear();
			return null;
		}
		if (highscoreEntry.Score == 0L)
		{
			Curses.Instance.WriteLine("You failed to score any points.");
			Curses.Instance.WriteLine("The gods are not impressed by your feeble attempts.");
			Curses.Instance.WriteLine("You are not allowed to enter the holy halls of Hi'Skorr!");
			Curses.Instance.InvertColors();
			Curses.Instance.Write("---more---");
			Curses.Instance.InvertColors();
			Curses.Instance.ReadKey(showReadKey: false, showCursor: false);
			Curses.Instance.Clear();
			return null;
		}
		int num = 0;
		using (List<HighscoreEntry>.Enumerator enumerator = entries.GetEnumerator())
		{
			while (enumerator.MoveNext() && enumerator.Current.Score >= highscoreEntry.Score)
			{
				num++;
			}
		}
		if (num < 1000)
		{
			highscoreEntry.AskForFate();
			entries.Insert(num, highscoreEntry);
			Game.Instance.HighscoreManager.SaveHighscore(this);
			return highscoreEntry;
		}
		return null;
	}

	public void DisplayHighscore(HighscoreEntry entry)
	{
		Curses.Instance.Clear();
		int num = ((entry == null) ? (-1) : entries.FindIndex((HighscoreEntry e) => e.Equals(entry)));
		if (num == -1)
		{
			Curses.Instance.WriteFormatted("You failed to enter the ranks of the brave... " + entries[999].Score + " points were required but you scored only " + entry.Score + "...");
			Curses.Instance.WriteLine("\n");
			DisplayHighscoreHeader("Top 8");
			DisplayHighScoreEntries(0, 7, -1);
		}
		else
		{
			Curses.Instance.WriteFormatted("You scored rank " + (num + 1) + " of " + entries.Count + " in the highscore!");
			Curses.Instance.WriteLine("\n");
			int num2 = Math.Max(num - 2, 0);
			int num3 = Math.Min(num + 2, entries.Count - 1);
			if (num2 <= 2)
			{
				DisplayHighscoreHeader("Top " + Math.Min(entries.Count, 8));
				DisplayHighScoreEntries(0, 7, num);
			}
			else if (num3 - num2 + 1 < 5)
			{
				DisplayHighscoreHeader("Famous heroes");
				DisplayHighScoreEntries(0, 2, num);
				int num4 = Math.Max(3, entries.Count - 5);
				if (num4 > 3)
				{
					Curses.Instance.WriteLine("...");
				}
				DisplayHighScoreEntries(num4, entries.Count - 1, num);
			}
			else
			{
				DisplayHighscoreHeader("Famous heroes");
				DisplayHighScoreEntries(0, 2, num);
				if (num2 > 3)
				{
					Curses.Instance.WriteLine("...");
				}
				DisplayHighScoreEntries(num2, num3, num);
			}
		}
		Curses.Instance.InvertColors();
		Curses.Instance.Write("\n[Press any key to EXIT]");
		Curses.Instance.InvertColors();
		Curses.Instance.ClearToEndOfLine();
		Curses.Instance.ReadKey(showReadKey: false, showCursor: false);
	}

	private void DisplayHighScoreEntries(int from, int to, int index)
	{
		to = Math.Min(to, entries.Count - 1);
		int num = 0;
		for (int i = from; i <= to; i++)
		{
			num = Math.Max(entries[i].Score.ToString().Length, num);
		}
		int length = entries.Count.ToString().Length;
		for (int j = from; j <= to; j++)
		{
			if (index == j)
			{
				Curses.Instance.InvertColors();
			}
			Curses.Instance.Write((j + 1).ToString().PadLeft(length) + "  ");
			Curses.Instance.Write(entries[j].Score.ToString().PadLeft(num) + "  ");
			Curses.Instance.WriteFormatted(entries[j].ShortDeathText, length + num + 4);
			if (entries[j].LongDeathText.Trim().Length > 0)
			{
				Curses.Instance.Write(" ");
				Curses.Instance.WriteFormatted(entries[j].LongDeathText, length + num + 4);
			}
			Curses.Instance.ClearToEndOfLine();
			if (index == j)
			{
				Curses.Instance.InvertColors();
			}
			if (Curses.Instance.CursorX > 0)
			{
				Curses.Instance.Write("\n");
			}
		}
	}

	private void DisplayHighscoreHeader(string top)
	{
		top = " " + top + " ";
		int num = (Curses.Instance.WindowWidth - top.Length) / 2;
		for (int i = 0; i < num; i++)
		{
			Curses.Instance.Write("=");
		}
		Curses.Instance.Write(top);
		for (int j = 0; j < num; j++)
		{
			Curses.Instance.Write("=");
		}
		if (Curses.Instance.CursorX != 0)
		{
			Curses.Instance.Write("\n");
		}
	}
}
