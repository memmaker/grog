using System;
using Grog.Dressings.Beings;
using Grog.Dressings.Items;

namespace Grog.Kernel.FileAccess;

[Serializable]
public class HighscoreManager
{
	private const string KilledPlayerFileName = "grog_v1.kpc";

	private const Environment.SpecialFolder HighscoreFileFolder = Environment.SpecialFolder.CommonApplicationData;

	public Highscore LoadHighscore()
	{
		FileManager.LockFileAccess();
		try
		{
			return FileManager.ReadObject("grog_v1.hsc", () => new Highscore(), Environment.SpecialFolder.CommonApplicationData);
		}
		finally
		{
			FileManager.UnlockFileAccess();
		}
	}

	public int GetFailedPlayerCount()
	{
		return FileManager.ReadObject("grog_v1.kpc", () => 0);
	}

	public void IncreaseFailedPlayerCount()
	{
		FileManager.WriteObject("grog_v1.kpc", FileManager.ReadObject("grog_v1.kpc", () => 0) + 1);
	}

	public void SaveHighscore(Highscore highscore)
	{
		FileManager.LockFileAccess();
		try
		{
			FileManager.WriteObject("grog_v1.hsc", highscore, Environment.SpecialFolder.CommonApplicationData);
		}
		catch (Exception ex)
		{
			Game.RaiseError("Failed to write the highscore file.\n" + ex.StackTrace);
		}
		finally
		{
			FileManager.UnlockFileAccess();
		}
	}

	public long GetScoreFor(Player grog)
	{
		long num = 0L;
		int num2 = (grog.Level - 1) * 100;
		num += num2;
		num += Math.Max(grog.Experience - num2, 0);
		num += grog.Gold * 20;
		num += (Game.Instance.DungeonMaster.DeepestLevelReached - 1) * Game.Instance.DungeonMaster.DeepestLevelReached * 50;
		foreach (Item item in grog.Inventory.GetInventory())
		{
			int num3 = (item.IsEquipped ? item.Value : Math.Max(1, item.Value / 5));
			if (item.IsIdentified)
			{
				num3 *= 2;
			}
			num += num3;
		}
		if (grog.NumberOfPrayersUttered < grog.NumberOfFreePrayers && grog.NumberOfFreePrayers > grog.Level)
		{
			num += Math.Max(0, (grog.NumberOfFreePrayers - grog.Level - grog.NumberOfPrayersUttered) * 500);
		}
		if (grog.IsChampion)
		{
			num += 50000;
			if (grog.HasLeftDungeon && grog.HasGainedImmortality && grog.Moves < 15000)
			{
				num += (grog.Moves - 15000) * 5;
			}
		}
		if (grog.HasGainedImmortality)
		{
			num += 100000;
			if (grog.HasLeftDungeon)
			{
				if (grog.Moves < 15000)
				{
					num += (grog.Moves - 15000) * 50;
				}
				num *= 2;
			}
		}
		return num;
	}
}
