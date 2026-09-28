using System;
using Grog.Dressings.Beings;
using Grog.Kernel.GCurses;

namespace Grog.Kernel.FileAccess;

[Serializable]
public class HighscoreEntry
{
	public long Score { get; }

	public string ShortDeathText { get; }

	public string LongDeathText { get; private set; }

	public HighscoreEntry(Player grog)
	{
		ShortDeathText = grog.Name + ", the " + grog.Gender + " " + grog.Type + (grog.IsChampion ? ", champion of Ta'ker'na, " : "") + " (L:" + grog.Level + ", AC:" + grog.ArmorClass + ", HP:" + grog.MaxHitPoints + ", ST:" + grog.Strength + ", DX:" + grog.Dexterity + ", CN:" + grog.Constitution + ", M:" + grog.Moves + ") ";
		if (!grog.IsAlive)
		{
			ShortDeathText += "was ";
			if (grog.Strength < 1)
			{
				ShortDeathText += "turned into a wight ";
			}
			else if (grog.Constitution < 1)
			{
				ShortDeathText += "turned into a vampire ";
			}
			else
			{
				ShortDeathText += "killed ";
			}
			if (Game.Instance.DeathCause != null)
			{
				ShortDeathText = ShortDeathText + Game.Instance.DeathCause + " ";
			}
			if (grog.HasGainedImmortality)
			{
				ShortDeathText += "(after gaining immortality) ";
			}
			string shortDeathText = ShortDeathText;
			int level = Game.Instance.DungeonMaster.CurrentDungeonLevel.Level;
			ShortDeathText = shortDeathText + "on level " + level + " of the dungeon of despair.";
		}
		else if (grog.HasLeftDungeon)
		{
			if (grog.HasGainedImmortality)
			{
				ShortDeathText += "left the dungeon of despair after ascending to immortality to become a legendary hero and ruler of all surrounding lands!";
			}
			else
			{
				ShortDeathText += "fled the dungeon of despair.";
			}
		}
		else
		{
			string shortDeathText2 = ShortDeathText;
			int level = Game.Instance.DungeonMaster.CurrentDungeonLevel.Level;
			ShortDeathText = shortDeathText2 + "committed suicide on level " + level + " of the dungeon of despair.";
		}
		Score = Game.Instance.HighscoreManager.GetScoreFor(grog) - grog.InitialScore;
	}

	public void AskForFate()
	{
		Curses.Instance.Write("Would you like to describe your fate [y/N]? ");
		char keyChar;
		do
		{
			keyChar = Curses.Instance.ReadKey().KeyChar;
		}
		while (keyChar != 'y' && keyChar != 'n' && keyChar != ' ' && keyChar != '\n' && keyChar != 'N');
		string text = "";
		if (keyChar == 'y')
		{
			Curses.Instance.WriteLine("\n\nPlease describe your fate in less than 140 characters: ");
			text = Curses.Instance.ReadLine(140)?.Trim() ?? "";
			if (text.Length > 140)
			{
				text = text.Substring(0, 137) + "...";
			}
		}
		LongDeathText = text;
	}
}
