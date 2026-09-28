using System;
using System.Collections.Generic;
using System.Text;
using Grog.Dressings;
using Grog.Dressings.Beings;
using Grog.Dressings.Features;
using Grog.Dressings.Items;
using Grog.Dressings.Items.Types;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Rooms;
using Grog.Kernel;
using Grog.Kernel.GCurses;
using Grog.Kernel.Interactions;
using Grog.Systems.Ghosts;

namespace Grog;

public static class CheatMode
{
	public static void Cheat(DungeonLevel dungeonLevel, Player grog)
	{
		dungeonLevel.InitiateComplexInteraction("Select a cheat:", () => GetCheats(dungeonLevel, grog));
	}

	private static Interaction[] GetCheats(DungeonLevel dungeonLevel, Player grog)
	{
		List<Interaction> list = new List<Interaction>();
		list.Add(new Interaction('a', "Create altar", dungeonLevel, grog, grog.X, grog.Y, CreateAltar));
		list.Add(new Interaction('A', "Display ASCII table", dungeonLevel, grog, grog.X, grog.Y, DisplayAsciiTable));
		if (dungeonLevel.IsRoomAt(grog.X, grog.Y))
		{
			list.Add(new Interaction('c', "Create all items", dungeonLevel, grog, grog.X, grog.Y, CreateAllItems));
		}
		if (grog.Inventory.GetEquippedItems().Count > 0)
		{
			list.Add(new Interaction('C', "Curse all equipped items", dungeonLevel, grog, grog.X, grog.Y, CurseEquippedItems));
		}
		list.Add(new Interaction('e', "Create item with excessive name", dungeonLevel, grog, grog.X, grog.Y, CreateExcessiveItem));
		list.Add(new Interaction('g', "Create ghost from player", dungeonLevel, grog, grog.X, grog.Y, CreateGhostFromPlayer));
		list.Add(new Interaction('G', "Create ghost on level", dungeonLevel, grog, grog.X, grog.Y, CreateGhostOnLevel));
		list.Add(new Interaction('i', "Internal information", dungeonLevel, grog, grog.X, grog.Y, DisplayInformation));
		list.Add(new Interaction('m', "Map level", dungeonLevel, grog, grog.X, grog.Y, MapLevel));
		list.Add(new Interaction('S', "Strength + 10", dungeonLevel, grog, grog.X, grog.Y, IncreaseStrength));
		list.Add(new Interaction('t', "Teleport", dungeonLevel, grog, grog.X, grog.Y, Teleport));
		if (!dungeonLevel.IsTrapAt(grog.X, grog.Y) && !dungeonLevel.HasFeatureAt(grog.X, grog.Y))
		{
			list.Add(new Interaction('T', "Create trap", dungeonLevel, grog, grog.X, grog.Y, CreateTrap));
		}
		if (grog.Inventory.CanTake(1))
		{
			list.Add(new Interaction('w', "Wish for item", dungeonLevel, grog, grog.X, grog.Y, WishForItem));
		}
		if (!dungeonLevel.FindPositionOfTile(Tile.StairDown).Equals(Position.Undefined))
		{
			list.Add(new Interaction('>', "Level down", dungeonLevel, grog, grog.X, grog.Y, LevelDown));
			list.Add(new Interaction('=', "Go to specific level", dungeonLevel, grog, grog.X, grog.Y, GoToSpecificLevel));
		}
		list.Add(new Interaction('!', "Message excessively", dungeonLevel, grog, grog.X, grog.Y, MessageExcessively));
		list.Add(new Interaction('$', "Gain 10000 gold pieces", dungeonLevel, grog, grog.X, grog.Y, GainGold));
		if (grog.Level < 25)
		{
			list.Add(new Interaction('*', "Advance to maximum level", dungeonLevel, grog, grog.X, grog.Y, AdvanceLevels));
		}
		return list.ToArray();
	}

	private static bool IncreaseStrength(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		grog.Strength += 10;
		dungeonLevel.Message("Strengthth increased by 10 points.");
		return false;
	}

