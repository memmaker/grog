using System;
using System.Collections.Generic;
using Grog.Dressings.Beings;
using Grog.Dressings.Items;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Monsters;
using Grog.Kernel.FileAccess;
using Grog.Kernel.GCurses;
using Grog.Kernel.Options;
using Grog.Systems.Ghosts;
using Grog.Systems.Revenge;
using Grog.Systems.SpecialRooms;
using Grog.Systems.Traps;

namespace Grog.Kernel;

[Serializable]
public class Game
{
	[Serializable]
	private class SaveGameSummary
	{
		public int Version { get; }

		public string Summary { get; }

		public SaveGameSummary(int version, string summary)
		{
			Version = version;
			Summary = summary;
		}
	}

	public const int AutoSaveFileIndex = 42;

	private const int SaveGameVersion = 1;

	private const string SaveGameSummaryFilePrefix = "grog";

	private const string SaveGameFilePrefix = "grog";

	private const string SaveGameSummarySuffix = ".sgs";

	private const string SaveGameSuffix = ".sg";

	private static Game _singleton = null;

	private static readonly string[] Number = new string[11]
	{
		"zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine",
		"ten"
	};

	[NonSerialized]
	private Random _rng;

	private int _uid;

	private MonsterPool _monsterPool;

	[NonSerialized]
	private GrogDefaults _defaults;

	[NonSerialized]
	private GrogOptions _options;

	private static HashSet<string> _processedCrashes = new HashSet<string>();

	private Random RNG
	{
		get
		{
			if (_rng == null)
			{
				_rng = new Random();
			}
			return _rng;
		}
	}

	public static Game Instance
	{
		get
		{
			if (_singleton == null)
			{
				_singleton = new Game();
			}
			return _singleton;
		}
	}

	public long NextUID => _uid++;

	public GrogDefaults Defaults
	{
		get
		{
			if (_defaults == null)
			{
				_defaults = FileManager.ReadObject("grog_v1.dft", () => new GrogDefaults());
			}
			return _defaults;
		}
	}

	public GrogOptions Options
	{
		get
		{
			if (_options == null)
			{
				_options = GrogOptions.Load();
			}
			return _options;
		}
	}

	public DungeonMaster DungeonMaster { get; }

	public MonsterPool MonsterPool
	{
		get
		{
			if (_monsterPool == null)
			{
				_monsterPool = new MonsterPool();
			}
			return _monsterPool;
		}
	}

	public Player Grog { get; set; }

	public ItemPool ItemPool { get; private set; }

	public HighscoreManager HighscoreManager { get; }

	public RevengeSystem RevengeSystem { get; }

	public TrapSystem TrapSystem { get; }

	public string DeathCause { get; set; }

	public bool IsFirstTurnWithAutomaticAction { get; set; }

	public GhostSystem Ghosts { get; }

	public SpecialRoomSystem SpecialRoomSystem { get; }

	public static void Reset()
	{
		_singleton = null;
	}

	private Game()
	{
		HighscoreManager = new HighscoreManager();
		DungeonMaster = new DungeonMaster();
		ItemPool = new ItemPool();
		Ghosts = new GhostSystem();
		RevengeSystem = new RevengeSystem();
		TrapSystem = new TrapSystem();
		SpecialRoomSystem = new SpecialRoomSystem();
	}

	public int Random(int maximum)
	{
		return RNG.Next() % maximum;
	}

	public bool Probability(int probability)
	{
		return Random(100) < probability;
	}

	public void Win()
	{
		if (Grog.HasGainedImmortality)
		{
			DungeonMaster.CurrentDungeonLevel.Message(" sits on the mighty throne. It feels cold and empty...");
			return;
		}
		if (!MonsterPool.HasBeenKilled(MonsterPool.MonsterType.Lich))
		{
			DungeonMaster.CurrentDungeonLevel.Message(" sits on the mighty throne.", more: true);
			DungeonMaster.CurrentDungeonLevel.Message("Nothing seems to happen...", more: true);
			DungeonMaster.CurrentDungeonLevel.Message("Then a horrible whisper worms in " + Grog.Name + "'s mind: \"Aspirations ye have? Immortality in eternal death ye will find!\"", more: true);
			return;
		}
		DungeonMaster.CurrentDungeonLevel.Message(" sits on the mighty throne.", more: true);
		DungeonMaster.CurrentDungeonLevel.Message("Nothing seems to happen...", more: true);
		DungeonMaster.CurrentDungeonLevel.Message("Suddenly " + Grog.Name + " is flooded by an icy energy!", more: true);
		DungeonMaster.CurrentDungeonLevel.Message("A voice booms: \"Leave behind mortality, child!\"", more: true);
		DungeonMaster.CurrentDungeonLevel.Message("\"Age from now on will be meaningless to you - but be careful lest ye die for other reasons!\"", more: true);
		DungeonMaster.CurrentDungeonLevel.Message("Then " + Grog.Name + " is alone again... very alone...", more: true);
		DungeonMaster.CurrentDungeonLevel.Message(" now is an unaging immortal (+6 St, +20 Cn, +100 H).", more: true);
		Grog.Strength += 6;
		Grog.Constitution += 20;
		Grog.MaxHitPoints += 100;
		Grog.HitPoints += 100;
		Grog.HasGainedImmortality = true;
	}

