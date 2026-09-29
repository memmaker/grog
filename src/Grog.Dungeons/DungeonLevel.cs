using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Grog.Dressings;
using Grog.Dressings.Beings;
using Grog.Dressings.Features;
using Grog.Dressings.Items;
using Grog.Dressings.Items.Implementations.Food;
using Grog.Dressings.Items.Inventory;
using Grog.Dressings.Items.Types;
using Grog.Dressings.MapElements;
using Grog.Dungeons.Generators.Rooms;
using Grog.Kernel;
using Grog.Kernel.GCurses;
using Grog.Kernel.Interactions;
using Grog.Systems.Traps;

namespace Grog.Dungeons;

[Serializable]
public partial class DungeonLevel
{
	public readonly int Level;

	private string _message;

	private const int MaxItemCountPerTile = 26;

	private readonly int[][] _tileMap;

	private readonly Thing[][] _thingMap;

	private readonly Feature[][] _featureMap;

	private readonly List<IRoom> _rooms = new List<IRoom>();

	private readonly IRoom[][] _roomMap;

	private readonly char[][] _memory;

	private readonly List<Item>[][] _items;

	private readonly Dictionary<Position, ITrap> _traps = new Dictionary<Position, ITrap>();

	[NonSerialized]
	private HashSet<Thing> _things;

	[NonSerialized]
	private bool _isAutoMoreActive;

	public int Width { get; }

	public int Height { get; }

	public IEnumerable<IRoom> Rooms => _rooms;

	public Player Grog => Game.Instance.Grog;

	public bool IsAttunedToGrog { get; set; }

	public HashSet<Thing> Things
	{
		get
		{
			if (_things == null)
			{
				_things = new HashSet<Thing>();
				for (int i = 0; i < Width; i++)
				{
					for (int j = 0; j < Height; j++)
					{
						Thing thingAt = GetThingAt(i, j);
						if (thingAt != null)
						{
							_things.Add(thingAt);
						}
					}
				}
			}
			return _things;
		}
	}

	public DungeonLevel(int level)
	{
		Level = level;
		Width = Curses.Instance.WindowWidth;
		Height = Curses.Instance.WindowHeight - 4;
		IsAttunedToGrog = false;
		_tileMap = new int[Width][];
		_featureMap = new Feature[Width][];
		_thingMap = new Thing[Width][];
		_roomMap = new IRoom[Width][];
		_memory = new char[Width][];
		_items = new List<Item>[Width][];
		for (int i = 0; i < Width; i++)
		{
			_tileMap[i] = new int[Height];
			_featureMap[i] = new Feature[Height];
			_thingMap[i] = new Thing[Height];
			_roomMap[i] = new IRoom[Height];
			_memory[i] = new char[Height];
			_items[i] = new List<Item>[Height];
		}
		for (int j = 0; j < Width; j++)
		{
			for (int k = 0; k < Height; k++)
			{
				_memory[j][k] = ' ';
			}
		}
		Game.Instance.DungeonMaster.GetDungeonLevelGenerator(this).Generate(this);
	}

	public void More(string moreMessage = "---more---", char moreKey = ' ')
	{
		Render(more: true, moreMessage, moreKey);
	}