	private static bool DisplayAsciiTable(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		List<string> list = new List<string> { "--- ASCII Table ---" };
		StringBuilder stringBuilder = new StringBuilder("  ");
		for (int i = 0; i < 16; i++)
		{
			stringBuilder.Append(i.ToString("X")).Append(' ');
		}
		list.Add(stringBuilder.ToString());
		for (int j = 0; j < 16; j++)
		{
			StringBuilder stringBuilder2 = new StringBuilder(j.ToString("X")).Append(' ');
			for (int k = 0; k < 16; k++)
			{
				char c = (char)(j * 16 + k);
				if (char.IsControl(c))
				{
					c = ' ';
				}
				stringBuilder2.Append(c).Append(' ');
			}
			list.Add(stringBuilder2.ToString());
		}
		dungeonLevel.DisplayStringList(list);
		return false;
	}

	private static bool CreateTrap(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		grog.DidCheat = true;
		dungeonLevel.SetFeatureAt(x, y, new HiddenTrapFeature());
		dungeonLevel.Message("Trap created!");
		return false;
	}

	private static bool DisplayInformation(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		grog.DidCheat = true;
		List<string> list = new List<string>();
		Game.Instance.Ghosts.AddInformation(list);
		Game.Instance.RevengeSystem.AddInformation(list);
		Game.Instance.MonsterPool.AddInformation(list);
		Game.Instance.ItemPool.AddInformation(list);
		Game.Instance.ItemPool.ItemModifiers.AddInformation(list);
		Game.Instance.SpecialRoomSystem.AddInformation(list);
		list.Sort();
		for (int i = 0; i < list.Count; i++)
		{
			list[i] = "|  " + list[i];
		}
		dungeonLevel.Render();
		dungeonLevel.DisplayStringList(list);
		return false;
	}

	private static bool MapLevel(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		grog.DidCheat = true;
		dungeonLevel.MapLevel();
		return false;
	}

	private static bool Teleport(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		grog.DidCheat = true;
		dungeonLevel.Teleport(grog);
		dungeonLevel.Render();
		return false;
	}

	private static bool AdvanceLevels(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		grog.DidCheat = true;
		while (grog.Level < 25)
		{
			grog.GainExperience(dungeonLevel, grog.MissingExperiencePointsForNextLevel);
		}
		return false;
	}

	private static bool GainGold(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		grog.DidCheat = true;
		grog.Gold += 10000;
		return false;
	}

	private static bool MessageExcessively(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		grog.DidCheat = true;
		dungeonLevel.Render();
		dungeonLevel.Message(null, "The quick brown fox jumps over the lazy kobold. And the big fat ogre kills the huge swack iron dragon while said dragon slumbers on its hoard. Meanwhile Andor Drakon dreams about losing ChAoS upon Ancardia. And spoiling this peaceful world forever. Truly forever. And so it begins... again and again... until the end of time.", more: true);
		return false;
	}

	private static bool CreateGhostOnLevel(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		grog.DidCheat = true;
		List<IRoom> list = new List<IRoom>(dungeonLevel.Rooms);
		Position insidePositionForThing = (dungeonLevel.GetRoomAt(grog.X, grog.Y) ?? list[Game.Instance.Random(list.Count)]).GetInsidePositionForThing(dungeonLevel);
		bool flag = false;
		if (!insidePositionForThing.Equals(Position.Undefined) && dungeonLevel.GetThingAt(insidePositionForThing.X, insidePositionForThing.Y) == null)
		{
			Ghost ghost = Game.Instance.Ghosts.CreateGhostMonsterIfAppropriate(dungeonLevel, enforceGhostCreation: true);
			if (ghost != null)
			{
				dungeonLevel.SetBeing(insidePositionForThing.X, insidePositionForThing.Y, ghost);
				flag = true;
			}
		}
		if (flag)
		{
			dungeonLevel.Message("Ghost created (" + insidePositionForThing.X + ", " + insidePositionForThing.Y + ").", more: true);
		}
		else
		{
			dungeonLevel.Message("Failed to create a ghost...");
		}
		return false;
	}

	private static bool CreateGhostFromPlayer(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		grog.DidCheat = true;
		Game.Instance.Ghosts.CheckForGhostCreation(dungeonLevel, grog, null, enforceGhostCreation: true);
		return false;
	}

