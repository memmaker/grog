using System;
using System.Collections.Generic;
using Grog.Dressings.Beings;
using Grog.Dressings.Features;
using Grog.Dressings.Items.Types;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Monsters;
using Grog.Dungeons.Generators.Rooms;
using Grog.Kernel;
using Grog.Systems.Ghosts;
using Grog.Systems.Revenge;

namespace Grog.Dressings.Items.Implementations;

[Serializable]
public class WandImplementations
{
	private bool ConsumeCharge(Item wand)
	{
		wand.Charges--;
		return wand.Charges == 0;
	}

	public bool ZapWandOfLight(DungeonLevel dungeonLevel, Being applier, Being user, Item wand)
	{
		if (applier is Player player)
		{
			IRoom roomAt = dungeonLevel.GetRoomAt(player.X, player.Y);
			if (roomAt == null || !roomAt.IsDark)
			{
				dungeonLevel.Message("The " + wand.ShortDescription + " briefly emits a flash of light!");
			}
			else
			{
				dungeonLevel.Message("The " + wand.ShortDescription + " permanently lights up the room!", more: true);
				roomAt.IsDark = false;
				dungeonLevel.Render();
			}
			Game.Instance.ItemPool.Identify(wand);
			return ConsumeCharge(wand);
		}
		return false;
	}

	public bool ZapWandOfInvisibility(DungeonLevel dungeonLevel, Being applier, Being user, Item wand)
	{
		if (applier is Player player && dungeonLevel.ZapEffect("Zap the wand", player.X, player.Y, 4, (int x, int y, bool first, Tile t, Feature f, Being b, List<Item> i) =>
		{
			if (first)
			{
				dungeonLevel.Message(" fires a translucent ray from the wand.");
			}
			if (b != null)
			{
				if (!b.IsInvisible)
				{
					dungeonLevel.Message(b, " suddenly vanishes!");
					Game.Instance.ItemPool.Identify(wand);
				}
				if (b.TurnsOfInvisibility >= 0)
				{
					b.TurnsOfInvisibility += Game.Instance.Roll(4, 6);
				}
			}
			return !t.IsSolid;
		}))
		{
			return ConsumeCharge(wand);
		}
		return false;
	}

	public bool ZapWandOfLightning(DungeonLevel dungeonLevel, Being applier, Being user, Item wand)
	{
		if (applier is Player player && dungeonLevel.ZapEffect("Zap the wand", player.X, player.Y, 6, (int x, int y, bool first, Tile t, Feature f, Being b, List<Item> i) =>
		{
			if (first)
			{
				dungeonLevel.Message(" fires a wand of lightning from the wand!");
			}
			if (b != null)
			{
				int total = Game.Instance.Roll(6, 6);
				dungeonLevel.Message(b, " is shocked for " + total + " points of damage!");
				b.SufferDamage(dungeonLevel, applier, total, "by a lightning wand blast");
			}
			DestroyItems(dungeonLevel, x, y, i, (Item item) => item.IsMetallic && Game.Instance.Probability(18), "is shattered");
			return !t.IsSolid;
		}))
		{
			Game.Instance.ItemPool.Identify(wand);
			return ConsumeCharge(wand);
		}
		return false;
	}

	private void DestroyItems(DungeonLevel dungeonLevel, int x, int y, List<Item> items, Func<Item, bool> willBeDestroyed, string destructionMessage)
	{
		if (items == null || items.Count == 0)
		{
			return;
		}
		foreach (Item item in items)
		{
			if (willBeDestroyed(item))
			{
				if (dungeonLevel.Grog.CanSee(dungeonLevel, x, y))
				{
					dungeonLevel.Message("The " + item.ShortDescription + " " + destructionMessage + "!");
				}
				dungeonLevel.RemoveItemAt(x, y, item);
			}
		}
	}

	public bool ZapWandOfFire(DungeonLevel dungeonLevel, Being applier, Being user, Item wand)
	{
		if (applier is Player player && dungeonLevel.ZapEffect("Zap the wand", player.X, player.Y, 6, (int x, int y, bool first, Tile t, Feature f, Being b, List<Item> i) =>
		{
			if (first)
			{
				dungeonLevel.Message(" emits a fiery blast from the wand!");
			}
			if (b != null)
			{
				if (b.IsImmuneToFire)
				{
					dungeonLevel.Message(b, " resists the fiery blast!");
				}
				else
				{
					int total = Game.Instance.Roll(6, 6);
					dungeonLevel.Message(b, " is burned for " + total + " points of damage!");
					b.SufferDamage(dungeonLevel, applier, total, "by a fire wand blast");
				}
			}
			DestroyItems(dungeonLevel, x, y, i, (Item item) => item.ItemType == ItemType.Scroll || Game.Instance.Probability(18), "is burned away");
			return !t.IsSolid;
		}))
		{
			Game.Instance.ItemPool.Identify(wand);
			return ConsumeCharge(wand);
		}
		return false;
	}