	public void Render(bool more = false, string moreMessage = "---more---", char moreKey = ' ', bool renderMap = true)
	{
		HashSet<IRoom> clearedRooms = new HashSet<IRoom>();
		IRoom roomAt = GetRoomAt(Grog.X, Grog.Y);
		if (renderMap)
		{
			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					PrintMapAt(i, j, clearedRooms, roomAt);
				}
			}
		}
		string text = string.Format("{0}  AC:{1} H:{2}/{3}  St:{4} Dx:{5} Cn:{6}  D:{7} $:{8} M:{9} XP:{10}/{11} Piety:{12}", new object[13]
		{
			Grog.Name, Grog.ArmorClass, Grog.HitPoints, Grog.MaxHitPoints, Grog.Strength, Grog.Dexterity, Grog.Constitution, Level, Grog.Gold, Grog.Moves,
			Grog.Level, Grog.MissingExperienceForNextLevel, Grog.PietyLevel
		});
		text = text.PadRight(Width);
		int num = Grog.HitPoints * 100 / Grog.MaxHitPoints;
		int num2 = Width * num / 100;
		Curses.Instance.SetCursorPosition(0, Curses.Instance.WindowHeight - 2);
		if (num2 > 0)
		{
			if (num2 < text.Length)
			{
				Curses.Instance.InvertColors();
				Curses.Instance.Write(text.Substring(0, num2));
				Curses.Instance.InvertColors();
				Curses.Instance.Write(text.Substring(num2));
			}
			else
			{
				Curses.Instance.InvertColors();
				Curses.Instance.Write(text);
				Curses.Instance.InvertColors();
			}
		}
		string status = GetStatus();
		_status = new[] { text.TrimEnd(), status.Replace("|||", "").TrimEnd() }; // RVIP 5: Status window
		Curses.Instance.SetCursorPosition(0, Curses.Instance.WindowHeight - 1);
		if (status.Length > 0)
		{
			int num3 = status.IndexOf("|||", StringComparison.InvariantCulture);
			if (num3 == -1)
			{
				Curses.Instance.Write(status);
			}
			else
			{
				status = status.Replace("|||", "");
				Curses.Instance.InvertColors();
				Curses.Instance.Write(status.Substring(0, num3));
				Curses.Instance.InvertColors();
				Curses.Instance.Write(status.Substring(num3));
			}
		}
		Curses.Instance.ClearToEndOfLine();
		Term.MainView = true; Term.Clickable = false; // RVIP 5: map screen
		while (true)
		{
			Curses.Instance.SetCursorPosition(0, 0);
			if (_message != null)
			{
				Output(_message, moreMessage);
			}
			int cursorX = Curses.Instance.CursorX;
			int cursorY = Curses.Instance.CursorY;
			Curses.Instance.ClearToEndOfLine();
			if (Curses.Instance.CursorY == 0)
			{
				Curses.Instance.SetCursorPosition(0, 1);
				Curses.Instance.ClearToEndOfLine();
			}
			if (!more || _message == null)
			{
				break;
			}
			if (_isAutoMoreActive || (AutoMore && moreKey == ' ')) // RVIP 3d: no --more-- stops in the web build
			{
				_message = null;
				more = false;
				continue;
			}
			Curses.Instance.SetCursorPosition(cursorX, cursorY);
			Curses.Instance.Write(' ');
			Curses.Instance.InvertColors();
			Output(moreMessage, "", pauseInversion: false);
			Curses.Instance.InvertColors();
			ConsoleKeyInfo consoleKeyInfo = Curses.Instance.ReadKey(showReadKey: false, showCursor: false);
			if (consoleKeyInfo.KeyChar == moreKey)
			{
				_isAutoMoreActive = consoleKeyInfo.Key == ConsoleKey.Enter;
				more = false;
				_message = null;
			}
		}
	}

	private string GetStatus()
	{
		string text = GetLongStatus();
		if (text.Length > Curses.Instance.WindowWidth)
		{
			text = GetShortStatus();
		}
		return text;
	}

	private string GetShortStatus()
	{
		StringBuilder stringBuilder = new StringBuilder();
		AddFeelings(stringBuilder);
		bool didPrintStatus = false;
		if (Grog.IsSick != 0)
		{
			stringBuilder.Append('S');
			didPrintStatus = true;
		}
		if (Grog.IsStunned != 0)
		{
			stringBuilder.Append('s');
			didPrintStatus = true;
		}
		if (Grog.IsConfused != 0)
		{
			stringBuilder.Append('C');
			didPrintStatus = true;
		}
		if (Grog.IsParalyzed != 0)
		{
			stringBuilder.Append('P');
			didPrintStatus = true;
		}
		if (Grog.IsSleeping != 0)
		{
			stringBuilder.Append('Z');
			didPrintStatus = true;
		}
		if (Grog.IsBlind != 0)
		{
			stringBuilder.Append('B');
			didPrintStatus = true;
		}
		if (Grog.IsHastened > 0)
		{
			stringBuilder.Append("H+");
			didPrintStatus = true;
		}
		if (Grog.IsHastened < 0)
		{
			stringBuilder.Append("H-");
			didPrintStatus = true;
		}
		if (Grog.IsDrunk != 0)
		{
			stringBuilder.Append('D');
			didPrintStatus = true;
		}
		if (Grog.IsDeaf != 0)
		{
			stringBuilder.Append('d');
			didPrintStatus = true;
		}
		if (Grog.IsInvisible)
		{
			stringBuilder.Append('I');
			didPrintStatus = true;
		}
		if (Grog.IsLevitating != 0)
		{
			stringBuilder.Append('L');
			didPrintStatus = true;
		}
		if (Grog.IsPoisoned != 0)
		{
			stringBuilder.Append('p');
			didPrintStatus = true;
		}
		if (Grog.IsFrozen != 0)
		{
			stringBuilder.Append('F');
			didPrintStatus = true;
		}
		AddSatiationStatus(stringBuilder, didPrintStatus);
		return stringBuilder.ToString();
	}

	private string GetLongStatus()
	{
		StringBuilder stringBuilder = new StringBuilder();
		AddFeelings(stringBuilder);
		bool didPrintStatus = false;
		if (Grog.IsSick != 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append("Sick");
			didPrintStatus = true;
		}
		if (Grog.IsStunned != 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append("Stunned");
			didPrintStatus = true;
		}
		if (Grog.IsConfused != 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append("Confused");
			didPrintStatus = true;
		}
		if (Grog.IsParalyzed != 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append("Paralyzed");
			didPrintStatus = true;
		}
		if (Grog.IsSleeping != 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append("Sleeping");
			didPrintStatus = true;
		}
		if (Grog.IsBlind != 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append("Blind");
			didPrintStatus = true;
		}
		if (Grog.IsHastened > 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append("Hastened");
			didPrintStatus = true;
		}
		if (Grog.IsHastened < 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append("Slowed");
			didPrintStatus = true;
		}
		if (Grog.IsDrunk != 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append("Drunk");
			didPrintStatus = true;
		}
		if (Grog.IsDeaf != 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append("Deaf");
			didPrintStatus = true;
		}
		if (Grog.IsInvisible)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append("Invisible");
			didPrintStatus = true;
		}
		if (Grog.IsLevitating != 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append("Levitating");
			didPrintStatus = true;
		}
		if (Grog.IsPoisoned != 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append("Poisoned");
			didPrintStatus = true;
		}
		if (Grog.IsFrozen != 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append("Frozen");
			didPrintStatus = true;
		}
		AddSatiationStatus(stringBuilder, didPrintStatus);
		return stringBuilder.ToString();
	}

	private void AddFeelings(StringBuilder status)
	{
		bool flag = false;
		if ((Grog.Feelings & 0x10) != 0)
		{
			if (status.Length > 0)
			{
				status.Append("  ");
			}
			status.Append("Overburdened!");
			flag = true;
		}
		if ((Grog.Feelings & 8) != 0)
		{
			if (status.Length > 0)
			{
				status.Append("  ");
			}
			status.Append("Pitted!");
			flag = true;
		}
		if ((Grog.Feelings & 1) != 0)
		{
			if (status.Length > 0)
			{
				status.Append("  ");
			}
			status.Append("Threatened");
			flag = true;
		}
		if ((Grog.Feelings & 2) != 0)
		{
			if (status.Length > 0)
			{
				status.Append("  ");
			}
			status.Append("Endangered!");
			flag = true;
		}
		if ((Grog.Feelings & 4) != 0)
		{
			if (status.Length > 0)
			{
				status.Append("  ");
			}
			status.Append("Tingling Back");
			flag = true;
		}
		if (flag)
		{
			status.Append(" |||");
		}
	}

	private void AddSatiationStatus(StringBuilder status, bool didPrintStatus)
	{
		SatiationLevel satiationLevel = Grog.SatiationLevel;
		if (satiationLevel != SatiationLevel.Average)
		{
			if (didPrintStatus)
			{
				status.Append(' ');
			}
			switch (satiationLevel)
			{
			case SatiationLevel.Overfed:
				status.Append("Overfed");
				break;
			case SatiationLevel.Satiated:
				status.Append("Satiated");
				break;
			case SatiationLevel.Hungry:
				status.Append("Hungry");
				break;
			case SatiationLevel.VeryHungry:
				status.Append("Hungry!");
				break;
			case SatiationLevel.Starving:
				status.Append("Starving");
				break;
			case SatiationLevel.Starved:
				status.Append("Starved");
				break;
			}
		}
	}

	public void PrintMapAt(int x, int y, HashSet<IRoom> clearedRooms = null, IRoom grogRoom = null)
	{
		if (grogRoom == null)
		{
			grogRoom = GetRoomAt(Grog.X, Grog.Y);
		}
		if (Grog.CanSee(this, x, y) || (IsAttunedToGrog && IsBeingAt(x, y)))
		{
			if (_thingMap[x][y] != null)
			{
				Being being = _thingMap[x][y] as Being;
				if (being == null || IsAttunedToGrog || !being.IsInvisible || Grog.CanSeeInvisible)
				{
					if (being != null && being.IsInverted)
					{
						Curses.Instance.InvertColors();
					}
					Curses.Instance.SetCursorPosition(x, y + 2);
					Curses.Instance.Write(_thingMap[x][y].Character(this, x, y));
					if (being != null && being.IsInverted)
					{
						Curses.Instance.InvertColors();
					}
				}
				return;
			}
			if (IsItemAt(x, y))
			{
				Curses.Instance.SetCursorPosition(x, y + 2);
				Curses.Instance.Write(GetItemAt(x, y).Character);
				if (!IsRoomAt(x, y))
				{
					_memory[x][y] = GetFeatureAt(x, y)?.Character(this, x, y) ?? GetTileAt(x, y).Character(this, x, y);
				}
				return;
			}
			if (IsItemsAt(x, y))
			{
				Curses.Instance.SetCursorPosition(x, y + 2);
				Curses.Instance.Write('&');
				if (!IsRoomAt(x, y))
				{
					_memory[x][y] = GetFeatureAt(x, y)?.Character(this, x, y) ?? GetTileAt(x, y).Character(this, x, y);
				}
				return;
			}
			Feature feature = _featureMap[x][y];
			Tile tile = Tile.GetTile(_tileMap[x][y]);
			char c = feature?.Character(this, x, y) ?? tile.Character(this, x, y);
			IRoom roomAt = GetRoomAt(x, y);
			if (roomAt != null && roomAt.IsWallOfRoom(x, y) && roomAt.IsSpecial && roomAt.IsPartOfRoom(Grog.X, Grog.Y))
			{
				Curses.Instance.InvertColors();
			}
			Curses.Instance.SetCursorPosition(x, y + 2);
			Curses.Instance.Write(c);
			if (roomAt != null && roomAt.IsWallOfRoom(x, y) && roomAt.IsSpecial && roomAt.IsPartOfRoom(Grog.X, Grog.Y))
			{
				Curses.Instance.InvertColors();
			}
			if ((roomAt != null && !roomAt.IsInsideOfRoom(x, y) && roomAt == grogRoom) || (roomAt != null && roomAt.IsInsideOfRoom(x, y) && roomAt == grogRoom && tile != Tile.Floor) || tile == Tile.Door || (feature != null && !(feature is HiddenTrapFeature)) || roomAt == null)
			{
				_memory[x][y] = c;
			}
			if (roomAt == null || !roomAt.IsPartOfRoom(x, y) || roomAt != grogRoom || (clearedRooms != null && clearedRooms.Contains(roomAt)))
			{
				return;
			}
			if (clearedRooms == null)
			{
				clearedRooms = new HashSet<IRoom>();
			}
			clearedRooms.Add(roomAt);
			for (int i = roomAt.BoundingX1 + 1; i < roomAt.BoundingX2; i++)
			{
				for (int j = roomAt.BoundingY1 + 1; j < roomAt.BoundingY2; j++)
				{
					if (Tile.GetTile(_tileMap[i][j]) == Tile.Floor && !HasFeatureAt(i, j))
					{
						_memory[i][j] = ' ';
					}
				}
			}
		}
		else
		{
			Curses.Instance.SetCursorPosition(x, y + 2);
			Curses.Instance.Write(_memory[x][y]);
		}
	}

	private void Output(string message, string moreMessage, bool pauseInversion = true)
	{
		if (pauseInversion)
		{
			Curses.Instance.PauseInversion();
		}
		string[] array = message.Split(new char[1] { ' ' });
		string text = ((!string.IsNullOrEmpty(moreMessage)) ? (" " + moreMessage) : "");
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		int windowWidth = Curses.Instance.WindowWidth;
		int num = Curses.Instance.CursorX;
		int num2 = Curses.Instance.CursorY;
		for (int i = 0; i < array.Length; i++)
		{
			string text2 = array[i];
			string text3 = ((i < array.Length - 1) ? (" " + array[i + 1]) : "");
			if (num2 == 0 && num + stringBuilder2.Length + text2.Length >= windowWidth)
			{
				Curses.Instance.Write(stringBuilder2.ToString());
				if (!pauseInversion)
				{
					Curses.Instance.PauseInversion();
				}
				Curses.Instance.ClearToEndOfLine();
				if (!pauseInversion)
				{
					Curses.Instance.UnpauseInversion();
				}
				stringBuilder2.Clear().Append(text2).Append(' ');
				stringBuilder.Append(text2).Append(' ');
				Curses.Instance.SetCursorPosition(0, 1);
				num = 0;
				num2 = 1;
			}
			else if (Curses.Instance.CursorY == 1 && (num + stringBuilder2.Length + text2.Length + text3.Length >= windowWidth || num + stringBuilder2.Length + text2.Length + text.Length >= windowWidth))
			{
				if (num + stringBuilder2.Length + text2.Length + text.Length < windowWidth)
				{
					stringBuilder2.Append(text2);
					stringBuilder.Append(text2);
					Curses.Instance.Write(stringBuilder2.ToString());
					ProcessMore(moreMessage);
					Curses.Instance.SetCursorPosition(0, 0);
					Curses.Instance.ClearToEndOfLine();
					Curses.Instance.SetCursorPosition(0, 1);
					Curses.Instance.ClearToEndOfLine();
					Curses.Instance.SetCursorPosition(0, 0);
					stringBuilder2.Clear();
					RemoveFromMessage(stringBuilder);
					num = 0;
					num2 = 0;
				}
				else
				{
					Curses.Instance.Write(stringBuilder2.ToString());
					ProcessMore(moreMessage);
					Curses.Instance.SetCursorPosition(0, 0);
					Curses.Instance.ClearToEndOfLine();
					Curses.Instance.SetCursorPosition(0, 1);
					Curses.Instance.ClearToEndOfLine();
					Curses.Instance.SetCursorPosition(0, 0);
					stringBuilder2.Clear().Append(text2).Append(' ');
					stringBuilder.Append(text2).Append(' ');
					num = 0;
					num2 = 0;
				}
			}
			else
			{
				stringBuilder2.Append(text2);
				stringBuilder.Append(text2);
				if (num + stringBuilder2.Length > 0)
				{
					stringBuilder2.Append(' ');
					stringBuilder.Append(' ');
				}
			}
		}
		if (stringBuilder2.Length > 0)
		{
			Curses.Instance.Write(stringBuilder2.ToString());
		}
		if (pauseInversion)
		{
			Curses.Instance.UnpauseInversion();
		}
	}

	private void RemoveFromMessage(StringBuilder printedMessage)
	{
		if (printedMessage.Length > 0)
		{
			if (!_message.StartsWith(printedMessage.ToString()))
			{
				Game.RaiseError("Message buffer mismatch:\n\n_message:\n" + _message.Substring(0, 150) + "\n\nprintedMessage:\n" + printedMessage.ToString().Substring(0, 150) + "\n\n");
			}
			_message = _message.Substring(printedMessage.Length).Trim();
			printedMessage.Clear();
		}
	}

	private void ProcessMore(string moreMessage)
	{
		if (!_isAutoMoreActive && !AutoMore)
		{
			Curses.Instance.InvertColors();
			Curses.Instance.Write(moreMessage);
			Curses.Instance.InvertColors();
			_isAutoMoreActive = Curses.Instance.ReadKey(showReadKey: false, showCursor: false).Key == ConsoleKey.Enter;
		}
	}

	public bool HasItems(int x, int y)
	{
		if (!IsValid(x, y))
		{
			return false;
		}
		if (_items[x][y] != null)
		{
			return _items[x][y].Count > 0;
		}
		return false;
	}

	public void MovePlayerNorth()
	{
		MoveThing(Grog, Grog.X, Grog.Y, Grog.X, Grog.Y - 1);
	}

	public void MoveThing(Thing thing, int fromX, int fromY, int toX, int toY, bool forceMovement = false)
	{
		if (!IsValid(fromX, fromY))
		{
			throw new GrogException("Invalid from coordinates: " + fromX + ", " + fromY + ".");
		}
		if (!IsValid(toX, toY))
		{
			throw new GrogException("Invalid to coordinates: " + toX + ", " + toY + ".");
		}
		if (thing is Being being)
		{
			if (being is Player player)
			{
				if (player.Inventory.MaxItemCount * 3 < player.Inventory.ItemsCarried)
				{
					player.SufferDamage(this, null, player.HitPoints, "by being crushed under excessive equipment");
					return;
				}
				if (player.Inventory.MaxItemCount < player.Inventory.ItemsCarried)
				{
					Message(" is carrying too much to move any further!");
					player.Feelings |= 16;
					player.Moves++;
					return;
				}
			}
			if (!forceMovement && being.IsConfused > 0)
			{
				Message(being, " staggers...");
				being.GetRandomMovementModifier(being, out var xm, out var ym);
				toX = being.X + xm;
				toY = being.Y + ym;
			}
			if (!forceMovement && GetFeatureAt(being.X, being.Y)?.WhenTryingToLeave != null)
			{
				IBeingInteraction whenTryingToLeave = GetFeatureAt(being.X, being.Y).WhenTryingToLeave;
				if (whenTryingToLeave == null || !whenTryingToLeave.Interact(this, being))
				{
					return;
				}
			}
			if (being.IsAffectedBySpecialRoomPower(SpecialRoomPower.InvertedMovement))
			{
				int num = toX - fromX;
				int num2 = toY - fromY;
				if (Math.Abs(num) <= 1 && Math.Abs(num2) <= 1)
				{
					toX = fromX - num;
					toY = fromY - num2;
				}
			}
		}
		IMoveIntoInteraction moveInto = GetTileAt(toX, toY).MoveInto;
		if ((moveInto != null && !moveInto.MoveInto(this, thing, GetTileAt(toX, toY), fromX, fromY, toX, toY)) || !(GetThingAt(toX, toY)?.MoveInto?.MoveInto(this, thing, GetThingAt(toX, toY), fromX, fromY, toX, toY) ?? true))
		{
			if (thing is Being being2)
			{
				being2.ResetAutomaticAction();
			}
			return;
		}
		if (!forceMovement && !(thing is Player))
		{
			List<Item> itemsAt = GetItemsAt(toX, toY);
			if (itemsAt != null)
			{
				foreach (Item item in itemsAt)
				{
					if (item.Id == ItemId.ScrollOfScareMonster)
					{
						return;
					}
				}
			}
		}
		_thingMap[fromX][fromY] = null;
		_thingMap[toX][toY] = thing;
		thing.X = toX;
		thing.Y = toY;
		thing.Moved = true;
		IRoom roomAt = GetRoomAt(fromX, fromY);
		IRoom roomAt2 = GetRoomAt(toX, toY);
		if (roomAt != roomAt2 && thing is Being being3 && !Game.Instance.IsFirstTurnWithAutomaticAction)
		{
			being3.ResetAutomaticAction();
		}
		else if (roomAt2 != null && thing is Being { AutomaticAction: not null } being4 && (GetTileAt(toX - 1, toY) == Tile.Door || GetTileAt(toX + 1, toY) == Tile.Door || GetTileAt(toX, toY - 1) == Tile.Door || GetTileAt(toX, toY + 1) == Tile.Door))
		{
			being4.ResetAutomaticAction();
		}
		if (thing is Being being5)
		{
			GetTileAt(being5.X, being5.Y)?.WhenEntering?.Interact(this, being5);
			GetFeatureAt(being5.X, being5.Y)?.WhenEntering?.Interact(this, being5);
			GetRoomAt(being5.X, being5.Y)?.WhenMovingWithinRoom?.Interact(this, being5);
		}
		if (thing is Player player2)
		{
			InteractWithSurroundingMapElements(this, player2);
			player2.ExecuteMovementBasedInteractions(this);
			player2.Moves++;
		}
	}

	public void InteractWithSurroundingMapElements(DungeonLevel dungeonLevel, Player grog)
	{
		InteractionContext interactionContext = new InteractionContext();
		InteractWithAdjacentMapElements(dungeonLevel, grog, grog.X, grog.Y, interactionContext);
		InteractWithDiagonalMapElements(dungeonLevel, grog, grog.X, grog.Y, interactionContext);
		interactionContext.Interact();
	}

	private void InteractWithDiagonalMapElements(DungeonLevel dungeonLevel, Player grog, int x, int y, InteractionContext interactionContext)
	{
		InteractWithDiagonalMapElement(dungeonLevel, grog, x - 1, y - 1, interactionContext);
		InteractWithDiagonalMapElement(dungeonLevel, grog, x + 1, y + 1, interactionContext);
		InteractWithDiagonalMapElement(dungeonLevel, grog, x - 1, y + 1, interactionContext);
		InteractWithDiagonalMapElement(dungeonLevel, grog, x + 1, y - 1, interactionContext);
	}

	private void InteractWithAdjacentMapElements(DungeonLevel dungeonLevel, Player grog, int x, int y, InteractionContext interactionContext)
	{
		InteractWithAdjacentMapElement(dungeonLevel, grog, x - 1, y, interactionContext);
		InteractWithAdjacentMapElement(dungeonLevel, grog, x + 1, y, interactionContext);
		InteractWithAdjacentMapElement(dungeonLevel, grog, x, y - 1, interactionContext);
		InteractWithAdjacentMapElement(dungeonLevel, grog, x, y + 1, interactionContext);
	}

	private void InteractWithAdjacentMapElement(DungeonLevel dungeonLevel, Player grog, int x, int y, InteractionContext interactionContext)
	{
		if (!IsValid(x, y))
		{
			return;
		}
		Thing thingAt = GetThingAt(x, y);
		thingAt?.InteractWithAdjacentElement(dungeonLevel, grog, x, y, thingAt, interactionContext);
		if (grog.IsAlive)
		{
			GetFeatureAt(x, y)?.InteractWithAdjacentElement(dungeonLevel, grog, x, y, thingAt, interactionContext);
			if (grog.IsAlive)
			{
				GetTileAt(x, y)?.InteractWithAdjacentElement(dungeonLevel, grog, x, y, thingAt, interactionContext);
			}
		}
	}

	private void InteractWithDiagonalMapElement(DungeonLevel dungeonLevel, Player grog, int x, int y, InteractionContext interactionContext)
	{
		if (!IsValid(x, y))
		{
			return;
		}
		Thing thingAt = GetThingAt(x, y);
		thingAt?.InteractWithDiagonalElement(dungeonLevel, grog, x, y, thingAt, interactionContext);
		if (grog.IsAlive)
		{
			GetFeatureAt(x, y)?.InteractWithDiagonalElement(dungeonLevel, grog, x, y, thingAt, interactionContext);
			if (grog.IsAlive)
			{
				GetTileAt(x, y)?.InteractWithDiagonalElement(dungeonLevel, grog, x, y, thingAt, interactionContext);
			}
		}
	}

	public bool CanMoveTo(Thing thing, Direction direction)
	{
		int num = thing.X;
		int num2 = thing.Y;
		switch (direction)
		{
		case Direction.East:
			num++;
			break;
		case Direction.West:
			num--;
			break;
		case Direction.MinDirection:
			num2--;
			break;
		case Direction.South:
			num2++;
			break;
		}
		if (!IsValid(num, num2))
		{
			return false;
		}
		if ((GetTileAt(num, num2) == Tile.Wall || GetTileAt(num, num2) == Tile.WallOfRoom) && (!(thing is Being being) || (being.SpecialAbility != SpecialAbility.Desolid && being.SpecialAbility != SpecialAbility.Burrow && being.SpecialAbility != SpecialAbility.PoisonAndBurrow)))
		{
			return false;
		}
		return true;
	}

	public bool IsValid(int x, int y)
	{
		if (x >= 0 && y >= 0 && x < Width)
		{
			return y < Height;
		}
		return false;
	}

	public Thing GetThingAt(int x, int y)
	{
		if (!IsValid(x, y))
		{
			return null;
		}
		return _thingMap[x][y];
	}

	public Feature GetFeatureAt(int x, int y)
	{
		if (!IsValid(x, y))
		{
			return null;
		}
		return _featureMap[x][y];
	}

	public bool HasFeatureAt(int x, int y)
	{
		return GetFeatureAt(x, y) != null;
	}

	public void SetFeatureAt(int x, int y, Feature feature)
	{
		if (feature == null && _featureMap[x][y] != null && (_featureMap[x][y] is KnownTrapFeature || _featureMap[x][y] is HiddenTrapFeature))
		{
			RemoveTrapAt(x, y);
		}
		else
		{
			_featureMap[x][y] = feature;
		}
	}

	public void ClearFeatureAt(int x, int y)
	{
		SetFeatureAt(x, y, null);
	}

	public void ClearMessages()
	{
		_message = null;
	}

	public bool Message(string message, bool more = false, bool renderMap = true, string moreMessage = "---more---", char moreKey = ' ')
	{
		if (message == null)
		{
			return false;
		}
		return Message(null, (message.StartsWith(" ") || message.StartsWith("'")) ? (Grog.Name + message) : message, more, renderMap, moreMessage, moreKey);
	}

	public static int MessageSerial;

	public bool Message(Thing observed, string message, bool more = false, bool renderMap = true, string moreMessage = "---more---", char moreKey = ' ')
	{
		if (message == null)
		{
			return false;
		}
		if (observed != null && !Grog.CanSee(this, observed))
		{
			return false;
		}
		LogMessage(observed is Being b0 && (message.StartsWith(" ") || message.StartsWith("'")) ? b0.Name + message : message);
		if (!Tile.IsTileDescription(message))
		{
			MessageSerial++; // RVIP: auto-explore stops on new messages (not tile descriptions)
		}
		if (observed != null && observed is Being being && (message.StartsWith(" ") || message.StartsWith("'")))
		{
			message = being.Name + message;
		}
		if (_message == null)
		{
			_message = char.ToUpper(message[0]) + message.Substring(1);
		}
		else if (_message.EndsWith(")"))
		{
			int num = _message.LastIndexOf(" (x", StringComparison.InvariantCulture);
			if (num != -1)
			{
				if (_message.Substring(0, num).EndsWith(message, StringComparison.InvariantCultureIgnoreCase))
				{
					int.TryParse(_message.Substring(num + 3, _message.Length - num - 4), out var result);
					_message = _message.Substring(0, num) + " (x" + (result + 1) + ")";
				}
				else
				{
					_message = _message + " " + char.ToUpper(message[0]) + message.Substring(1);
				}
			}
		}
		else if (_message.EndsWith(message))
		{
			_message += " (x2)";
		}
		else
		{
			_message = _message + " " + char.ToUpper(message[0]) + message.Substring(1);
		}
		if (more)
		{
			Render(more: true, moreMessage, moreKey, renderMap);
		}
		return true;
	}

	public Tile GetTileAt(int x, int y)
	{
		if (!IsValid(x, y))
		{
			return null;
		}
		return Tile.GetTile(_tileMap[x][y]);
	}

	public void MovePlayerWest()
	{
		MoveThing(Grog, Grog.X, Grog.Y, Grog.X - 1, Grog.Y);
	}

	public void MovePlayerSouth()
	{
		MoveThing(Grog, Grog.X, Grog.Y, Grog.X, Grog.Y + 1);
	}

	public void MovePlayerEast()
	{
		MoveThing(Grog, Grog.X, Grog.Y, Grog.X + 1, Grog.Y);
	}

	public void SetTile(int x, int y, Tile tile)
	{
		_tileMap[x][y] = tile.Index;
	}

	public void SetBeing(int x, int y, Being being)
	{
		_thingMap[x][y] = being;
		Things.Add(being);
		being.X = x;
		being.Y = y;
	}

	public IRoom AddRoom(int x1, int y1, int x2, int y2)
	{
		IRoom room = new RectangularRoom(this, x1, y1, x2, y2);
		_rooms.Add(room);
		for (int i = x1; i <= x2; i++)
		{
			for (int j = y1; j <= y2; j++)
			{
				if (room.IsPartOfRoom(i, j))
				{
					_roomMap[i][j] = room;
				}
			}
		}
		return room;
	}

	public IRoom GetRoomAt(int x, int y)
	{
		if (!IsValid(x, y))
		{
			return null;
		}
		return _roomMap[x][y];
	}

	public void RemoveThing(int x, int y)
	{
		Thing thingAt = GetThingAt(x, y);
		int x2 = (thingAt.Y = -1);
		thingAt.X = x2;
		Things.Remove(thingAt);
		_thingMap[x][y] = null;
		if (Grog.CanSee(this, x, y) && GetRoomAt(x, y) != null && _memory[x][y] != ' ')
		{
			_memory[x][y] = ' ';
		}
	}

	public void ExecuteBeingActions(bool grogOnly = false)
	{
		foreach (Thing item in new List<Thing>(Things))
		{
			Being being = item as Being;
			if (grogOnly && !(being is Player))
			{
				continue;
			}
			if (being == null || being.SpecialAbility != SpecialAbility.MovesSlowly || being.Moves % 3 != 0)
			{
				if (being != null && being.IsHastened < 0 && being.Moves % 2 != 0)
				{
					continue;
				}
				int num = ((being == null || being.IsHastened <= 0) ? 1 : 2);
				for (int i = 0; i < num; i++)
				{
					if (being != null && !being.IsAlive)
					{
						break;
					}
					if (!item.IsActive)
					{
						break;
					}
					if (being != null && being.SpecialAbility == SpecialAbility.MovesSlowly && being.Moves % 3 == 0)
					{
						being.Moves++;
						continue;
					}
					item.HandleEffects(this);
					item.Act?.Act(this, item);
					if (!Grog.IsAlive)
					{
						return;
					}
				}
			}
			else if (being.SpecialAbility == SpecialAbility.MovesSlowly)
			{
				being.Moves++;
			}
		}
	}

	public bool IsOpenForMonster(Being monster, int x, int y)
	{
		if (!IsValid(x, y))
		{
			return false;
		}
		Thing thingAt = GetThingAt(x, y);
		Tile tileAt = GetTileAt(x, y);
		if (thingAt == null || thingAt is Player)
		{
			if (tileAt == Tile.Wall || tileAt == Tile.WallOfRoom)
			{
				if (monster != null)
				{
					if (monster.SpecialAbility != SpecialAbility.Desolid && monster.SpecialAbility != SpecialAbility.PoisonAndBurrow)
					{
						return monster.SpecialAbility == SpecialAbility.Burrow;
					}
					return true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public bool IsOpenForNewThing(int x, int y)
	{
		if (!IsValid(x, y))
		{
			return false;
		}
		Thing thingAt = GetThingAt(x, y);
		Tile tileAt = GetTileAt(x, y);
		if (thingAt == null)
		{
			return tileAt.IsOpen;
		}
		return false;
	}

	public bool IsOpen(int x, int y)
	{
		if (!IsValid(x, y))
		{
			return false;
		}
		return GetTileAt(x, y).IsOpen;
	}

	public char MemoryOf(int x, int y)
	{
		return _memory[x][y];
	}

	public Position FindPositionOfTile(Tile tile)
	{
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Tile tileAt = GetTileAt(i, j);
				if (tile == tileAt)
				{
					return new Position(i, j);
				}
			}
		}
		return Position.Undefined;
	}

	public void InteractWithEnvironment()
	{
		Feature featureAt = GetFeatureAt(Grog.X, Grog.Y);
		if (featureAt?.Interaction != null)
		{
			featureAt.Interaction?.Interact(this, Grog, Grog.X, Grog.Y);
			return;
		}
		Tile tileAt = GetTileAt(Grog.X, Grog.Y);
		if (tileAt.Interaction == null)
		{
			Message("Hu?");
		}
		else
		{
			tileAt.Interaction.Interact(this, Grog);
		}
	}

	public bool Sure(string question)
	{
		More();
		ClearMessages();
		Render();
		Curses.Instance.SetCursorPosition(0, 0);
		Curses.Instance.Write("Do you want to " + question + " [y/n]? ");
		char keyChar = Curses.Instance.ReadKey().KeyChar;
		ClearMessages();
		return keyChar == 'y';
	}

	public void DisplayStringList(List<string> strings, bool showMore = true, bool waitForMore = true, List<bool> highlight = null)
	{
		int num = 0;
		foreach (string @string in strings)
		{
			num = Math.Max(num, @string.Length);
		}
		num += 2;
		int num2 = Curses.Instance.WindowHeight - (waitForMore ? 3 : 0);
		if (showMore)
		{
			num2--;
		}
		List<List<string>> list = new List<List<string>>();
		for (int i = 0; i < strings.Count / num2 + 1; i++)
		{
			List<string> list2 = new List<string>();
			for (int j = 0; j < num2 && i * num2 + j != strings.Count; j++)
			{
				list2.Add(strings[i * num2 + j]);
			}
			list.Add(list2);
		}
		int num3 = 0;
		while (true)
		{
			Render();
			List<string> list3 = list[num3];
			for (int k = 0; k < list3.Count; k++)
			{
				Curses.Instance.SetCursorPosition(Curses.Instance.WindowWidth - num - 2, k);
				if (highlight != null && highlight[num3 * num2 + k])
				{
					Curses.Instance.Write("  ");
					Curses.Instance.InvertColors();
					Curses.Instance.Write(list3[k]);
					Curses.Instance.ClearToEndOfLine();
					Curses.Instance.InvertColors();
				}
				else
				{
					Curses.Instance.Write("  " + list3[k]);
					Curses.Instance.ClearToEndOfLine();
				}
			}
			if (showMore)
			{
				Curses.Instance.SetCursorPosition(Curses.Instance.WindowWidth - num - 2, list3.Count);
				Curses.Instance.Write("  ");
				Curses.Instance.InvertColors();
				Curses.Instance.Write("---more---");
				Curses.Instance.InvertColors();
				Curses.Instance.ClearToEndOfLine();
			}
			Curses.Instance.Refresh();
			if (waitForMore || list.Count > 1)
			{
				if (Curses.Instance.ReadKey(showReadKey: false, showCursor: false).KeyChar == ' ' && list.Count > 1)
				{
					num3 = (num3 + 1) % list.Count;
					continue;
				}
				Render();
				break;
			}
			break;
		}
	}

	public void AddItem(int x, int y, Item item)
	{
		if (_items[x][y] == null)
		{
			_items[x][y] = new List<Item>();
		}
		if (_items[x][y].Count < 26)
		{
			_items[x][y].Add(item);
			return;
		}
		List<Position> list = new List<Position>();
		for (int i = x - 1; i <= x + 1; i++)
		{
			for (int j = y - 1; j <= y + 1; j++)
			{
				if ((i != x || j != y) && IsValid(i, j) && IsOpen(i, j) && (_items[i][j] == null || _items[i][j].Count < 26))
				{
					list.Add(new Position(i, j));
				}
			}
		}
		if (list.Count == 0)
		{
			if (Grog.CanSee(this, x, y))
			{
				ResetAutoMore();
				Message("An oily black demonling appears out of nowhere!", more: true);
				Message("It snatches the " + item.ShortDescription + " and disappears in a sulphuric puff cackling with glee!", more: true);
			}
			return;
		}
		Position position = list[Game.Instance.Random(list.Count)];
		string relativeDirectionText = Game.Instance.GetRelativeDirectionText(x, y, position.X, position.Y);
		if (Grog.CanSee(this, x, y))
		{
			Message("The " + item.ShortDescription + " slides to the " + relativeDirectionText + "!");
		}
		AddItem(position.X, position.Y, item);
	}

	public bool IsItemAt(int x, int y)
	{
		if (!IsValid(x, y))
		{
			return false;
		}
		if (_items[x][y] != null)
		{
			return _items[x][y].Count == 1;
		}
		return false;
	}

	public bool IsItemsAt(int x, int y)
	{
		if (!IsValid(x, y))
		{
			return false;
		}
		if (_items[x][y] != null)
		{
			return _items[x][y].Count > 1;
		}
		return false;
	}

	public Item GetItemAt(int x, int y)
	{
		if (!IsItemAt(x, y))
		{
			throw new GrogException("Requested one item at " + x + ", " + y + " but found " + _items[x][y].Count + ".");
		}
		return _items[x][y][0];
	}

	public void RemoveItemAt(int x, int y, Item item)
	{
		if (!IsValid(x, y))
		{
			throw new GrogException("Invalid item removal position: " + x + ", " + y + " (" + item.Description + ")");
		}
		if (!HasItems(x, y) || !_items[x][y].Remove(item))
		{
			throw new GrogException(" Trying to remove non-existant item " + item.Description + " at " + x + ", " + y + ".");
		}
	}

	public List<Item> GetItemsAt(int x, int y)
	{
		if (!HasItems(x, y))
		{
			return null;
		}
		return new List<Item>(_items[x][y]);
	}

	public void PickUpItems()
	{
		if (!HasItems(Grog.X, Grog.Y))
		{
			Message("There is nothing here to pick up.");
		}
		else if (!Grog.Inventory.CanTake(1))
		{
			Message(" is carrying too much. Drop something else first!");
		}
		else if (IsItemAt(Grog.X, Grog.Y))
		{
			Item itemAt = GetItemAt(Grog.X, Grog.Y);
			RemoveItem(Grog.X, Grog.Y, itemAt);
			Grog.Inventory.Add(itemAt);
			Message(" picks up the " + itemAt.Description + " (" + itemAt.AssociatedCharacter + ").");
			Grog.Moves++;
		}
		else
		{
			List<Item> itemsAt = GetItemsAt(Grog.X, Grog.Y);
			ItemSelectionList itemsToPickUp = new ItemSelectionList(itemsAt, useAssociatedItemCharacters: false);
			ProcessItemSelectionList(() => GetPickUpInfos(itemsToPickUp), itemsToPickUp, ProcessPickupCommand);
		}
	}

	private List<string> GetPickUpInfos(ItemSelectionList itemsToPickUp)
	{
		return new List<string>
		{
			"--- Pick up stuff ---",
			"[" + GetCompressedInteractionRange(itemsToPickUp.AssociatedKeys) + "$SPACE-HINT$] Pick up item"
		};
	}

	public void ProcessItemSelectionList(Func<List<string>> getInfos, ItemSelectionList list, Func<ItemSelectionList, char, bool, bool> processKey, bool printChoice = true, Func<string> headline = null, bool highlightEquippedItems = false)
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		bool flag = false;
		char c;
		bool nav = false, stop = false; // RVIP 3c: cursor moves / item menu handled here
		int[] itemPage = new int[0], itemLine = new int[0];
		do
		{
			nav = false;
			List<string> list2 = getInfos();
			int num4 = Curses.Instance.WindowHeight - 2 - ((headline != null) ? 1 : 0) - list2.Count - (printChoice ? 1 : 0);
			bool flag2 = false;
			foreach (string item2 in list2)
			{
				if (item2.Contains("[$SPACE-HINT$]"))
				{
					flag2 = true;
					break;
				}
			}
			int splitIndex = GetSplitIndex(list);
			bool flag3 = false;
			List<List<bool>> list3 = new List<List<bool>>();
			List<bool> list4 = new List<bool>();
			List<List<string>> list5 = new List<List<string>>();
			List<string> list6 = new List<string>();
			int num5 = 0;
			itemPage = new int[list.Count];
			itemLine = new int[list.Count];
			int itemIndex = 0;
			foreach (Item item3 in list)
			{
				string itemInfo = GetItemInfo(list, item3);
				string[] splitLines = GetSplitLines(splitIndex, itemInfo);
				flag3 |= splitLines.Length > 1;
				while (splitLines.Length + list6.Count > num4)
				{
					list5.Add(list6);
					list3.Add(list4);
					list6 = new List<string>();
					list4 = new List<bool>();
				}
				itemPage[itemIndex] = list5.Count;
				itemLine[itemIndex++] = list6.Count;
				bool item = highlightEquippedItems && item3.IsEquipped;
				for (int i = 0; i < splitLines.Length; i++)
				{
					list6.Add(((i > 0) ? "    " : "") + splitLines[i]);
					num5 = Math.Max(splitLines[i].Length, num5);
					list4.Add(item);
				}
			}
			if (list6.Count > 0)
			{
				list5.Add(list6);
				list3.Add(list4);
			}
			if (flag2 && list.Count / num4 == 2 && list.Count % num4 == 1 && num4 < Curses.Instance.WindowHeight - 2 && !flag3)
			{
				num4++;
			}
			num3 = list5.Count;
			if (list.Count > 0)
			{
				_itemCursor = Math.Max(0, Math.Min(_itemCursor, list.Count - 1));
				num2 = itemPage[_itemCursor];
			}
			if (num2 >= num3)
			{
				num2 = num3 - 1;
			}
			Render();
			ClearMessages();
			Term.MainView = false; Term.Clickable = true; // RVIP 5: list over the map -> whole screen, rows clickable
			List<string> list7 = new List<string>();
			for (int j = 0; j < list2.Count; j++)
			{
				if (list2[j].Contains("[$SPACE-HINT$]"))
				{
					if (num3 > 1)
					{
						list7.Add(GetLengthDelimited(list2[j].Replace("$SPACE-HINT$", "SPACE")));
						num = Math.Max(num, list7[j].Length);
					}
				}
				else
				{
					list7.Add(GetLengthDelimited(list2[j].Replace("$SPACE-HINT$", (num3 > 1) ? ", SPACE to page" : "")));
					num = Math.Max(num, list7[j].Length);
				}
			}
			list2 = list7;
			if (headline != null)
			{
				num = Math.Max(num, headline().Length);
			}
			if (num3 > 1)
			{
				num = Math.Max(32, num);
			}
			int num6 = Math.Min(Curses.Instance.WindowWidth - 2, Math.Max(num5 + 4, num));
			if (headline != null)
			{
				Curses.Instance.SetCursorPosition(Curses.Instance.WindowWidth - num6, 0);
				Curses.Instance.Write(headline());
				if (Curses.Instance.CursorX > 0)
				{
					Curses.Instance.ClearToEndOfLine();
				}
			}
			List<string> list8 = list5[num2];
			List<bool> list9 = list3[num2];
			for (int k = 0; k < list8.Count; k++)
			{
				Curses.Instance.SetCursorPosition(Curses.Instance.WindowWidth - num6, ((headline != null) ? 1 : 0) + k);
				if (list9[k])
				{
					Curses.Instance.Write(" | ");
					Curses.Instance.InvertColors();
					Curses.Instance.Write(' ');
				}
				else
				{
					Curses.Instance.Write(" |  ");
				}
				Curses.Instance.Write(list8[k]);
				if (Curses.Instance.CursorX > 0)
				{
					Curses.Instance.ClearToEndOfLine();
				}
				if (list9[k])
				{
					Curses.Instance.InvertColors();
				}
			}
			if (list.Count > 0 && itemPage[_itemCursor] == num2)
			{
				Curses.Instance.SetCursorPosition(Curses.Instance.WindowWidth - num6 + 2, ((headline != null) ? 1 : 0) + itemLine[_itemCursor]);
				Curses.Instance.Write('>'); // RVIP 3c: item cursor
			}
			for (int l = 0; l < list2.Count; l++)
			{
				Curses.Instance.SetCursorPosition(Curses.Instance.WindowWidth - num6 - 2, list8.Count + l + ((headline != null) ? 1 : 0));
				if (!printChoice && l == list2.Count - 1)
				{
					Curses.Instance.Write("   ");
					Curses.Instance.InvertColors();
					Curses.Instance.Write(GetLengthDelimited(list2[l]));
					Curses.Instance.InvertColors();
				}
				else
				{
					Curses.Instance.Write("   " + GetLengthDelimited(list2[l]));
				}
				if (Curses.Instance.CursorX > 0)
				{
					Curses.Instance.ClearToEndOfLine();
				}
			}
			if (printChoice)
			{
				Curses.Instance.SetCursorPosition(Curses.Instance.WindowWidth - num6 - 2, list8.Count + list2.Count + ((headline != null) ? 1 : 0));
				Curses.Instance.Write("   Your choice" + ((num3 > 1) ? " [SPACE to page]" : "") + ": ".PadRight(num - 1));
				if (Curses.Instance.CursorX > 0)
				{
					Curses.Instance.ClearToEndOfLine();
				}
				Curses.Instance.SetCursorPosition(Math.Min(Curses.Instance.WindowWidth - 1, Curses.Instance.WindowWidth - num6 + 14 + ((num3 > 1) ? 16 : 0)), list8.Count + list2.Count + ((headline != null) ? 1 : 0));
			}
			ConsoleKeyInfo consoleKeyInfo = Curses.Instance.ReadKey(showReadKey: false, printChoice);
			if (consoleKeyInfo.Key == ConsoleKey.F24) consoleKeyInfo = ItemClick(consoleKeyInfo.KeyChar - 0xE000 - ((headline != null) ? 1 : 0), itemPage, itemLine, num2); // RVIP 5: mouse
			c = consoleKeyInfo.KeyChar;
			flag = (consoleKeyInfo.Modifiers & ConsoleModifiers.Control) != 0 || consoleKeyInfo.Modifiers.HasFlag(ConsoleModifiers.Control);
			if (flag && consoleKeyInfo.Key >= ConsoleKey.A && consoleKeyInfo.Key <= ConsoleKey.Z)
			{
				c = (char)(97 + (consoleKeyInfo.Key - 65));
			}
			if (c == ' ' && num3 > 1)
			{
				num2 = (num2 + 1) % num3;
				_itemCursor = Array.IndexOf(itemPage, num2);
			}
			else if (list.Count > 0)
			{
				_lastItemKey = consoleKeyInfo;
				(nav, stop, c, flag) = ItemCursorKey(list, consoleKeyInfo, c, flag, num3 > 1);
			}
		}
		while (!stop && (nav || (c == ' ' && num3 > 1) || processKey(list, c, flag)));
		Render();
	}

	private int GetSplitIndex(ItemSelectionList list)
	{
		int num = 0;
		int num2 = 0;
		if (list.Count <= 8)
		{
			foreach (Item item in list)
			{
				string itemInfo = GetItemInfo(list, item);
				num2 = Math.Max(itemInfo.Length, num2);
				num += itemInfo.Length;
			}
			num /= list.Count;
		}
		else
		{
			List<int> list2 = new List<int>();
			foreach (Item item2 in list)
			{
				string itemInfo2 = GetItemInfo(list, item2);
				num2 = Math.Max(itemInfo2.Length, num2);
				list2.Add(itemInfo2.Length);
			}
			list2.Sort();
			num = ((list2[list2.Count - 2] >= Curses.Instance.WindowWidth / 3 * 2) ? list2[list2.Count - 3] : list2[list2.Count - 2]);
			foreach (Item item3 in list)
			{
				int val = GetItemInfo(list, item3).IndexOf(' ');
				num = Math.Max(num, val);
			}
		}
		if (num2 < Curses.Instance.WindowWidth / 3 * 2)
		{
			return num2;
		}
		return Math.Min(num, Curses.Instance.WindowWidth - 20);
	}

	private string GetItemInfo(ItemSelectionList list, Item item)
	{
		return list.CharacterAssociatedWith(item) + " " + (item.IsExciting ? "!" : "-") + " " + item.Description;
	}

	private string[] GetSplitLines(int splitPoint, string info)
	{
		List<string> list = new List<string>();
		if (info.Length <= splitPoint)
		{
			list.Add(info);
		}
		else
		{
			do
			{
				if (info.Length < splitPoint)
				{
					list.Add(info);
					info = "";
					continue;
				}
				int num = info.LastIndexOf(' ', splitPoint);
				string item = info.Substring(0, num).Trim();
				list.Add(item);
				info = info.Substring(num).Trim();
			}
			while (info.Length > 0);
		}
		return list.ToArray();
	}

	private string GetTruncatedItemDescription(bool excessiveItemNames, string description)
	{
		if (description.Length <= Curses.Instance.WindowWidth - 18)
		{
			return description;
		}
		return description.Substring(0, Curses.Instance.WindowWidth - 18) + "...";
	}

	private string GetLengthDelimited(string line)
	{
		if (line.Length >= Width - 5)
		{
			return line.Substring(0, Width - 5) + "...";
		}
		return line;
	}

	private bool ProcessPickupCommand(ItemSelectionList list, char c, bool isCtrlModified)
	{
		if (isCtrlModified)
		{
			return false;
		}
		Item itemAssociatedWith = list.GetItemAssociatedWith(c);
		if (itemAssociatedWith != null)
		{
			list.Remove(itemAssociatedWith);
			RemoveItem(Grog.X, Grog.Y, itemAssociatedWith);
			Grog.Inventory.Add(itemAssociatedWith);
			global::Grog.Kernel.GCurses.Term.Sound("pickup"); //RVIP 6b
			Message(" picks up " + itemAssociatedWith.Description + " (" + itemAssociatedWith.AssociatedCharacter + ").");
			bool flag = Grog.Inventory.CanTake(1);
			if (!flag)
			{
				Message("The inventory is full!", more: true);
			}
			return (list.Count > 0) & flag;
		}
		return false;
	}

	private void RemoveItem(int x, int y, Item item)
	{
		if (!HasItems(x, y))
		{
			throw new GrogException("Trying to remove an item from an invalid location (" + x + ", " + y + "): " + item.Description);
		}
		if (!_items[x][y].Remove(item))
		{
			throw new GrogException(item.Description + " does not exist at " + x + ", " + y + ".");
		}
		if (_items[x][y].Count == 0)
		{
			_items[x][y] = null;
		}
	}

	public void DisplayInventoryAfterDeath()
	{
		if (Grog.Inventory.ItemsCarried == 0)
		{
			return;
		}
		foreach (Item item in Grog.Inventory.GetInventory())
		{
			item.IsIdentified = true;
			if (item.IsCursed)
			{
				item.IsCursedStatusKnown = true;
			}
		}
		ItemSelectionList list = new ItemSelectionList(Grog.Inventory.GetInventory(), useAssociatedItemCharacters: false);
		ProcessItemSelectionList(() => GetInventoryInfos(list, 0, "Press ESC to continue$SPACE-HINT$."), list, ProcessEndKey, printChoice: false, () => "--- " + Grog.Name + "'s possessions " + (Grog.IsAlive ? "when last seen" : "in the moment of death") + " ---");
	}

	public void ManageInventory()
	{
		if (Grog.Inventory.ItemsCarried == 0)
		{
			Message(" does not own anything.");
			return;
		}
		ItemSelectionList list = new ItemSelectionList(Grog.Inventory.GetInventory(), useAssociatedItemCharacters: true);
		_inInventory = true;
		try
		{
		ProcessItemSelectionList(() => GetInventoryInfos(list, Grog.Gold), list, ProcessInventoryMain, printChoice: true, () => "--- Manage Inventory (" + Grog.Inventory.ItemsCarried + "/" + Grog.Inventory.MaxItemCount + ") ---", highlightEquippedItems: true);
		}
		finally
		{
			_inInventory = false;
		}
	}

	private bool ProcessEndKey(ItemSelectionList list, char key, bool isCtrlModified)
	{
		if (key != '\u001b')
		{
			return !isCtrlModified;
		}
		return false;
	}

	private bool ProcessInventoryKey(ItemSelectionList list, char c, bool isCtrlModified)
	{
		if (c == '$')
		{
			int gold = GetGold("drop");
			if (gold > 0)
			{
				Message(" drops " + gold + " gold piece" + ((gold != 1) ? "s" : "") + ".", more: true);
				Grog.Gold -= gold;
				SetFeatureAt(Grog.X, Grog.Y, new GoldFeature(gold));
				Render();
				return false;
			}
		}
		else if (char.IsUpper(c))
		{
			Item itemAssociatedWith = list.GetItemAssociatedWith(char.ToLower(c));
			if (itemAssociatedWith != null && (!HasItems(Grog.X, Grog.Y) || GetItemsAt(Grog.X, Grog.Y).Count < Grog.Inventory.MaxItemCount))
			{
				if (FailsToUnequipCursedItem(itemAssociatedWith))
				{
					return true;
				}
				list.Remove(itemAssociatedWith);
				Grog.Inventory.Remove(itemAssociatedWith);
				itemAssociatedWith.IsEquipped = false;
				Message(" drops the " + itemAssociatedWith.Description + " (" + itemAssociatedWith.AssociatedCharacter + ").");
				PhysicallyDropItem(this, Grog, Grog.X, Grog.Y, itemAssociatedWith);
				Grog.Moves++;
				return Grog.Inventory.ItemsCarried > 0;
			}
		}
		else
		{
			if (isCtrlModified)
			{
				return ProcessUseKey(list, char.ToLower(c), isCtrlModified: true);
			}
			Item itemAssociatedWith2 = list.GetItemAssociatedWith(c);
			if (itemAssociatedWith2 != null && itemAssociatedWith2.IsEquippable)
			{
				if (!itemAssociatedWith2.IsEquipped)
				{
					if (itemAssociatedWith2.ItemType == ItemType.Armor)
					{
						foreach (Item item in list)
						{
							if (item.ItemType == ItemType.Armor && item.IsEquipped)
							{
								if (FailsToUnequipCursedItem(item))
								{
									return true;
								}
								item.IsEquipped = false;
								Grog.Moves++;
								break;
							}
						}
					}
					else if ((itemAssociatedWith2.ItemType == ItemType.Shield || itemAssociatedWith2.ItemType == ItemType.MeleeWeapon || itemAssociatedWith2.ItemType == ItemType.Ring) && !CanEquipRightNow(itemAssociatedWith2, list))
					{
						Message(" needs to unequip something first!", more: true);
						return true;
					}
				}
				if (FailsToUnequipCursedItem(itemAssociatedWith2))
				{
					return true;
				}
				itemAssociatedWith2.IsEquipped = !itemAssociatedWith2.IsEquipped;
				if (itemAssociatedWith2.IsEquipped && (itemAssociatedWith2.ItemType == ItemType.Armor || itemAssociatedWith2.ItemType == ItemType.Shield))
				{
					itemAssociatedWith2.IsIdentified = true;
				}
				Message((itemAssociatedWith2.IsEquipped ? "equip" : "unequip") + " the " + itemAssociatedWith2.Description + ".");
				Grog.Moves++;
				return true;
			}
		}
		return false;
	}

	public int GetGold(string action)
	{
		ClearMessages();
		Render();
		Curses.Instance.SetCursorPosition(0, 0);
		Curses.Instance.Write("How much gold do you want to " + action + " [0-" + Grog.Gold + "]? ");
		if (!int.TryParse(Curses.Instance.ReadLine(Grog.Gold.ToString().Length), out var result) || result <= 0 || result > Grog.Gold)
		{
			return 0;
		}
		return result;
	}

	public void PhysicallyDropItem(DungeonLevel dungeonLevel, Player grog, int x, int y, Item item)
	{
		item = GetTileAt(x, y).GetProcessedItem(this, grog, item);
		if (HasFeatureAt(x, y))
		{
			item = GetFeatureAt(x, y).GetProcessedItem(this, grog, item);
		}
		if (item != null)
		{
			AddItem(x, y, item);
		}
	}

	private bool FailsToUnequipCursedItem(Item item)
	{
		if (item.IsCursed && item.IsEquipped)
		{
			item.IsCursedStatusKnown = true;
			Message(" is unwilling to remove the " + item.ShortDescription + "!", more: true);
			return true;
		}
		return false;
	}

	private bool CanEquipRightNow(Item item, ItemSelectionList list)
	{
		if (!item.IsEquippable)
		{
			return false;
		}
		int num = 0;
		foreach (Item item2 in list)
		{
			if (((item.ItemType == ItemType.Ring && item2.ItemType == ItemType.Ring) || ((item.ItemType == ItemType.Shield || item.ItemType == ItemType.MeleeWeapon) && (item2.ItemType == ItemType.Shield || item2.ItemType == ItemType.MeleeWeapon))) && item2.IsEquipped)
			{
				num += item2.EquipmentSlots;
			}
		}
		return num < 3 - item.EquipmentSlots;
	}

	private List<string> GetInventoryInfos(ItemSelectionList list, int gold, string commandPrompt = null)
	{
		List<string> list2 = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		StringBuilder stringBuilder3 = new StringBuilder();
		StringBuilder stringBuilder4 = new StringBuilder();
		if (gold > 0 && GetFeatureAt(Grog.X, Grog.Y) == null)
		{
			stringBuilder2.Append('$');
		}
		foreach (Item item in list)
		{
			if (item.IsEquipped)
			{
				stringBuilder3.Append(item.AssociatedCharacter);
				if (!HasItems(Grog.X, Grog.Y) || GetItemsAt(Grog.X, Grog.Y).Count < Grog.Inventory.MaxItemCount)
				{
					stringBuilder2.Append(char.ToUpper(item.AssociatedCharacter));
				}
			}
			else
			{
				if (CanEquipRightNow(item, list))
				{
					stringBuilder.Append(item.AssociatedCharacter);
				}
				if (!HasItems(Grog.X, Grog.Y) || GetItemsAt(Grog.X, Grog.Y).Count < Grog.Inventory.MaxItemCount)
				{
					stringBuilder2.Append(char.ToUpper(item.AssociatedCharacter));
				}
			}
			if (item.UseAction != null)
			{
				stringBuilder4.Append(item.AssociatedCharacter);
			}
		}
		if (commandPrompt == null)
		{
			if (stringBuilder3.Length > 0)
			{
				list2.Add("[" + GetCompressedInteractionRange(stringBuilder3.ToString()) + "] Unequip item");
			}
			if (stringBuilder.Length > 0)
			{
				list2.Add("[" + GetCompressedInteractionRange(stringBuilder.ToString()) + "] Equip item");
			}
			if (stringBuilder2.Length > 0)
			{
				list2.Add("[" + GetCompressedInteractionRange(stringBuilder2.ToString()) + "] Drop item");
			}
			if (stringBuilder4.Length > 0)
			{
				list2.Add("[<Ctrl> + " + GetCompressedInteractionRange(stringBuilder4.ToString()) + "] Use item");
			}
			list2.Add("[$SPACE-HINT$] to page");
		}
		else
		{
			list2.Add(commandPrompt);
		}
		return list2;
	}

	public bool Teleport(Being being)
	{
		IRoom roomAt = GetRoomAt(being.X, being.Y);
		IRoom room = null;
		int num = 20;
		Position trulyInsidePositionForThing;
		do
		{
			room = _rooms[Game.Instance.Random(_rooms.Count)];
			trulyInsidePositionForThing = room.GetTrulyInsidePositionForThing(this);
		}
		while ((room == roomAt || trulyInsidePositionForThing.IsUndefined) && num-- > 0);
		if (!trulyInsidePositionForThing.IsUndefined && IsOpenForMonster(being, trulyInsidePositionForThing.X, trulyInsidePositionForThing.Y) && (trulyInsidePositionForThing.X != Grog.X || trulyInsidePositionForThing.Y != Grog.Y) && (being is Player || !IsItemOfTypeAt(trulyInsidePositionForThing, ItemId.ScrollOfScareMonster)))
		{
			return Teleport(being, trulyInsidePositionForThing);
		}
		return false;
	}

	public bool IsItemOfTypeAt(Position position, ItemId itemId)
	{
		return IsItemOfTypeAt(position.X, position.Y, itemId);
	}

	public bool IsItemOfTypeAt(int x, int y, ItemId itemId)
	{
		if (!IsValid(x, y))
		{
			return false;
		}
		if (!HasItems(x, y))
		{
			return false;
		}
		foreach (Item item in GetItemsAt(x, y))
		{
			if (item.Id == itemId)
			{
				return true;
			}
		}
		return false;
	}

	public bool Teleport(Being being, Position newPosition)
	{
		if (IsOpenForMonster(being, newPosition.X, newPosition.Y))
		{
			Message(being, " suddenly disappears!");
			RemoveThing(being.X, being.Y);
			SetBeing(newPosition.X, newPosition.Y, being);
			Message(being, " suddenly appears out of thin air!");
			being.Moved = true;
			being.ResetAutomaticAction();
			return true;
		}
		return false;
	}

	public void DisplayStringList(string[] texts)
	{
		DisplayStringList(new List<string>(texts));
	}

	public void UseItem()
	{
		List<Item> usableItems = Grog.Inventory.GetUsableItems();
		if (usableItems.Count == 0)
		{
			Message(" does not own any usable items.");
			return;
		}
		ItemSelectionList list = new ItemSelectionList(usableItems, useAssociatedItemCharacters: true);
		ProcessItemSelectionList(() => GetUsableInfos(list), list, ProcessUseKey);
	}

	public void ZapWand()
	{
		List<Item> usableItemsOfType = Grog.Inventory.GetUsableItemsOfType(ItemType.Wand);
		if (usableItemsOfType.Count == 0)
		{
			Message(" does not own any wands.");
			return;
		}
		ItemSelectionList list = new ItemSelectionList(usableItemsOfType, useAssociatedItemCharacters: true);
		ProcessItemSelectionList(() => GetUsableInfos(list), list, ProcessUseKey);
	}

	private bool ProcessUseKey(ItemSelectionList list, char c, bool isCtrlModified)
	{
		if (list == null)
		{
			return false;
		}
		Item itemAssociatedWith = list.GetItemAssociatedWith(c);
		if (itemAssociatedWith == null)
		{
			return false;
		}
		if (itemAssociatedWith.UseAction == null)
		{
			return false;
		}
		if (Grog.IsConfused != 0 && Game.Instance.Probability(80))
		{
			Message(" is too befuddled to do that!");
			Grog.Moves++;
			return false;
		}
		if (Grog.IsStunned != 0 && Game.Instance.Probability(50))
		{
			Message(" can't focus enough to do that!");
			Grog.Moves++;
			return false;
		}
		if (itemAssociatedWith.UseAction(this, Grog, Grog, itemAssociatedWith))
		{
			Grog.Inventory.Remove(itemAssociatedWith);
		}
		Grog.Moves++;
		return false;
	}

	private List<string> GetUsableInfos(ItemSelectionList list)
	{
		return new List<string> { "[" + GetCompressedInteractionRange(list.AssociatedKeys) + "] Use item (drink, zap, etc.)" };
	}

	public bool LearnMoreAboutMap(Func<DungeonLevel, int, int, bool> matcher, Func<DungeonLevel, int, int, char> learner)
	{
		bool result = false;
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (matcher(this, i, j))
				{
					char c = learner(this, i, j);
					if (_memory[i][j] != c)
					{
						result = true;
						_memory[i][j] = c;
					}
				}
			}
		}
		return result;
	}

	public bool IsBeingAt(int x, int y)
	{
		if (!IsValid(x, y))
		{
			return false;
		}
		Thing thingAt = GetThingAt(x, y);
		if (thingAt is Being)
		{
			return !(thingAt is Player);
		}
		return false;
	}

	public bool IsTrapAt(int x, int y)
	{
		if (!IsValid(x, y))
		{
			return false;
		}
		Position key = new Position(x, y);
		if (_traps.ContainsKey(key))
		{
			return true;
		}
		return GetFeatureAt(x, y) is HiddenTrapFeature;
	}

	public void DrawShot(char missileCharacter, int sx, int sy, int xm, int ym, int ex, int ey, Func<int, int, bool> executeEffectOnPosition = null)
	{
		do
		{
			Render();
			Curses.Instance.SetCursorPosition(sx, sy + 2);
			Curses.Instance.Write(missileCharacter);
			Curses.Instance.SetCursorPosition(sx, sy + 2);
			Curses.Instance.Refresh();
			if (executeEffectOnPosition != null && !executeEffectOnPosition(sx, sy))
			{
				break;
			}
			Term.Sleep(150);
			if (sx != ex || sy != ey)
			{
				sx += xm;
				sy += ym;
			}
		}
		while (sx != ex || sy != ey);
		Render();
		Curses.Instance.Refresh();
	}

	public void DrawRay(int sx, int sy, int xm, int ym, int range, Func<int, int, bool, Tile, Feature, Being, List<Item>, bool> executeEffectOnPosition = null)
	{
		char character = ((xm == 0) ? '|' : '-');
		bool flag = false;
		for (int i = 0; i < range; i++)
		{
			if (flag)
			{
				break;
			}
			sx += xm;
			sy += ym;
			if (!IsValid(sx, sy))
			{
				continue;
			}
			Render();
			Curses.Instance.SetCursorPosition(sx, sy + 2);
			Curses.Instance.Write(character);
			if (executeEffectOnPosition != null)
			{
				Tile tileAt = GetTileAt(sx, sy);
				Feature featureAt = GetFeatureAt(sx, sy);
				Being beingAt = GetBeingAt(sx, sy);
				List<Item> itemsAt = GetItemsAt(sx, sy);
				if (!executeEffectOnPosition(sx, sy, i == 0, tileAt, featureAt, beingAt, itemsAt))
				{
					flag = true;
				}
			}
			Curses.Instance.Refresh();
			Term.Sleep(100);
		}
		Render();
		Curses.Instance.Refresh();
	}

	public bool InitiateComplexInteraction(string text, params Interaction[] interactions)
	{
		return InitiateComplexInteraction(text, new List<string>(), () => interactions);
	}

	public bool InitiateComplexInteraction(string text, Func<Interaction[]> getInteractions)
	{
		return InitiateComplexInteraction(text, new List<string>(), getInteractions);
	}

	public bool InitiateComplexInteraction(string text, Func<List<Interaction>> getInteractions)
	{
		return InitiateComplexInteraction(text, new List<string>(), () => getInteractions().ToArray());
	}

	public bool InitiateComplexInteraction(string text, List<string> pagedText, Func<Interaction[]> getInteractions)
	{
		char c = ' ';
		bool flag = true;
		int num = 0;
		List<List<string>> list = new List<List<string>>();
		bool result = false;
		string text2 = "";
		bool flag2 = false;
		do
		{
			Interaction[] array = getInteractions();
			flag2 = array != null && array.Length != 0;
			text2 = "";
			Dictionary<char, Interaction> dictionary = new Dictionary<char, Interaction>();
			if (flag2)
			{
				List<char> list2 = new List<char>();
				Interaction[] array2 = array;
				foreach (Interaction interaction in array2)
				{
					list2.Add(interaction.Key);
					dictionary.Add(interaction.Key, interaction);
				}
				list2.Sort();
				StringBuilder stringBuilder = new StringBuilder();
				foreach (char item in list2)
				{
					stringBuilder.Append(item);
				}
				text2 = stringBuilder.ToString();
			}
			Render();
			string[] array3 = text.Split(new char[1] { ' ' });
			List<string> list3 = new List<string>();
			StringBuilder stringBuilder2 = new StringBuilder();
			int num2 = Width / 2;
			string[] array4 = array3;
			for (int i = 0; i < array4.Length; i++)
			{
				string[] array5 = array4[i].Split(new char[1] { '\n' });
				if (array5.Length > 1)
				{
					int num3 = 0;
					if (array5[0].Length > 0)
					{
						if (stringBuilder2.Length > 0)
						{
							stringBuilder2.Append(' ');
						}
						stringBuilder2.Append(array5[0]);
						num3++;
						list3.Add(stringBuilder2.ToString());
						stringBuilder2.Clear();
					}
					for (int j = num3; j < array5.Length; j++)
					{
						string text3 = array5[j];
						if (text3.Length == 0)
						{
							list3.Add(text3);
							continue;
						}
						string text4 = text3.Replace("\t", "    ");
						string text5 = ((stringBuilder2.Length > 0) ? " " : "");
						if (stringBuilder2.Length + text4.Length + text5.Length > num2)
						{
							list3.Add(stringBuilder2.ToString());
							stringBuilder2.Clear();
							stringBuilder2.Append(text4);
						}
						else
						{
							stringBuilder2.Append(text5);
							stringBuilder2.Append(text4);
						}
					}
				}
				else
				{
					string text6 = array5[0].Replace("\t", "    ");
					string text7 = ((stringBuilder2.Length > 0) ? " " : "");
					if (stringBuilder2.Length + text6.Length + text7.Length > num2)
					{
						list3.Add(stringBuilder2.ToString());
						stringBuilder2.Clear();
						stringBuilder2.Append(text6);
					}
					else
					{
						stringBuilder2.Append(text7);
						stringBuilder2.Append(text6);
					}
				}
			}
			if (stringBuilder2.Length > 0)
			{
				list3.Add(stringBuilder2.ToString());
			}
			for (int k = 0; k < list3.Count; k++)
			{
				list3[k] = "|  " + list3[k];
			}
			int num4 = 0;
			string text8 = null;
			List<string> list4 = new List<string>(pagedText);
			if (flag2)
			{
				Interaction[] array2 = array;
				foreach (Interaction interaction2 in array2)
				{
					list4.Add("|    [" + interaction2.Key + "] " + interaction2.Description);
				}
				text8 = "|  Your choice [" + GetCompressedInteractionRange(text2) + "$SPACE-HINT$]: ";
				num4 = 1;
			}
			list.Clear();
			if (list3.Count + num4 + list4.Count >= Curses.Instance.WindowHeight)
			{
				text8 = text8?.Replace("$SPACE-HINT$", ", SPACE to page");
				int num5 = Curses.Instance.WindowHeight - num4 - list3.Count;
				int num6 = list4.Count / num5 + ((list4.Count % num5 != 0) ? 1 : 0);
				for (int l = 0; l < num6; l++)
				{
					List<string> list5 = new List<string>(list3);
					for (int m = 0; m < num5; m++)
					{
						int num7 = l * num5 + m;
						list5.Add(list4[num7]);
						if (num7 == list4.Count - 1)
						{
							break;
						}
					}
					if (text8 != null)
					{
						list5.Add(text8);
					}
					list.Add(list5);
				}
			}
			else
			{
				text8 = text8?.Replace("$SPACE-HINT$", "");
				List<string> list6 = new List<string>(list3);
				list6.AddRange(list4);
				if (text8 != null)
				{
					list6.Add(text8);
				}
				list.Add(list6);
			}
			DisplayStringList(list[num], !flag2, !flag2);
			if (!flag2 && list.Count <= 0)
			{
				continue;
			}
			c = Curses.Instance.ReadKey().KeyChar;
			if (Enumerable.Contains(text2, c))
			{
				Interaction interaction3 = dictionary[c];
				ClearMessages();
				flag = interaction3.Execute();
				result = true;
				Render();
			}
			else if (c == ' ')
			{
				num = (num + 1) % list.Count;
				if (list.Count > 1)
				{
					flag = false;
				}
			}
		}
		while (flag2 && (Enumerable.Contains(text2, c) || (c == ' ' && list.Count > 1)) && !flag && Game.Instance.Grog.IsAlive);
		Render();
		return result;
	}

	private string GetCompressedInteractionRange(string interactions)
	{
		if (interactions.Contains("-"))
		{
			return interactions;
		}
		List<char> list = new List<char>(interactions.ToCharArray());
		list.Sort();
		interactions = new string(list.ToArray());
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		do
		{
			char c = interactions[num];
			int i;
			for (i = num + 1; i < interactions.Length && c + i - num == interactions[i]; i++)
			{
			}
			int num2 = i - num;
			if (num2 < 4)
			{
				for (int j = 0; j < num2; j++)
				{
					stringBuilder.Append((char)(c + j));
				}
			}
			else
			{
				stringBuilder.Append(c).Append('-').Append(interactions[i - 1]);
			}
			num = i;
		}
		while (num < interactions.Length - 1);
		if (num == interactions.Length - 1)
		{
			stringBuilder.Append(interactions[interactions.Length - 1]);
		}
		return stringBuilder.ToString();
	}

	public bool MapLevel()
	{
		bool flag = false;
		int x = Grog.X;
		int y = Grog.Y;
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				Feature feature = _featureMap[i][j];
				Tile tile = Tile.GetTile(_tileMap[i][j]);
				Grog.X = i;
				Grog.Y = j;
				char c = feature?.Character(this, i, j) ?? tile.Character(this, i, j);
				if (GetRoomAt(i, j) == null)
				{
					if (_memory[i][j] != c)
					{
						_memory[i][j] = c;
						flag = true;
						Curses.Instance.SetCursorPosition(i, j + 2);
						Curses.Instance.Write(c);
					}
				}
				else if ((GetRoomAt(i, j).IsInsideOfRoom(i, j) && feature != null) || GetRoomAt(i, j).IsWallOfRoom(i, j))
				{
					_memory[i][j] = ((GetRoomAt(i, j).IsWallOfRoom(i, j) && !GetRoomAt(i, j).IsDoorAt(i, j)) ? GetWallCharacter(i, j) : c);
					flag = true;
					Curses.Instance.SetCursorPosition(i, j + 2);
					Curses.Instance.Write(c);
				}
			}
		}
		Grog.X = x;
		Grog.Y = y;
		if (flag)
		{
			Render();
		}
		return flag;
	}

	public List<Being> GetAllBeingsVisibleTo(Being observer)
	{
		List<Being> list = new List<Being>();
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (GetThingAt(i, j) is Being being && being != observer && observer.CanSee(this, being))
				{
					list.Add(being);
				}
			}
		}
		return list;
	}

	public List<Item> GetItemsOfTypeAt(int x, int y, ItemType itemType)
	{
		List<Item> list = new List<Item>();
		if (HasItems(x, y))
		{
			foreach (Item item in GetItemsAt(x, y))
			{
				if (item.ItemType == itemType)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public bool MapLevel(Func<int, int, bool> shouldBeMapped, Func<int, int, char> getCharacter)
	{
		bool flag = false;
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (shouldBeMapped(i, j))
				{
					char c = getCharacter(i, j);
					if (_memory[i][j] != c)
					{
						_memory[i][j] = c;
						flag = true;
						Curses.Instance.SetCursorPosition(i, j + 2);
						Curses.Instance.Write(c);
					}
				}
			}
		}
		if (flag)
		{
			Render();
		}
		return flag;
	}

	public Position FindSuitableTeleportationTargetPosition()
	{
		List<Position> list = new List<Position>();
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if ((GetTileAt(i, j) == Tile.Floor || GetTileAt(i, j) == Tile.Tunnel || GetTileAt(i, j) == Tile.Door) && GetThingAt(i, j) == null && !HasFeatureAt(i, j) && !HasItems(i, j))
				{
					list.Add(new Position(i, j));
				}
			}
		}
		if (list.Count <= 0)
		{
			return Position.Undefined;
		}
		return list[Game.Instance.Random(list.Count)];
	}

	public Position FindPositionForMonsterCreationAround(Being being)
	{
		List<Position> list = new List<Position>();
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j < 2; j++)
			{
				if (i != 0 || j != 0)
				{
					int x = being.X + i;
					int y = being.Y + j;
					if ((GetTileAt(x, y) == Tile.Floor || GetTileAt(x, y) == Tile.Tunnel || GetTileAt(x, y) == Tile.Door) && GetThingAt(x, y) == null)
					{
						list.Add(new Position(x, y));
					}
				}
			}
		}
		if (list.Count <= 0)
		{
			return Position.Undefined;
		}
		return list[Game.Instance.Random(list.Count)];
	}

	public List<Being> GetAllBeings()
	{
		List<Being> list = new List<Being>();
		for (int i = 0; i < Width; i++)
		{
			for (int j = 0; j < Height; j++)
			{
				if (GetThingAt(i, j) is Being item)
				{
					list.Add(item);
				}
			}
		}
		return list;
	}

	public IRoom FindEmptyRoom()
	{
		List<IRoom> list = new List<IRoom>();
		foreach (IRoom room in _rooms)
		{
			if (room.IsBoring(this))
			{
				list.Add(room);
			}
		}
		if (list.Count != 0)
		{
			return list[Game.Instance.Random(list.Count)];
		}
		return null;
	}

	public void MemorizePosition(int x, int y)
	{
		Feature obj = _featureMap[x][y];
		Tile tile = Tile.GetTile(_tileMap[x][y]);
		char c = obj?.Character(this, x, y) ?? tile.Character(this, x, y);
		if (_memory[x][y] != c)
		{
			_memory[x][y] = c;
			Curses.Instance.SetCursorPosition(x, y + 2);
			Curses.Instance.Write(c);
		}
	}

	public void ThrowItem()
	{
		List<Item> allUnequippedItems = Grog.Inventory.GetAllUnequippedItems();
		if (allUnequippedItems.Count == 0)
		{
			Message(" does not own anything that could be thrown.");
			return;
		}
		ItemSelectionList list = new ItemSelectionList(allUnequippedItems, useAssociatedItemCharacters: true);
		ProcessItemSelectionList(() => GetThrowableInfos(list), list, ProcessThrowKey);
	}

	private List<string> GetThrowableInfos(ItemSelectionList list)
	{
		return new List<string> { "[" + GetCompressedInteractionRange(list.AssociatedKeys) + "] Throw item" };
	}

	private bool ProcessThrowKey(ItemSelectionList list, char c, bool isCtrlModified)
	{
		if (isCtrlModified)
		{
			return false;
		}
		Item itemAssociatedWith = list.GetItemAssociatedWith(c);
		if (itemAssociatedWith == null)
		{
			return false;
		}
		Direction direction = GetDirection("Throw the " + itemAssociatedWith.ShortDescription);
		if (direction != Direction.None)
		{
			var (xm, ym) = direction.GetDirectionalModifiers();
			ThrowItem(xm, ym, itemAssociatedWith);
		}
		else
		{
			ClearMessages();
			Render();
			Curses.Instance.Refresh();
		}
		return false;
	}

	private void ThrowItem(int xm, int ym, Item item)
	{
		ClearMessages();
		int x = Grog.X + xm;
		int y = Grog.Y + ym;
		if (!IsValid(x, y))
		{
			Message("Impossible!");
			return;
		}
		if (GetTileAt(x, y).IsSolid)
		{
			Message("Blocked!");
			return;
		}
		Grog.Inventory.Remove(item);
		int num = item.ThrowingRange - 1;
		x = Grog.X;
		y = Grog.Y;
		do
		{
			if (num > 0)
			{
				num--;
				if (GetTileAt(x + xm, y + ym).IsSolid)
				{
					break;
				}
				x += xm;
				y += ym;
			}
		}
		while (num > 0 && (GetThingAt(x, y) == null || !(GetThingAt(x, y) is Being)));
		DrawShot(item.Character, Grog.X + xm, Grog.Y + ym, xm, ym, x, y);
		Thing thingAt = GetThingAt(x, y);
		Being being = thingAt as Being;
		if (being != null)
		{
			Grog.MissileAttack(this, Grog, being, () => item.MissileDamage.GetDieResult(), (bool hit) =>
			{
				HandleHitOrMiss(hit, x, y, being, item);
			});
		}
		else if (Game.Instance.Random(100) >= item.ProbabilityToBreakOnMiss)
		{
			AddItem(x, y, item);
			if (item.ItemType == ItemType.MeleeWeapon)
			{
				item.IsIdentified = true;
			}
		}
		Grog.Moves++;
	}

	private void HandleHitOrMiss(bool hit, int x, int y, Being being, Item item)
	{
		if (hit)
		{
			if (Game.Instance.Random(100) >= item.ProbabilityToBreakOnHit)
			{
				AddItem(x, y, item);
			}
			else if (item.ItemType == ItemType.Potion && being.IsAlive)
			{
				item.UseAction(this, Grog, being, item);
			}
		}
		else if (Game.Instance.Random(100) >= item.ProbabilityToBreakOnMiss)
		{
			AddItem(x, y, item);
		}
	}

	public void Say(string text, bool more = false)
	{
		Message(Grog, "\"" + text + "\"", more);
	}

	public bool HasFeatureAt(Position position)
	{
		if (position.Equals(Position.Undefined))
		{
			return false;
		}
		return HasFeatureAt(position.X, position.Y);
	}

	public Tile GetTileAt(Position position)
	{
		if (position.Equals(Position.Undefined))
		{
			return Tile.Wall;
		}
		return GetTileAt(position.X, position.Y);
	}

	public bool HasItems(Position position)
	{
		if (position.Equals(Position.Undefined))
		{
			return false;
		}
		return HasItems(position.X, position.Y);
	}

	public bool IsBeingAt(Position position)
	{
		if (position.Equals(Position.Undefined))
		{
			return false;
		}
		return IsBeingAt(position.X, position.Y);
	}

	public bool IsMonsterAround(int x, int y)
	{
		if (!IsBeingAt(x - 1, y - 1) && !IsBeingAt(x - 1, y) && !IsBeingAt(x - 1, y + 1) && !IsBeingAt(x, y - 1) && !IsBeingAt(x, y + 1) && !IsBeingAt(x + 1, y - 1) && !IsBeingAt(x + 1, y))
		{
			return IsBeingAt(x + 1, y - 1);
		}
		return true;
	}

	public int SummonMonsters(DungeonLevel dungeonLevel, int ox, int oy, int range, int probabilityForGeneration, Func<Being> createBeing)
	{
		int num = 0;
		for (int i = ox - range; i <= ox + range; i++)
		{
			for (int j = oy - range; j <= oy + range; j++)
			{
				if (dungeonLevel.IsOpenForNewThing(i, j) && Game.Instance.Probability(probabilityForGeneration))
				{
					Being being = createBeing();
					dungeonLevel.SetBeing(i, j, being);
					num++;
				}
			}
		}
		return num;
	}

	public bool IsRoomAt(int x, int y)
	{
		return GetRoomAt(x, y) != null;
	}

	public void PrepareScene()
	{
		Render();
		for (int i = 2; i < 2 + Height; i++)
		{
			Curses.Instance.SetCursorPosition(0, i);
			Curses.Instance.ClearToEndOfLine();
		}
	}

	public ITrap GetOrCreateTrapAt(int x, int y)
	{
		Position key = new Position(x, y);
		if (_traps.ContainsKey(key))
		{
			return _traps[key];
		}
		ITrap trap = Game.Instance.TrapSystem.CreateTrapFor(this, x, y);
		_traps[key] = trap;
		return trap;
	}

	public void RemoveTrapAt(int x, int y)
	{
		if (!IsTrapAt(x, y))
		{
			Game.RaiseError("Trying to remove a non-existant trap at " + x + ", " + y + ".");
		}
		else
		{
			_featureMap[x][y] = null;
			_traps.Remove(new Position(x, y));
		}
	}

	public bool AggravateMonsters(Player grog)
	{
		bool flag = false;
		foreach (Being allBeing in GetAllBeings())
		{
			if (allBeing != grog && allBeing.Mood != Mood.Hunting && allBeing.Mood != Mood.WaitForPlayer && allBeing.Mood != Mood.FightStatically)
			{
				allBeing.Mood = Mood.Hunting;
				flag = true;
			}
		}
		if (flag)
		{
			Message(" feels like an endangered species!");
			return true;
		}
		Message(" feels a brief moment of relief.");
		return false;
	}

	public char GetWallCharacter(int x, int y)
	{
		IRoom roomAt = GetRoomAt(x, y);
		if (roomAt == null)
		{
			return ' ';
		}
		if (!roomAt.IsInsideOfRoom(x - 1, y) && !roomAt.IsInsideOfRoom(x + 1, y))
		{
			return '-';
		}
		return '|';
	}

	public void SetFeatureAt(Position position, Feature feature)
	{
		SetFeatureAt(position.X, position.Y, feature);
	}

	public void ReactToTurnSpentBy(Player grog)
	{
		GetFeatureAt(grog.X, grog.Y)?.WhenSpendingTurnOnMapElement?.Interact(this, grog);
		GetTileAt(grog.X, grog.Y)?.WhenSpendingTurnOnMapElement?.Interact(this, grog);
		if (grog.IsAffectedBySpecialRoomPower(SpecialRoomPower.SlashieRoom))
		{
			ReactToSlashing(grog);
		}
	}

	private void ReactToSlashing(Player grog)
	{
		if (Game.Instance.Probability(20))
		{
			Message("Some of the tiny swirling blades target " + grog.Name + "!");
			if (Game.Instance.Random(30) > grog.ArmorClass)
			{
				List<Item> equippedItemsOfType = grog.Inventory.GetEquippedItemsOfType(ItemType.Armor);
				if (equippedItemsOfType.Count > 0)
				{
					Item item = equippedItemsOfType[Game.Instance.Random(equippedItemsOfType.Count)];
					if (item.IsMetallic)
					{
						Message("The blades bounce off the " + item.ShortDescription + ".");
						return;
					}
					Message("The " + item.ShortDescription + " of " + grog.Name + " is shredded to pieces by the tiny blades!", more: true);
					grog.Inventory.Remove(item);
				}
				else
				{
					int total = Game.Instance.Roll(1, 4, Level / 4);
					Message(" receives a slashie for " + total + " damage!");
					grog.SufferDamage(this, null, total, "by being 'slashied' to pieces");
				}
			}
			else
			{
				Message("They barely zip by and miss!");
			}
		}
		else if (grog.IsBlind != 0)
		{
			Message(" notices swishing noises!");
		}
		else
		{
			Message("Small swirling blades run amok here slashing everything to shreds that gets into their way!");
		}
	}

	public bool IsValid(Position next)
	{
		return IsValid(next.X, next.Y);
	}

	public void SetTile(Position position, Tile tile)
	{
		SetTile(position.X, position.Y, tile);
	}

	public void InitiateComplexInteraction(string title, List<Interaction> interactions)
	{
		InitiateComplexInteraction(title, interactions.ToArray());
	}

	private Direction GetDirection(string message)
	{
		ClearMessages();
		Render();
		Curses.Instance.SetCursorPosition(0, 0);
		Curses.Instance.Write(message + " to which direction [wasd]? ");
		switch (Curses.Instance.ReadKey().Key)
		{
		case ConsoleKey.UpArrow:
		case ConsoleKey.W:
			Curses.Instance.Write("North.");
			Curses.Instance.Refresh();
			Render();
			Curses.Instance.Refresh();
			return Direction.MinDirection;
		case ConsoleKey.LeftArrow:
		case ConsoleKey.A:
			Curses.Instance.Write("West.");
			Curses.Instance.Refresh();
			Render();
			Curses.Instance.Refresh();
			return Direction.West;
		case ConsoleKey.DownArrow:
		case ConsoleKey.S:
			Curses.Instance.Write("South.");
			Curses.Instance.Refresh();
			Render();
			Curses.Instance.Refresh();
			return Direction.South;
		case ConsoleKey.RightArrow:
		case ConsoleKey.D:
			Curses.Instance.Write("East.");
			Curses.Instance.Refresh();
			Render();
			Curses.Instance.Refresh();
			return Direction.East;
		default:
			ClearMessages();
			Message("-");
			Render();
			return Direction.None;
		}
	}

	public Being GetBeingAt(int x, int y)
	{
		return GetThingAt(x, y) as Being;
	}

	public bool ZapEffect(string message, int x, int y, int range, Func<int, int, bool, Tile, Feature, Being, List<Item>, bool> executeEffectOnPosition)
	{
		Direction direction = GetDirection(message);
		if (direction == Direction.None)
		{
			return false;
		}
		var (xm, ym) = direction.GetDirectionalModifiers();
		DrawRay(x, y, xm, ym, range, executeEffectOnPosition);
		return true;
	}

	public void ResetAutoMore()
	{
		_isAutoMoreActive = false;
	}

	public bool IsOpenForNewThing(Position position)
	{
		return IsOpenForNewThing(position.X, position.Y);
	}
}