	private static bool CreateAllItems(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		grog.DidCheat = true;
		int num = 0;
		int num2 = 0;
		IRoom roomAt = dungeonLevel.GetRoomAt(x, y);
		foreach (ItemId value in Enum.GetValues(typeof(ItemId)))
		{
			if (!roomAt.GetInsidePositionForItem(dungeonLevel).Equals(Position.Undefined))
			{
				dungeonLevel.AddItem(grog.X, grog.Y, Game.Instance.ItemPool.CreateItem(value));
				num++;
			}
			num2++;
		}
		dungeonLevel.Message("Created " + num + " out of " + num2 + " items.", more: true);
		return false;
	}

	private static bool CreateExcessiveItem(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		grog.DidCheat = true;
		if (!dungeonLevel.GetRoomAt(x, y).GetInsidePositionForItem(dungeonLevel).Equals(Position.Undefined))
		{
			Item item = Game.Instance.ItemPool.CreateItem(ItemId.TwoHandedSword);
			Game.Instance.ItemPool.ItemModifiers.ModifyItemForExcessivelyLongName(item);
			Game.Instance.ItemPool.Identify(item);
			dungeonLevel.AddItem(grog.X, grog.Y, item);
			dungeonLevel.Message("Created " + item.a() + ".", more: true);
		}
		return false;
	}

	private static bool CurseEquippedItems(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		grog.DidCheat = true;
		foreach (Item equippedItem in grog.Inventory.GetEquippedItems())
		{
			equippedItem.IsCursed = true;
		}
		dungeonLevel.Message("Your equipment now is cursed.", more: true);
		return false;
	}

	private static bool WishForItem(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		grog.DidCheat = true;
		List<Interaction> list = new List<Interaction>();
		int num = 0;
		foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
		{
			list.Add(new Interaction(num, "Type: " + itemType, dungeonLevel, grog, x, y, (DungeonLevel dl, Player g, int xp, int yp) => SelectItemTypeForItemWish(dl, g, xp, yp, itemType)));
			num++;
		}
		dungeonLevel.InitiateComplexInteraction("Select an item type to wish for:", list.ToArray());
		return false;
	}

	private static bool SelectItemTypeForItemWish(DungeonLevel dungeonLevel, Player grog, int x, int y, ItemType itemType)
	{
		grog.DidCheat = true;
		ListOfItemDefinitions itemDefinitionsByType = Game.Instance.ItemPool.GetItemDefinitionsByType(itemType);
		List<Interaction> list = new List<Interaction>();
		char c = 'a';
		foreach (ItemDefinition definition in itemDefinitionsByType)
		{
			list.Add(new Interaction(c, definition.Template.Name, dungeonLevel, grog, x, y, (DungeonLevel dl, Player g, int xp, int yp) => FulfillItemWish(dl, g, xp, yp, definition)));
			c = (char)(c + 1);
		}
		dungeonLevel.InitiateComplexInteraction("Select an item to wish for:", list.ToArray());
		return true;
	}

	private static bool FulfillItemWish(DungeonLevel dungeonLevel, Player grog, int x, int y, ItemDefinition definition)
	{
		grog.DidCheat = true;
		Item item = definition.CreateItem();
		Game.Instance.ItemPool.Identify(item);
		dungeonLevel.Message(item.A() + " is added to your inventory.", more: true);
		grog.Inventory.Add(item);
		return true;
	}

	private static bool LevelDown(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		grog.DidCheat = true;
		Position position = dungeonLevel.FindPositionOfTile(Tile.StairDown);
		dungeonLevel.GetTileAt(position).Interaction.Interact(dungeonLevel, grog);
		Game.Instance.DungeonMaster.CurrentDungeonLevel.Message("Descended a level.");
		Game.Instance.DungeonMaster.CurrentDungeonLevel.Render();
		return true;
	}

	private static bool GoToSpecificLevel(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		grog.DidCheat = true;
		Curses.Instance.SetCursorPosition(0, 0);
		Curses.Instance.ClearToEndOfLine();
		Curses.Instance.Write("To which level [1-" + 25 + "]? ");
		if (int.TryParse(Curses.Instance.ReadLine(2), out var result) && result >= 1 && result <= 25)
		{
			Game.Instance.DungeonMaster.SetUpDungeonLevel(result, Tile.StairUp);
			return true;
		}
		return false;
	}

	private static bool CreateAltar(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		grog.DidCheat = true;
		dungeonLevel.SetFeatureAt(x, y, new AltarFeature(dungeonLevel));
		dungeonLevel.Message("Altar created.");
		return false;
	}
}