	public bool ZapWandOfCold(DungeonLevel dungeonLevel, Being applier, Being user, Item wand)
	{
		if (applier is Player player && dungeonLevel.ZapEffect("Zap the wand", player.X, player.Y, 6, (int x, int y, bool first, Tile t, Feature f, Being b, List<Item> i) =>
		{
			if (first)
			{
				dungeonLevel.Message(" emits a blizzard for deadly ice from the wand!");
			}
			if (b != null)
			{
				if (b.MonsterType == MonsterPool.MonsterType.Yeti)
				{
					dungeonLevel.Message(b, " resists the deadly cold!");
				}
				else
				{
					int total = Game.Instance.Roll(6, 6);
					dungeonLevel.Message(b, " is frozen for " + total + " points of damage!");
					b.SufferDamage(dungeonLevel, applier, total, "by a cold wand blast");
				}
			}
			DestroyItems(dungeonLevel, x, y, i, (Item item) => item.ItemType == ItemType.Potion || Game.Instance.Probability(5), "is shattered by the biting cold");
			return !t.IsSolid;
		}))
		{
			Game.Instance.ItemPool.Identify(wand);
			return ConsumeCharge(wand);
		}
		return false;
	}

	public bool ZapWandOfPolymorph(DungeonLevel dungeonLevel, Being applier, Being user, Item wand)
	{
		if (applier is Player player && dungeonLevel.ZapEffect("Zap the wand", player.X, player.Y, 2, (int x, int y, bool first, Tile t, Feature f, Being b, List<Item> i) =>
		{
			if (first)
			{
				dungeonLevel.Message(" emits a ray of scintillating colors!");
			}
			if (b != null)
			{
				if (b.MonsterType == MonsterPool.MonsterType.Lich || b is Ghost || b is RevengeMonster)
				{
					dungeonLevel.Message(b, " resists the weird ray!");
				}
				else
				{
					Game.Instance.ItemPool.Identify(wand);
					Being being = Game.Instance.MonsterPool.CreateRandomMonster(dungeonLevel);
					dungeonLevel.Message(b, " transforms into " + Game.Instance.a(being.Type) + "!");
					dungeonLevel.RemoveThing(x, y);
					dungeonLevel.SetBeing(x, y, being);
				}
				_ = t.IsSolid;
				return false;
			}
			return true;
		}))
		{
			return ConsumeCharge(wand);
		}
		return false;
	}

	public bool ZapWandOfMagicMissiles(DungeonLevel dungeonLevel, Being applier, Being user, Item wand)
	{
		if (applier is Player player && dungeonLevel.ZapEffect("Zap the wand", player.X, player.Y, 8, (int x, int y, bool first, Tile t, Feature f, Being b, List<Item> i) =>
		{
			if (first)
			{
				dungeonLevel.Message(" fires a magic missile from the tip of the wand!");
			}
			if (b != null)
			{
				int total = Game.Instance.Roll(1, 6, 1);
				dungeonLevel.Message(b, " is zapped for " + total + " points of damage!");
				b.SufferDamage(dungeonLevel, applier, total, "by a magic missile");
				return false;
			}
			return !t.IsSolid;
		}))
		{
			Game.Instance.ItemPool.Identify(wand);
			return ConsumeCharge(wand);
		}
		return false;
	}

	public bool ZapWandOfHasteMonster(DungeonLevel dungeonLevel, Being applier, Being user, Item wand)
	{
		if (applier is Player player && dungeonLevel.ZapEffect("Zap the wand", player.X, player.Y, 2, (int x, int y, bool first, Tile t, Feature f, Being b, List<Item> i) =>
		{
			if (first)
			{
				dungeonLevel.Message(" emits a quicksilver ray!");
			}
			if (b != null)
			{
				dungeonLevel.Message(b, " seems to speed up!");
				b.IsHastened += Game.Instance.Roll(4, 4);
				Game.Instance.ItemPool.Identify(wand);
			}
			return !t.IsSolid;
		}))
		{
			return ConsumeCharge(wand);
		}
		return false;
	}

