using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using Grog.Dressings;
using Grog.Dressings.Beings;
using Grog.Dressings.Features;
using Grog.Dressings.Items;
using Grog.Dungeons;
using Grog.Kernel;
using Grog.Kernel.FileAccess;
using Grog.Kernel.GCurses;
using Grog.Kernel.Interactions;

namespace Grog;

internal static class Grog
{
	private class MonsterDistanceComparer : IComparer<Being>
	{
		private readonly Dictionary<Being, int> _monsterDistances;

		public MonsterDistanceComparer(Dictionary<Being, int> monsterDistances)
		{
			_monsterDistances = monsterDistances;
		}

		public int Compare(Being x, Being y)
		{
			int num = _monsterDistances[x];
			int num2 = _monsterDistances[y];
			if (num != num2)
			{
				return num2 - num;
			}
			if (!x.Name.Equals(y.Name))
			{
				return string.Compare(x.Name, y.Name, StringComparison.InvariantCulture);
			}
			return Math.Sign(x.UID - y.UID);
		}
	}

	private const string ShortVersion = "1.0.2";

	private const string Release = "Release 3";

	private static string Version = "Version 1.0.2";

	private const int PreferredWindowWidth = 80;

	private const int PreferredWindowHeight = 26;

	public static string InitializationStatus = "";

	private static string Title;

	private static readonly string Copyright1 = "(C) Copyright 2018-" + Math.Max(2022, DateTime.Now.Year) + " by Dr.-Ing. Thomas Biskup.";

	private const string Copyright2 = "All rights reserved all over the world.";

	private const string WizardNameMessage = "<-- That be a wizard!";

	private const string WizardGenderMessage = "<-- Yup, that's right!!";

	private const string WizardTypeMessage = "<-- So mote it be!!!";

	private static readonly int MaximalWizardMessageLength = Math.Max("<-- That be a wizard!".Length, Math.Max("<-- So mote it be!!!".Length, "<-- Yup, that's right!!".Length));

