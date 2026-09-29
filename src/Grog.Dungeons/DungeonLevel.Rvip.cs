/* RVIP stage 3: item cursor in the one item prompt (ProcessItemSelectionList), inventory main
   actions + item menu (direct calls of the game's own Process*Key functions), message log for 3d. */
using System;
using System.Collections.Generic;
using Grog.Dressings.Items;
using Grog.Dressings.Items.Inventory;
using Grog.Kernel.GCurses;

namespace Grog.Dungeons;

public partial class DungeonLevel
{
	[NonSerialized] static int _itemCursor;
	[NonSerialized] static ConsoleKeyInfo _lastItemKey;
	[NonSerialized] static bool _inInventory;

	// 3d: --more-- never waits with a backend (web, headless); messages kept here for a message window.
	public static bool AutoMore => Term.Backend != null;
	public static readonly List<string> MessageLog = new List<string>();
	static void LogMessage(string m)
	{
		if (string.IsNullOrEmpty(m)) return;
		MessageLog.Add(char.ToUpper(m[0]) + m.Substring(1));
		if (MessageLog.Count > 200) MessageLog.RemoveAt(0);
	}

	static readonly ConsoleKeyInfo Esc = new ConsoleKeyInfo('\x1b', ConsoleKey.Escape, false, false, false);

	// returns (nav: redraw only, stop: close list, c, ctrl) for keys the cursor handles; others unchanged
	(bool, bool, char, bool) ItemCursorKey(ItemSelectionList list, ConsoleKeyInfo k, char c, bool ctrl, bool paged)
	{
		int n = list.Count;
		char letter = list.CharacterAssociatedWith(list[_itemCursor]);
		switch (k.Key)
		{
		case ConsoleKey.UpArrow: case ConsoleKey.NumPad8: _itemCursor = (_itemCursor + n - 1) % n; return (true, false, c, ctrl);
		case ConsoleKey.DownArrow: case ConsoleKey.NumPad2: _itemCursor = (_itemCursor + 1) % n; return (true, false, c, ctrl);
		case ConsoleKey.NumPad4: case ConsoleKey.NumPad6: return (true, false, c, ctrl); // one list only in Grog (no equipment/floor lists)
		case ConsoleKey.NumPad0: return (false, false, '\x1b', false);
		}
		bool choose = k.Key == ConsoleKey.Enter || k.Key == ConsoleKey.NumPad5 || (c == ' ' && !paged);
		if (choose)
		{
			if (!_inInventory) return (false, false, letter, false);
			bool keep = InventoryItemMenu(list, list[_itemCursor]);
			return (keep, !keep, c, ctrl);
		}
		if (_inInventory)
		{
			if (c == '+') return (false, false, letter, false);
			if (c == '-') return (false, false, char.ToUpper(letter), false);
			if (c == '*') { Examine(list[_itemCursor]); return (true, false, c, ctrl); }
			if (c == '.') return (false, false, '\x1b', false);
		}
		return (false, false, c, ctrl);
	}

	// inventory letter = main action (equip/unequip, else use, else examine), Shift = drop, Ctrl = use; other keys run as commands
	bool ProcessInventoryMain(ItemSelectionList list, char c, bool ctrl)
	{
		if (c == '\x1b') return false;
		Item it = list.GetItemAssociatedWith(char.ToLower(c));
		if (it == null && c != '$') { Term.Push(_lastItemKey); return false; }
		int moves = Grog.Moves;
		bool keep;
		if (!ctrl && it != null && char.IsLower(c) && !it.IsEquippable)
		{
			if (it.UseAction == null) { Examine(it); return true; }
			keep = ProcessUseKey(list, c, true);
		}
		else keep = ProcessInventoryKey(list, c, ctrl);
		return AfterItemAction(moves, keep);
	}

	// an action took a turn: close the list so the turn runs, reopen it (queued i) unless a monster is in view
	bool AfterItemAction(int moves, bool keep)
	{
		if (Grog.Moves == moves) return keep;
		if (Grog.IsAlive && Grog.Inventory.ItemsCarried > 0 && GetAllBeingsVisibleTo(Grog).Count == 0)
			Term.Push(new ConsoleKeyInfo('i', ConsoleKey.I, false, false, false));
		return false;
	}

	void Examine(Item it)
	{
		Message(it.Description + (it.IsEquipped ? " (equipped)" : "") + ".");
	}

	// Enter/Space/5 on an item: every action that fits, each with its key. Returns true = keep the list open.
	bool InventoryItemMenu(ItemSelectionList list, Item it)
	{
		char l = list.CharacterAssociatedWith(it);
		var e = new List<PopupEntry>();
		var act = new List<Func<bool>>();
		if (it.UseAction != null) { e.Add(new PopupEntry("u", "Use (^" + l + ")", PopupEntry.Char('u'))); act.Add(() => ProcessInventoryMain(list, l, true)); }
		if (it.IsEquippable) { e.Add(new PopupEntry("e", (it.IsEquipped ? "Unequip" : "Equip") + " (" + l + ")", PopupEntry.Char('e'))); act.Add(() => ProcessInventoryMain(list, l, false)); }
		if (!it.IsEquipped) e.Add(new PopupEntry("t", "Throw", PopupEntry.Char('t')));
		if (!it.IsEquipped) act.Add(() =>
		{
			int moves = Grog.Moves;
			bool keep = ProcessThrowKey(new ItemSelectionList(Grog.Inventory.GetAllUnequippedItems(), useAssociatedItemCharacters: true), l, false);
			return AfterItemAction(moves, keep);
		});
		e.Add(new PopupEntry("d", "Drop (" + char.ToUpper(l) + ")", PopupEntry.Char('d'))); act.Add(() => ProcessInventoryMain(list, char.ToUpper(l), false));
		e.Add(new PopupEntry("x", "Examine (*)", k => k.KeyChar == 'x' || k.KeyChar == '*')); act.Add(() => { Examine(it); return true; });
		int n = Popup.Choose(l + " - " + it.ShortDescription, e);
		if (n < 0) return true;
		return act[n]();
	}
}