	public bool ZapWandOfSlowMonster(DungeonLevel dungeonLevel, Being applier, Being user, Item wand)
	{
		if (applier is Player player && dungeonLevel.ZapEffect("Zap the wand", player.X, player.Y, 2, (int x, int y, bool first, Tile t, Feature f, Being b, List<Item> i) =>
		{
			if (first)
			{
				dungeonLevel.Message(" emits a gooey ray!");
			}
			if (b != null)
			{
				dungeonLevel.Message(b, " seems to slow down!");
				b.IsHastened -= Game.Instance.Roll(4, 4);
				Game.Instance.ItemPool.Identify(wand);
			}
			return !t.IsSolid;
		}))
		{
			return ConsumeCharge(wand);
		}
		return false;
	}

	public bool ZapWandOfDrainLife(DungeonLevel dungeonLevel, Being applier, Being user, Item wand)
	{
		if (applier is Player player)
		{
			Game.Instance.ItemPool.Identify(wand);
			int num = player.HitPoints / 2;
			dungeonLevel.Message("The wand emits a circular blast of necrotic energy!");
			List<Being> allBeingsVisibleTo = dungeonLevel.GetAllBeingsVisibleTo(player);
			int num2 = num / Math.Max(1, allBeingsVisibleTo.Count);
			if (num2 < 1 || allBeingsVisibleTo.Count == 0)
			{
				dungeonLevel.Message("It evaporates without any noticeable effects!");
			}
			else
			{
				foreach (Being item in allBeingsVisibleTo)
				{
					item.SufferDamage(dungeonLevel, player, num2, "by a mighty necrotic blast");
				}
			}
			player.SufferDamage(dungeonLevel, player, num, "by zapping a wand of life drain");
			return ConsumeCharge(wand);
		}
		return false;
	}

	public bool ZapWandOfNothing(DungeonLevel dungeonLevel, Being applier, Being user, Item wand)
	{
		if (applier is Player)
		{
			Game.Instance.ItemPool.Identify(wand);
			dungeonLevel.Message("Nothing happens.");
			return ConsumeCharge(wand);
		}
		return false;
	}

	public bool ZapWandOfTeleportAway(DungeonLevel dungeonLevel, Being applier, Being user, Item wand)
	{
		if (applier is Player player && dungeonLevel.ZapEffect("Zap the wand", player.X, player.Y, 1, (int x, int y, bool first, Tile t, Feature f, Being b, List<Item> i) =>
		{
			if (first)
			{
				dungeonLevel.Message(" emits a particle beam!");
			}
			if (b != null && dungeonLevel.Teleport(b))
			{
				dungeonLevel.Message(b, " is teleported away!");
				Game.Instance.ItemPool.Identify(wand);
			}
			return !t.IsSolid;
		}))
		{
			return ConsumeCharge(wand);
		}
		return false;
	}

	public bool ZapWandOfTeleportBy(DungeonLevel dungeonLevel, Being applier, Being user, Item wand)
	{
		Player grog = applier as Player;
		if (grog != null && dungeonLevel.ZapEffect("Zap the wand", grog.X, grog.Y, 6, (int x, int y, bool first, Tile t, Feature f, Being b, List<Item> i) =>
		{
			if (first)
			{
				dungeonLevel.Message(" emits a black ray!");
			}
			if (b != null)
			{
				Position newPosition = dungeonLevel.FindPositionForMonsterCreationAround(grog);
				if (!newPosition.IsUndefined && dungeonLevel.Teleport(b, newPosition))
				{
					dungeonLevel.Message(b, " is recalled to " + grog.Name + "!");
					Game.Instance.ItemPool.Identify(wand);
					return false;
				}
			}
			return !t.IsSolid;
		}))
		{
			return ConsumeCharge(wand);
		}
		return false;
	}

	public bool ZapWandOfCancellation(DungeonLevel dungeonLevel, Being applier, Being user, Item wand)
	{
		if (applier is Player player && dungeonLevel.ZapEffect("Zap the wand", player.X, player.Y, 1, (int x, int y, bool first, Tile t, Feature f, Being b, List<Item> i) =>
		{
			if (first)
			{
				dungeonLevel.Message(" emits a gray beam!");
			}
			if (b != null)
			{
				if (b.MonsterType == MonsterPool.MonsterType.Lich)
				{
					dungeonLevel.Message(b, " shrugs off the power of the beam with a rasping laugh!");
				}
				else if (b.SpecialAbility != SpecialAbility.None)
				{
					b.SpecialAbility = SpecialAbility.None;
					dungeonLevel.Message(b, " appears weakened!");
					Game.Instance.ItemPool.Identify(wand);
				}
				else
				{
					dungeonLevel.Message(b, " seems unaffected.");
				}
			}
			return !t.IsSolid;
		}))
		{
			return ConsumeCharge(wand);
		}
		return false;
	}