	public static void Main(string[] args)
	{
		if (Term.Backend == null) System.Console.CancelKeyPress += GrogCancelKeyHandler;
		string text = string.Empty;
		try
		{
			using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Grog.version.txt");
			using StreamReader streamReader = new StreamReader(stream);
			text = streamReader.ReadToEnd().Trim();
		}
		catch (Exception)
		{
		}
		Version += " (Release 3";
		if (!string.IsNullOrEmpty(text))
		{
			Version = Version + " - Build " + text;
		}
		Title = "Grog " + Version + ")";
		try
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
			InitializationStatus += "T";
		}
		catch (Exception)
		{
			InitializationStatus += "t";
		}
		try
		{
			Term.SetWindowSize(80, 26);
			InitializationStatus += "S";
		}
		catch (Exception)
		{
			InitializationStatus += "s";
		}
		if (Term.WindowWidth < Curses.MinWindowWidth || Term.WindowHeight < Curses.MinWindowHeight)
		{
			Term.WriteLine("Grog requires a minimum " + Curses.MinWindowWidth + "x" + Curses.MinWindowHeight + " console window size to run (current: " + Term.WindowWidth + "x" + Term.WindowHeight + ").\n\n");
			return;
		}
		bool flag = false;
		try
		{
			Term.BufferHeight = (Term.BufferWidth = 0);
			flag = true;
			InitializationStatus += "R";
		}
		catch (Exception)
		{
			InitializationStatus += "r";
		}
		if (!flag)
		{
			try
			{
				Term.BufferHeight = 26;
				Term.BufferWidth = 80;
				InitializationStatus += "P";
			}
			catch (Exception)
			{
				InitializationStatus += "p";
			}
		}
		SetConsoleWindowTitle();
		bool flag2 = true;
		do
		{
			Game.Reset();
		}
		while (Play());
	}

	public static void SetConsoleWindowTitle()
	{
		try
		{
			Term.Title = "Grog 1.0.2 [" + Term.WindowWidth + "x" + Term.WindowHeight + "|" + Term.BufferWidth + "x" + Term.WindowHeight + ":" + InitializationStatus + (int)Environment.OSVersion.Platform + "] - " + Copyright1;
		}
		catch (Exception)
		{
		}
	}

	private static void GrogCancelKeyHandler(object sender, ConsoleCancelEventArgs e)
	{
		e.Cancel = true;
	}

	static int _rvipSavedMoves = -1;
	static object _rvipSavedLevel;

	private static bool Play()
	{
		Game.Instance.Options.InitializeGameBasedOnOptions();
		DisplayStartHeader();
		if (Game.HasAutoSaveGame())
		{
			Curses.Instance.InvertColors();
			Curses.Instance.WriteLine("*** ATTENTION! ***");
			Curses.Instance.InvertColors();
			if (Term.Backend != null) // RVIP: in the browser the autosave is the normal way back into a running game
			{
				Curses.Instance.WriteLine("Grog has found your autosaved game and will continue it now.");
			}
			else
			{
			Curses.Instance.WriteLine("Grog has found an auto-save file of a previous game. It seems that your last");
			Curses.Instance.WriteLine("game crashed for some reason. Grog now will reload the auto-save. Either");
			Curses.Instance.WriteLine("continue the game, save it or quit it. Then you can proceed normally.");
			}
			Curses.Instance.InvertColors();
			Curses.Instance.WriteLine("---more---");
			Curses.Instance.InvertColors();
			Curses.Instance.ReadKey(showReadKey: false, showCursor: false);
			if (Game.Load(42))
			{
				Game.Instance.DungeonMaster.CurrentDungeonLevel.Render();
				goto IL_05cc;
			}
			DisplayStartHeader();
			Curses.Instance.InvertColors();
			Curses.Instance.WriteLine("*** ATTENTION! ***");
			Curses.Instance.InvertColors();
			Curses.Instance.WriteLine("I am sorry. I was unable to load the auto-save file. The game is lost.");
			Curses.Instance.WriteLine("You will have to create a new character or load another saved game.");
			Curses.Instance.InvertColors();
			Curses.Instance.WriteLine("---more---");
			Curses.Instance.InvertColors();
			DisplayStartHeader();
			Game.DeleteSaveGame(42);
		}
		int cursorY = Curses.Instance.CursorY;
		if (Game.HasSavedGames())
		{
			Curses.Instance.Write("Do you want to load a saved game [y/n]? ");
			if (Curses.Instance.ReadKey().KeyChar == 'y')
			{
				if (Game.Load())
				{
					Game.Instance.DungeonMaster.CurrentDungeonLevel.Render();
					goto IL_05cc;
				}
				DisplayStartHeader();
			}
			Curses.Instance.SetCursorPosition(0, cursorY);
			Curses.Instance.ClearToEndOfLine();
		}
		string text;
		int cursorY2;
		while (true)
		{
			Curses.Instance.Write("What be thy name [" + Game.Instance.Defaults.Name + "]? ");
			cursorY2 = Curses.Instance.CursorY;
			text = Curses.Instance.ReadLine(15)?.Trim().ToLower();
			if (!"kalmius".Equals(text) && !"ta'ker'na".Equals(text) && !"hi'skorr".Equals(text))
			{
				break;
			}
			Curses.Instance.SetCursorPosition(0, cursorY);
			Curses.Instance.ClearToEndOfLine();
			Curses.Instance.Write("The forces of Fate prevent you from choosing that name! ");
			Curses.Instance.Write("---more---");
			Curses.Instance.ReadKey(showReadKey: false, showCursor: false);
			Curses.Instance.SetCursorPosition(0, cursorY);
			Curses.Instance.ClearToEndOfLine();
		}
		if (string.IsNullOrEmpty(text))
		{
			text = Game.Instance.Defaults.Name;
		}
		else
		{
			text = char.ToUpper(text[0]) + text.Substring(1);
			Game.Instance.Defaults.Name = text;
		}
		if ("brannalbin".Equals(text, StringComparison.InvariantCultureIgnoreCase))
		{
			Curses.Instance.SetCursorPosition(Curses.Instance.WindowWidth - MaximalWizardMessageLength - 1, cursorY2);
			Curses.Instance.WriteLine("<-- That be a wizard!");
		}
		Curses.Instance.Write("What be thy gender [" + Game.Instance.Defaults.Gender + "]? ");
		cursorY2 = Curses.Instance.CursorY;
		string text2 = Curses.Instance.ReadLine(10)?.Trim().ToLower();
		if (string.IsNullOrEmpty(text2))
		{
			text2 = Game.Instance.Defaults.Gender;
		}
		else
		{
			Game.Instance.Defaults.Gender = text2;
		}
		if ("brannalbin".Equals(text, StringComparison.InvariantCultureIgnoreCase) && "male".Equals(text2, StringComparison.InvariantCultureIgnoreCase))
		{
			Curses.Instance.SetCursorPosition(Curses.Instance.WindowWidth - MaximalWizardMessageLength - 1, cursorY2);
			Curses.Instance.WriteLine("<-- Yup, that's right!!");
		}
		Curses.Instance.Write("What be thy type [" + Game.Instance.Defaults.Type + "]? ");
		cursorY2 = Curses.Instance.CursorY;
		string text3 = Curses.Instance.ReadLine(20)?.Trim().ToLower();
		if (string.IsNullOrEmpty(text3))
		{
			text3 = Game.Instance.Defaults.Type;
		}
		else
		{
			Game.Instance.Defaults.Type = text3;
		}
		if ("brannalbin".Equals(text, StringComparison.InvariantCultureIgnoreCase) && "male".Equals(text2, StringComparison.InvariantCultureIgnoreCase) && "wizard".Equals(text3, StringComparison.InvariantCultureIgnoreCase))
		{
			Curses.Instance.SetCursorPosition(Curses.Instance.WindowWidth - MaximalWizardMessageLength - 1, cursorY2);
			Curses.Instance.WriteLine("<-- So mote it be!!!");
		}
		Game.Instance.Defaults.Save();
		Game.Instance.ItemPool.Initialize();
		Curses.Instance.WriteLine("\nMorituri te salutant, " + text + ", the " + text2 + " " + text3 + "!\nPrepare thyself to explore the dungeon of despair!");
		if (Game.Instance.Ghosts.Count > 20)
		{
			Curses.Instance.WriteLine("This place reeks of death, suffering and unholy damnation!");
		}
		Curses.Instance.InvertColors();
		Curses.Instance.Write("---more---");
		Curses.Instance.InvertColors();
		Curses.Instance.ReadKey(showReadKey: false, showCursor: false);
		Player player = (Game.Instance.Grog = new Player(text, text2, text3));
		Player player3 = player;
		Game.Instance.DungeonMaster.SetUpDungeonLevel(1, Tile.StairUp);
		Curses.Instance.Clear();
		Game.Instance.DungeonMaster.CurrentDungeonLevel.Message(" descends into the depths of the dungeon of despair. Press 'h' to see the help texts.");
		goto IL_05cc;
		IL_05cc:
		bool isRunning = true;
		bool isSaved = false;
		player3 = Game.Instance.Grog;
		if (Game.Instance.DungeonMaster.CurrentDungeonLevel.GetFeatureAt(player3.X, player3.Y)?.WhenEntering != null)
		{
			Game.Instance.DungeonMaster.CurrentDungeonLevel.GetFeatureAt(player3.X, player3.Y).WhenEntering?.Interact(Game.Instance.DungeonMaster.CurrentDungeonLevel, player3);
		}
		do
		{
			try
			{
				DungeonLevel currentDungeonLevel = Game.Instance.DungeonMaster.CurrentDungeonLevel;
				if (currentDungeonLevel == null)
				{
					Game.RaiseError("Current dungeon level is undefined. Trying to restore the last one created.");
					if (!Game.Instance.DungeonMaster.RestoreWorkableDungeonLevel())
					{
						Game.RaiseError("Failed to restore a workable game state. Exiting...");
						return false;
					}
				}
				if (!currentDungeonLevel.IsValid(player3.X, player3.Y) && !Game.Instance.DungeonMaster.RestoreGrogPosition())
				{
					Game.RaiseError("Failed to restore a workable grog position. Exiting...");
					return false;
				}
				Tile tileAt = currentDungeonLevel.GetTileAt(player3.X, player3.Y);
				if (tileAt.Description != null)
				{
					currentDungeonLevel.Message(tileAt.Description);
				}
				if (currentDungeonLevel.HasItems(player3.X, player3.Y) && (player3.Moved || player3.Moves == 0))
				{
					if (player3.IsBlind != 0)
					{
						currentDungeonLevel.Message("Something seems to be lying on the ground.");
					}
					else if (currentDungeonLevel.IsItemAt(player3.X, player3.Y))
					{
						currentDungeonLevel.Message(currentDungeonLevel.GetItemAt(player3.X, player3.Y).A() + " is lying here.");
					}
					else
					{
						List<Item> itemsAt = currentDungeonLevel.GetItemsAt(player3.X, player3.Y);
						if (itemsAt.Count == 2)
						{
							currentDungeonLevel.Message(itemsAt[0].A() + " and " + itemsAt[1].a() + " are lying here.");
						}
						else
						{
							currentDungeonLevel.Message(itemsAt.Count + " items are lying here.");
						}
					}
				}
				currentDungeonLevel.Render();
				int moves = player3.Moves;
				if (player3.IsBlind == 0)
				{
					Curses.Instance.SetCursorPosition(player3.X, player3.Y + 2);
					if (!player3.IsInvisible)
					{
						Curses.Instance.Write('@');
						Curses.Instance.SetCursorPosition(player3.X, player3.Y + 2);
					}
				}
				else
				{
					Curses.Instance.SetCursorPosition(0, 0);
				}
				player3.Moved = false;
				if (player3.IsSleeping > 0 || player3.IsParalyzed > 0)
				{
					currentDungeonLevel.Message((player3.IsParalyzed > 0) ? " can't move!" : "Zzzzzzzz!");
					currentDungeonLevel.More();
					player3.ResetAutomaticAction();
					player3.Moves++;
				}
				else if (player3.AutomaticAction != null)
				{
					player3.Feelings = 0;
					if (Curses.Instance.IsKeyAvailable)
					{
						player3.ResetAutomaticAction();
						Curses.Instance.ReadKey();
					}
					else if (!player3.AutomaticAction.Execute())
					{
						player3.ResetAutomaticAction();
					}
					Game.Instance.IsFirstTurnWithAutomaticAction = false;
				}
				else
				{
					int num = ((player3.IsHastened <= 0) ? 1 : 2);
					for (int i = 0; i < num; i++)
					{
						if (!player3.IsAlive)
						{
							break;
						}
						if (player3.HasLeftDungeon)
						{
							break;
						}
						if (player3.IsStunned == 0 || Game.Instance.Random(player3.Constitution + 10) < player3.Constitution)
						{
							ConsoleKeyInfo consoleKeyInfo;
							try
							{
								// RVIP: autosave (slot 42, the game's own crash save) at the prompt with no keys pending; the loop end deletes it
								if (Term.Backend != null && player3.AutomaticAction == null && (player3.Moves != _rvipSavedMoves || currentDungeonLevel != _rvipSavedLevel) && !Term.KeyAvailable) { _rvipSavedMoves = player3.Moves; _rvipSavedLevel = currentDungeonLevel; Game.Save(42); }
								AutoExploreAction.Note(currentDungeonLevel, player3);
								Term.AtCmd = true;
								try { consoleKeyInfo = Curses.Instance.ReadKey(); } finally { Term.AtCmd = false; }
								currentDungeonLevel.ClearMessages();
								currentDungeonLevel.ResetAutoMore();
								currentDungeonLevel.PrintMapAt(player3.X, player3.Y);
								player3.Feelings = 0;
							}
							catch (IOException)
							{
								return false;
							}
							if (consoleKeyInfo.KeyChar == 'g' || consoleKeyInfo.KeyChar == '<' || consoleKeyInfo.KeyChar == '>')
							{
								if (AutoExploreAction.Start(player3, currentDungeonLevel, consoleKeyInfo.KeyChar == 'g' ? null : (consoleKeyInfo.KeyChar == '<' ? Tile.StairUp : Tile.StairDown)))
								{
									Game.Instance.IsFirstTurnWithAutomaticAction = true;
								}
							}
							else if ((consoleKeyInfo.Key == ConsoleKey.LeftArrow || consoleKeyInfo.Key == ConsoleKey.A) && (consoleKeyInfo.Modifiers & ConsoleModifiers.Shift) != 0)
							{
								currentDungeonLevel.MovePlayerWest();
								player3.AutomaticAction = new AutomaticMovementAction(player3, currentDungeonLevel, Direction.West);
								Game.Instance.IsFirstTurnWithAutomaticAction = true;
							}
							else if ((consoleKeyInfo.Key == ConsoleKey.RightArrow || consoleKeyInfo.Key == ConsoleKey.D) && ((consoleKeyInfo.Modifiers & ConsoleModifiers.Shift) != 0 || consoleKeyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift)))
							{
								currentDungeonLevel.MovePlayerEast();
								player3.AutomaticAction = new AutomaticMovementAction(player3, currentDungeonLevel, Direction.East);
								Game.Instance.IsFirstTurnWithAutomaticAction = true;
							}
							else if ((consoleKeyInfo.Key == ConsoleKey.UpArrow || consoleKeyInfo.Key == ConsoleKey.W) && ((consoleKeyInfo.Modifiers & ConsoleModifiers.Shift) != 0 || consoleKeyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift)))
							{
								currentDungeonLevel.MovePlayerNorth();
								player3.AutomaticAction = new AutomaticMovementAction(player3, currentDungeonLevel, Direction.MinDirection);
								Game.Instance.IsFirstTurnWithAutomaticAction = true;
							}
							else if ((consoleKeyInfo.Key == ConsoleKey.DownArrow || consoleKeyInfo.Key == ConsoleKey.S) && ((consoleKeyInfo.Modifiers & ConsoleModifiers.Shift) != 0 || consoleKeyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift)))
							{
								currentDungeonLevel.MovePlayerSouth();
								player3.AutomaticAction = new AutomaticMovementAction(player3, currentDungeonLevel, Direction.South);
								Game.Instance.IsFirstTurnWithAutomaticAction = true;
							}
							else if (consoleKeyInfo.Key == ConsoleKey.R && ((consoleKeyInfo.Modifiers & ConsoleModifiers.Control) != 0 || consoleKeyInfo.Modifiers.HasFlag(ConsoleModifiers.Control)))
							{
								Curses.Instance.RedrawScreen();
							}
							else if (consoleKeyInfo.Key == ConsoleKey.X && ((consoleKeyInfo.Modifiers & ConsoleModifiers.Control) != 0 || consoleKeyInfo.Modifiers.HasFlag(ConsoleModifiers.Control)) && player3.HasAccessToCheatMode)
							{
								CheatMode.Cheat(currentDungeonLevel, player3);
							}
							else if (consoleKeyInfo.KeyChar == '.')
							{
								if (player3.HitPoints < player3.MaxHitPoints)
								{
									player3.AutomaticAction = new AutomaticWaitUntilHealedAction(player3, currentDungeonLevel);
									Game.Instance.IsFirstTurnWithAutomaticAction = true;
								}
								else
								{
									Wait(player3, currentDungeonLevel, "(for one turn only)");
								}
							}
							else if (consoleKeyInfo.KeyChar == 'm')
							{
								DisplayListOfEnemies(currentDungeonLevel, player3);
							}
							else if (consoleKeyInfo.Key == ConsoleKey.LeftArrow || consoleKeyInfo.Key == ConsoleKey.A)
							{
								currentDungeonLevel.MovePlayerWest();
							}
							else if (consoleKeyInfo.Key == ConsoleKey.RightArrow || consoleKeyInfo.Key == ConsoleKey.D)
							{
								currentDungeonLevel.MovePlayerEast();
							}
							else if (consoleKeyInfo.Key == ConsoleKey.UpArrow || consoleKeyInfo.Key == ConsoleKey.W)
							{
								currentDungeonLevel.MovePlayerNorth();
							}
							else if (consoleKeyInfo.Key == ConsoleKey.DownArrow || consoleKeyInfo.Key == ConsoleKey.S)
							{
								currentDungeonLevel.MovePlayerSouth();
							}
							else if (consoleKeyInfo.Key == ConsoleKey.Enter)
							{
								CommandMenu.Show(); // RVIP: Enter = command menu (interact is r)
								currentDungeonLevel.Render();
							}
							else if (consoleKeyInfo.Key == ConsoleKey.R)
							{
								currentDungeonLevel.InteractWithEnvironment();
							}
							else if (consoleKeyInfo.Key == ConsoleKey.T || consoleKeyInfo.Key == ConsoleKey.X)
							{
								currentDungeonLevel.ThrowItem();
							}
							else if (consoleKeyInfo.Key == ConsoleKey.P)
							{
								Pray(currentDungeonLevel);
							}
							else if (consoleKeyInfo.Key == ConsoleKey.H || consoleKeyInfo.Key == ConsoleKey.Help || consoleKeyInfo.KeyChar == '?')
							{
								DisplayHelp(currentDungeonLevel);
							}
							else if (consoleKeyInfo.Key == ConsoleKey.Escape)
							{
								List<Interaction> list = new List<Interaction>();
								list.Add(new Interaction('h', "Display help", currentDungeonLevel, (DungeonLevel dl, Player g, int x, int y) =>
								{
									DisplayHelp(dl);
									return false;
								}));
								list.Add(new Interaction('o', "Options", currentDungeonLevel, (DungeonLevel dl, Player g, int x, int y) =>
								{
									Game.Instance.Options.Configure(dl);
									return false;
								}));
								list.Add(new Interaction('q', "Save & quit", currentDungeonLevel, (DungeonLevel dl, Player g, int x, int y) =>
								{
									(isRunning, isSaved) = CheckQuitAndSaveTheGame(dl);
									return true;
								}));
								list.Add(new Interaction('x', "Quit without saving", currentDungeonLevel, (DungeonLevel dl, Player g, int x, int y) =>
								{
									isRunning = CheckQuitTheGame(dl);
									return true;
								}));
								currentDungeonLevel.InitiateComplexInteraction("What do you want to do?", list);
							}
							else if (consoleKeyInfo.KeyChar == ',' || consoleKeyInfo.KeyChar == 'c')
							{
								currentDungeonLevel.PickUpItems();
							}
							else if (consoleKeyInfo.Key == ConsoleKey.I || consoleKeyInfo.Key == ConsoleKey.E)
							{
								currentDungeonLevel.ManageInventory();
							}
							else if (consoleKeyInfo.Key == ConsoleKey.U || consoleKeyInfo.Key == ConsoleKey.F)
							{
								currentDungeonLevel.UseItem();
							}
							else if (consoleKeyInfo.Key == ConsoleKey.Z)
							{
								currentDungeonLevel.ZapWand();
							}
							else if (consoleKeyInfo.Key == ConsoleKey.O)
							{
								Game.Instance.Options.Configure(currentDungeonLevel);
							}
							else if (consoleKeyInfo.Key == ConsoleKey.Q && ((consoleKeyInfo.Modifiers & ConsoleModifiers.Control) != 0 || consoleKeyInfo.Modifiers.HasFlag(ConsoleModifiers.Control)))
							{
								isRunning = CheckQuitTheGame(currentDungeonLevel);
							}
							else if (consoleKeyInfo.Key == ConsoleKey.Q && ((consoleKeyInfo.Modifiers & ConsoleModifiers.Shift) != 0 || consoleKeyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift)))
							{
								(isRunning, isSaved) = CheckQuitAndSaveTheGame(currentDungeonLevel);
							}
							else if (consoleKeyInfo.Key == ConsoleKey.Spacebar)
							{
								Wait(player3, currentDungeonLevel);
							}
							else if (consoleKeyInfo.Key == ConsoleKey.V)
							{
								currentDungeonLevel.Message(Title + ". " + Copyright1 + " All rights reserved all over the world.", more: true);
							}
						}
						else
						{
							currentDungeonLevel.Message(" is too stunned to act!");
							player3.Moves++;
						}
					}
				}
				if (player3.Moves != moves && player3.IsAlive)
				{
					currentDungeonLevel.ReactToTurnSpentBy(player3);
					int num2 = Math.Max(5, 15 - Math.Max(player3.Dexterity - 10, 0));
					currentDungeonLevel.ExecuteBeingActions(player3.Moves % num2 == 0);
				}
			}
			catch (Exception ex2)
			{
				Game.RaiseError("The game crashed with an exception. Error message:\n" + ex2.Message + "\n\nStack trace:\n" + ex2.StackTrace);
			}
		}
		while (isRunning && player3.IsAlive && !player3.HasLeftDungeon);
		Game.DeleteSaveGame(42);
		if (isSaved)
		{
			Curses.Instance.Clear();
			Curses.Instance.WriteLine("Fare thee well, brave adventurer - we shall meet again!");
			Curses.Instance.InvertColors();
			Curses.Instance.WriteLine("---more---");
			Curses.Instance.InvertColors();
			Curses.Instance.ReadKey(showReadKey: false, showCursor: false);
		}
		else
		{
			Game.Instance.DungeonMaster.CurrentDungeonLevel.Render();
			Game.Instance.DungeonMaster.CurrentDungeonLevel.More("[Press '/' to proceed]", '/');
			Game.Instance.DungeonMaster.CurrentDungeonLevel.DisplayInventoryAfterDeath();
			Curses.Instance.Clear();
			if (!player3.IsAlive)
			{
				if (player3.Strength < 1)
				{
					Curses.Instance.WriteLine("\nYou were turned into a wight...\n");
				}
				else if (player3.Constitution < 1)
				{
					Curses.Instance.WriteLine("\nYou were turned into a vampire...\n");
				}
				else
				{
					Curses.Instance.WriteLine("\nYou died...\n"); Term.Sound("death"); //RVIP 6b
				}
				Game.Instance.HighscoreManager.IncreaseFailedPlayerCount();
			}
			else if (player3.HasGainedImmortality)
			{
				Curses.Instance.WriteLine("Congratulations! You ascended to immortality!");
			}
			else
			{
				if (!isRunning)
				{
					Curses.Instance.WriteLine("\n" + player3.Name + " is forever lost in the depths of the dungeon of despair...");
				}
				else
				{
					Curses.Instance.WriteLine("You flee like a coward, weeping in the night due to your horrible memories of\nfailure until the very end of your miserable existence...");
				}
				Game.Instance.HighscoreManager.IncreaseFailedPlayerCount();
			}
			{ //RVIP 12: graveyard/leaderboard report (death, win = left the dungeon immortal, quit/fled)
				string ev = !player3.IsAlive ? "death" : (player3.HasLeftDungeon && player3.HasGainedImmortality) ? "win" : "quit";
				string killer = ev == "death" ? (Player.RvipKiller ?? Game.Instance.DeathCause ?? (player3.SatiationLevel == global::Grog.Dressings.Items.Implementations.Food.SatiationLevel.Starved ? "starvation" : null)) : null; // IsAlive is false at Satiation <= 0 even when the "dies of starvation" branch never ran
				if (killer != null) { if (killer.StartsWith("by ")) killer = killer.Substring(3); foreach (string a in new[] { "a ", "an ", "the " }) if (killer.StartsWith(a)) { killer = killer.Substring(a.Length); break; } }
				long score = 0; try { score = Game.Instance.HighscoreManager.GetScoreFor(player3) - player3.InitialScore; } catch { }
				Term.Beacon(ev, string.IsNullOrEmpty(player3.ChristenedName) ? null : player3.ChristenedName, killer, Game.Instance.DungeonMaster.CurrentDungeonLevel.Level, score, player3.Moves, player3.Level);
				Player.RvipKiller = null;
			}
			Game.Instance.Ghosts.StoreActiveGhostsForLater();
			Game.Instance.RevengeSystem.StoreActiveRevengeMonstersForLater();
			if (player3.DidCheat)
			{
				Curses.Instance.Clear();
				Curses.Instance.WriteLine("You cheated and thus are barred from entering the holy halls of Hi'Skorr.");
				Curses.Instance.InvertColors();
				Curses.Instance.Write("---more---");
				Curses.Instance.InvertColors();
				Curses.Instance.ReadKey(showReadKey: false, showCursor: false);
			}
			else
			{
				Highscore highscore = Game.Instance.HighscoreManager.LoadHighscore();
				HighscoreEntry highscoreEntry = highscore.AddEntry(player3);
				if (highscoreEntry != null)
				{
					highscore.DisplayHighscore(highscoreEntry);
				}
			}
		}
		Curses.Instance.Clear();
		Curses.Instance.Refresh();
		Curses.Instance.Write("Do you want to play again [y/N]? ");
		char keyChar = Curses.Instance.ReadKey(showReadKey: true).KeyChar;
		if (keyChar != 'Y')
		{
			return keyChar == 'y';
		}
		return true;
	}

	private static void Wait(Player grog, DungeonLevel dungeonLevel, string extraMessage = "")
	{
		grog.Moves++;
		dungeonLevel.Message("Waiting" + (string.IsNullOrEmpty(extraMessage) ? "" : (" " + extraMessage)) + ".");
		dungeonLevel.InteractWithSurroundingMapElements(dungeonLevel, grog);
	}

	private static void DisplayHelp(DungeonLevel dungeonLevel)
	{
		dungeonLevel.DisplayStringList(Constants.HelpText);
	}

	private static (bool, bool) CheckQuitAndSaveTheGame(DungeonLevel dungeonLevel)
	{
		if (dungeonLevel.Sure("save and then quit the game"))
		{
			if (Game.Save())
			{
				return (false, true);
			}
			dungeonLevel.Render();
		}
		return (true, false);
	}

	private static bool CheckQuitTheGame(DungeonLevel dungeonLevel)
	{
		return !dungeonLevel.Sure("quit the game (thus ending it forever)");
	}

	private static void Pray(DungeonLevel dungeonLevel)
	{
		if (dungeonLevel.Grog.Moves - dungeonLevel.Grog.LastPrayerTurn < 300 - dungeonLevel.Level * 10 && !(dungeonLevel.GetFeatureAt(dungeonLevel.Grog.X, dungeonLevel.Grog.Y) is AltarFeature))
		{
			dungeonLevel.Message(" is ignored by Ta'ker'na...", more: true);
			dungeonLevel.Message(" probably only should call upon Ta'ker'na in times of need and woe!", more: true);
			return;
		}
		dungeonLevel.Grog.LastPrayerTurn = dungeonLevel.Grog.Moves;
		if (AltarFeature.PrayForHelp(dungeonLevel, dungeonLevel.Grog, dungeonLevel.Grog.X, dungeonLevel.Grog.Y, dungeonLevel.GetFeatureAt(dungeonLevel.Grog.X, dungeonLevel.Grog.Y) is AltarFeature).Item1)
		{
			dungeonLevel.Grog.Moves++;
		}
	}

	private static void DisplayStartHeader()
	{
		Curses.Instance.Clear();
		Curses.Instance.SetCursorPosition(0, 0);
		Curses.Instance.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
		Curses.Instance.WriteLine("  " + Title + "\n");
		Curses.Instance.WriteLine("  " + Copyright1);
		Curses.Instance.WriteLine("  All rights reserved all over the world.");
		Curses.Instance.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=");
		Curses.Instance.WriteLine("``Grog\u00b4\u00b4 - derogatory title for a seasoned warrior, derived from ``grognard\u00b4\u00b4.");
		Curses.Instance.WriteLine("=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=\n");
		Curses.Instance.WriteLine("Welcome, adventurer!\n");
		Curses.Instance.WriteLine("Ahead of you looms the Dungeon of Despair. Countless adventurers lost");
		Curses.Instance.WriteLine("their lives trying to find the fabled 'Throne of Immortality'.\n");
		Curses.Instance.WriteLine("Will you perish, too, or win the ultimate prize?\n");
	}

	private static void DisplayListOfEnemies(DungeonLevel dungeonLevel, Player grog)
	{
		List<Being> list = dungeonLevel.GetAllBeingsVisibleTo(grog);
		string item = "|  " + list.Count + " " + ((list.Count != 1) ? "enemies" : "enemy") + ":";
		Dictionary<Being, int> dictionary = new Dictionary<Being, int>();
		if (list.Count == 0)
		{
			dungeonLevel.Message(" doesn't see any enemy monsters.");
			return;
		}
		foreach (Being item2 in list)
		{
			dictionary[item2] = Math.Abs(grog.X - item2.X) + Math.Abs(grog.Y - item2.Y);
		}
		list.Sort(new MonsterDistanceComparer(dictionary));
		int num = Curses.Instance.WindowHeight - 2;
		if (list.Count > num)
		{
			item = "|  The closest " + num + " out of " + list.Count + " enemies:";
			list = list.GetRange(0, num);
		}
		List<string> list2 = new List<string> { item };
		foreach (Being item3 in list)
		{
			list2.Add("|  " + Game.Instance.MonsterPool.GetMonsterDescription(item3));
		}
		list2.Add("Use the movement keys to navigate, any other key to abort: ");
		int num2 = 0;
		foreach (string item4 in list2)
		{
			num2 = Math.Max(num2, item4.Length);
		}
		int num3 = -1;
		while (true)
		{
			dungeonLevel.Render();
			if (num3 == -1 && list.Count > 1)
			{
				Curses.Instance.InvertColors();
				foreach (Being item5 in list)
				{
					Curses.Instance.SetCursorPosition(item5.X, item5.Y + 2);
					Curses.Instance.Write(item5.Character(dungeonLevel, item5.X, item5.Y));
				}
				Curses.Instance.InvertColors();
				for (int i = 0; i < list2.Count; i++)
				{
					if (i == 0 || i == list2.Count - 1)
					{
						Curses.Instance.InvertColors();
					}
					Curses.Instance.SetCursorPosition(Curses.Instance.WindowWidth - num2 - 1, i);
					Curses.Instance.Write(list2[i]);
					Curses.Instance.ClearToEndOfLine();
					if (i == 0 || i == list2.Count - 1)
					{
						Curses.Instance.InvertColors();
					}
				}
			}
			else
			{
				if (num3 == -1)
				{
					num3 = 0;
				}
				Curses.Instance.InvertColors();
				Curses.Instance.SetCursorPosition(list[num3].X, list[num3].Y + 2);
				Curses.Instance.Write(list[num3].Character(dungeonLevel, list[num3].X, list[num3].Y));
				Curses.Instance.InvertColors();
				Curses.Instance.SetCursorPosition(list[num3].X, list[num3].Y + 2);
				dungeonLevel.Message(((list.Count == 1) ? "Enemy: " : ("Enemy #" + (num3 + 1) + ": ")) + list2[num3 + 1] + ((list.Count == 1) ? "." : ""), list.Count == 1, renderMap: false);
				if (list.Count == 1)
				{
					return;
				}
			}
			ConsoleKeyInfo consoleKeyInfo = Curses.Instance.ReadKey();
			dungeonLevel.ClearMessages();
			if (consoleKeyInfo.Key == ConsoleKey.LeftArrow || consoleKeyInfo.Key == ConsoleKey.A)
			{
				num3--;
			}
			else if (consoleKeyInfo.Key == ConsoleKey.RightArrow || consoleKeyInfo.Key == ConsoleKey.D)
			{
				num3++;
			}
			else if (consoleKeyInfo.Key == ConsoleKey.UpArrow || consoleKeyInfo.Key == ConsoleKey.W)
			{
				num3--;
			}
			else
			{
				if (consoleKeyInfo.Key != ConsoleKey.DownArrow && consoleKeyInfo.Key != ConsoleKey.S)
				{
					break;
				}
				num3++;
			}
			if (num3 == -2)
			{
				num3 = list.Count - 1;
			}
			else if (num3 == list.Count)
			{
				num3 = -1;
			}
		}
		dungeonLevel.Render();
	}
}