	public int Roll(int numberOfDice, int dieSides, int dieBonus = 0)
	{
		return new Roll(numberOfDice, dieSides, dieBonus).GetDieResult();
	}

	public static bool Save(int index = -1)
	{
		if (index == 42)
		{
			try
			{
				SaveGame(42);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
		Curses.Instance.Clear();
		SaveGameSummary[] array = FindSavedGames();
		Curses.Instance.WriteLine("\n\n\tSelect a save game slot:\n");
		for (int i = 0; i < 10; i++)
		{
			Curses.Instance.WriteLine("\t\t" + i + ": " + (array[i]?.Summary ?? "-"));
		}
		Curses.Instance.Write("\n\tWhich one [0-9, any other key to abort]? ");
		char keyChar = Curses.Instance.ReadKey().KeyChar;
		if (keyChar >= '0' && keyChar <= '9')
		{
			index = keyChar - 48;
			if (array[index] != null)
			{
				Curses.Instance.Write("\n\n\tAre you sure you want to overwrite slot " + index + " [Y/n]? ");
				char keyChar2 = Curses.Instance.ReadKey().KeyChar;
				if (keyChar2 != 'Y' && keyChar2 != 'y' && keyChar2 != ' ')
				{
					Instance.DungeonMaster.CurrentDungeonLevel.ClearMessages();
					Instance.DungeonMaster.CurrentDungeonLevel.Message("Saving aborted.");
					return false;
				}
			}
			Curses.Instance.Clear();
			Curses.Instance.Write("Saving the game...");
			Curses.Instance.Refresh();
			SaveGame(index);
			return true;
		}
		return false;
	}

	public static bool Load(int index = -1)
	{
		if (index == 42)
		{
			LoadGame(42);
			return _singleton != null;
		}
		Curses.Instance.Clear();
		SaveGameSummary[] array = FindSavedGames();
		Curses.Instance.WriteLine("\n\n\tSelect a saved game to load:\n");
		for (int i = 0; i < 10; i++)
		{
			Curses.Instance.WriteLine("\t\t" + i + ": " + (array[i]?.Summary ?? "-"));
		}
		Curses.Instance.Write("\n\tWhich one [0-9, any other key to abort]? ");
		char keyChar = Curses.Instance.ReadKey().KeyChar;
		if (keyChar >= '0' && keyChar <= '9')
		{
			index = keyChar - 48;
			if (array[index] == null)
			{
				return false;
			}
			Curses.Instance.Clear();
			Curses.Instance.Write("Loading the game...");
			Curses.Instance.Refresh();
			LoadGame(index);
			return _singleton != null;
		}
		return false;
	}

	private static void LoadGame(int index)
	{
		_singleton = FileManager.ReadObject(GetSaveGameFileName(index), () => (Game)null);
		if (_singleton == null)
		{
			RaiseError("Failed to load game at index " + index + ".");
		}
		else
		{
			_singleton.MakeCopiesUniqueAgain();
		}
		DeleteSaveGame(index);
	}

	public static void DeleteSaveGame(int index)
	{
		FileManager.DeleteFile(GetSaveGameFileName(index));
		FileManager.DeleteFile(GetSaveGameSummaryFileName(index));
	}

	private void MakeCopiesUniqueAgain()
	{
		DungeonMaster.MakeCopiesUniqueAgain();
	}

	private static SaveGameSummary[] FindSavedGames()
	{
		SaveGameSummary[] array = new SaveGameSummary[10];
		for (int i = 0; i < 10; i++)
		{
			SaveGameSummary saveGameSummary = FileManager.ReadObject(GetSaveGameSummaryFileName(i), () => (SaveGameSummary)null);
			array[i] = ((saveGameSummary != null && saveGameSummary.Version == 1) ? saveGameSummary : null);
		}
		return array;
	}

	public static bool HasSavedGames()
	{
		SaveGameSummary[] array = FindSavedGames();
		for (int i = 0; i < 10; i++)
		{
			if (array[i] != null)
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasAutoSaveGame()
	{
		SaveGameSummary saveGameSummary = FileManager.ReadObject(GetSaveGameSummaryFileName(42), () => (SaveGameSummary)null);
		if (saveGameSummary != null)
		{
			return saveGameSummary.Version == 1;
		}
		return false;
	}

	private static void SaveGame(int index)
	{
		string[] array = new string[13]
		{
			Instance.Grog.Name,
			", the ",
			Instance.Grog.Gender,
			" ",
			Instance.Grog.Type,
			Instance.Grog.IsChampion ? ", champion of Ta'ker'na, " : "",
			" (DL: ",
			null,
			null,
			null,
			null,
			null,
			null
		};
		int level = Instance.DungeonMaster.CurrentDungeonLevel.Level;
		array[7] = level.ToString();
		array[8] = ", L:";
		array[9] = Instance.Grog.Level.ToString();
		array[10] = ", M:";
		array[11] = Instance.Grog.Moves.ToString();
		array[12] = ") ";
		SaveGameSummary data = new SaveGameSummary(1, string.Concat(array));
		FileManager.WriteObject(GetSaveGameSummaryFileName(index), data);
		FileManager.WriteObject(GetSaveGameFileName(index), Instance);
	}

	private static string GetSaveGameFileName(int index)
	{
		return "grog" + index + "_v" + 1 + ".sg";
	}

	private static string GetSaveGameSummaryFileName(int index)
	{
		return "grog" + index + "_v" + 1 + ".sgs";
	}

	public static void RaiseError(string errorMessage, bool logOnly = false)
	{
		if (!logOnly)
		{
			Curses.Instance.Clear();
			Curses.Instance.InvertColors();
			Curses.Instance.Center("*** Internal Error (" + global::Grog.Grog.InitializationStatus + " - " + DateTime.UtcNow.ToString() + ")! ***");
			Curses.Instance.InvertColors();
			Curses.Instance.WriteLine("\n" + errorMessage);
		}
		if (!_processedCrashes.Contains(errorMessage))
		{
			try
			{
				int hashCode = errorMessage.GetHashCode();
				string text = hashCode.ToString();
				string text2 = ((hashCode < 0) ? ("-" + Math.Abs(hashCode).ToString().PadLeft(15, '0')) : text.PadLeft(16, '0'));
				FileManager.WriteText("gcrash" + text2 + ".txt", errorMessage);
			}
			catch (Exception ex)
			{
				if (!logOnly)
				{
					Curses.Instance.WriteLine("\nSaving an error dump failed: " + ex.Message);
				}
			}
			_processedCrashes.Add(errorMessage);
		}
		if (!logOnly)
		{
			Curses.Instance.InvertColors();
			Curses.Instance.Write("[Press '!' to continue]");
			Curses.Instance.InvertColors();
			while (Curses.Instance.ReadKey(showReadKey: false, showCursor: false).KeyChar != '!')
			{
			}
			Curses.Instance.Clear();
		}
	}

	public string GetSpelledOutNumber(long number)
	{
		if (number < 0 || number >= 11)
		{
			return number.ToString();
		}
		return Number[number];
	}

	public string a(string word)
	{
		if ("aeiou".IndexOf(word[0]) != -1)
		{
			return "an " + word;
		}
		return "a " + word;
	}

	public string GetRelativeDirectionText(int fx, int fy, int tx, int ty)
	{
		int num = tx - fx;
		int num2 = ty - fy;
		switch (num)
		{
		case -1:
			switch (num2)
			{
			case -1:
				return "north-west";
			case 0:
				return "west";
			case 1:
				return "south-east";
			}
			break;
		case 0:
			switch (num2)
			{
			case -1:
				return "north";
			case 1:
				return "south";
			}
			break;
		case 1:
			switch (num2)
			{
			case -1:
				return "north-east";
			case 0:
				return "east";
			case 1:
				return "south-east";
			}
			break;
		}
		return "the ground";
	}
}