	public bool ZapWandOfMidas(DungeonLevel dungeonLevel, Being applier, Being user, Item wand)
	{
		Player grog = applier as Player;
		if (grog != null && dungeonLevel.ZapEffect("Zap the wand", grog.X, grog.Y, 6, (int x, int y, bool first, Tile t, Feature f, Being b, List<Item> i) =>
		{
			if (first)
			{
				dungeonLevel.Message(" emits a ray of golden hue!");
			}
			if (i != null && i.Count > 0)
			{
				int num = 0;
				foreach (Item item in i)
				{
					num += item.Value;
				}
				num *= 2;
				if (!dungeonLevel.HasFeatureAt(x, y) || dungeonLevel.GetFeatureAt(x, y) is GoldFeature)
				{
					if (grog.CanSee(dungeonLevel, x, y))
					{
						if (i.Count == 1)
						{
							dungeonLevel.Message("The " + i[0].ShortDescription + " is turned to gold!");
						}
						else
						{
							dungeonLevel.Message("The items on the ground are turned into gold!");
						}
						Game.Instance.ItemPool.Identify(wand);
					}
					if (!dungeonLevel.HasFeatureAt(x, y))
					{
						dungeonLevel.SetFeatureAt(x, y, new GoldFeature(num));
					}
					else
					{
						(dungeonLevel.GetFeatureAt(x, y) as GoldFeature)?.IncreaseInValueBy(num);
					}
					foreach (Item item2 in i)
					{
						dungeonLevel.RemoveItemAt(x, y, item2);
					}
					Game.Instance.ItemPool.Identify(wand);
				}
			}
			return !t.IsSolid;
		}))
		{
			return ConsumeCharge(wand);
		}
		return false;
	}

	public bool ZapWandOfDigging(DungeonLevel dungeonLevel, Being applier, Being user, Item wand)
	{
		Player grog = applier as Player;
		if (grog != null && dungeonLevel.ZapEffect("Zap the wand", grog.X, grog.Y, 6, (int x, int y, bool first, Tile t, Feature f, Being b, List<Item> i) =>
		{
			if (first)
			{
				dungeonLevel.Message(" emits a ray of dust!");
			}
			if (x > 0 && y > 0 && x < dungeonLevel.Width - 1 && y < dungeonLevel.Height - 1)
			{
				if (t.IsSolid)
				{
					if (t == Tile.Wall)
					{
						dungeonLevel.SetTile(x, y, Tile.Tunnel);
						if (grog.CanSee(dungeonLevel, x, y))
						{
							dungeonLevel.Message("The wall turns to dust as it is hit by the ray!");
							Game.Instance.ItemPool.Identify(wand);
						}
						else
						{
							dungeonLevel.Message(" hears a crushing sound!");
						}
					}
					else
					{
						IRoom roomAt = dungeonLevel.GetRoomAt(x, y);
						if (roomAt != null && roomAt.IsWallOfRoom(x, y))
						{
							dungeonLevel.SetTile(x, y, Tile.Door);
							if (grog.CanSee(dungeonLevel, x, y))
							{
								dungeonLevel.Message("The wall is transformed into a door as it is hit by the ray!");
								Game.Instance.ItemPool.Identify(wand);
							}
							else
							{
								dungeonLevel.Message(" hears a clicking sound!");
							}
						}
					}
				}
				return true;
			}
			return false;
		}))
		{
			return ConsumeCharge(wand);
		}
		return false;
	}

	public bool ZapWandOfWebbing(DungeonLevel dungeonLevel, Being applier, Being user, Item wand)
	{
		Player grog = applier as Player;
		if (grog != null)
		{
			bool firstWeb = true;
			if (dungeonLevel.ZapEffect("Zap the wand", grog.X, grog.Y, 1, (int x, int y, bool first, Tile t, Feature f, Being b, List<Item> i) =>
			{
				if (first)
				{
					dungeonLevel.Message(" emits silky strands!");
				}
				if (!dungeonLevel.HasFeatureAt(x, y) && dungeonLevel.GetTileAt(x, y) != Tile.Wall && dungeonLevel.GetTileAt(x, y) != Tile.WallOfRoom)
				{
					if (grog.CanSee(dungeonLevel, x, y))
					{
						if (firstWeb)
						{
							dungeonLevel.Message("Sticky spiderwebs cover the space!");
							Game.Instance.ItemPool.Identify(wand);
						}
						firstWeb = false;
					}
					dungeonLevel.SetFeatureAt(x, y, Feature.SpiderWeb);
				}
				return !t.IsSolid;
			}))
			{
				return ConsumeCharge(wand);
			}
		}
		return false;
	}
}
