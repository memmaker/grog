using System;
using System.Collections.Generic;
using Grog.Dressings.Beings;
using Grog.Dressings.Items;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Rooms;
using Grog.Kernel;
using Grog.Kernel.Interactions;
using Grog.Kernel.Representations;

namespace Grog.Dressings;

[Serializable]
public class Tile : Thing
{
	public class MoveIntoWallMoveIntoInteraction : IMoveIntoInteraction
	{
		public bool MoveInto(DungeonLevel dungeonLevel, Thing attacker, Thing target, int fx, int fy, int tx, int ty)
		{
			if ((!(attacker is Being being) || (being.SpecialAbility != SpecialAbility.Desolid && being.SpecialAbility != SpecialAbility.Burrow && being.SpecialAbility != SpecialAbility.PoisonAndBurrow)) && attacker is Being being2)
			{
				if ((being2.SpecialAbility == SpecialAbility.Burrow || being2.SpecialAbility == SpecialAbility.PoisonAndBurrow) && Game.Instance.Probability(being2.FirstAbilityValue))
				{
					dungeonLevel.Message(being2, " digs into the rock crushing it to small pebbles!");
					return true;
				}
				if (being2.AutomaticAction != null)
				{
					being2.ResetAutomaticAction();
					return false;
				}
				if (being2 is Player player)
				{
					if (player.Inventory.HasEquippedItemOfType(ItemId.PickAxe))
					{
						if (tx > 1 && ty > 1 && tx < dungeonLevel.Width - 1 && ty < dungeonLevel.Height - 1)
						{
							if (Game.Instance.Probability(Math.Max(0, 5 + ((player.Strength < 10) ? ((player.Strength - 10) / 2) : ((player.Strength - 10) * 2)))))
							{
								dungeonLevel.Message(" smashes the wall to pieces and manages to dig out a tunnel!");
								dungeonLevel.SetTile(tx, ty, Tunnel);
							}
							else
							{
								dungeonLevel.Message(" chisels at the wall and manages to shatter some parts of it. Carry on!");
							}
						}
						else
						{
							dungeonLevel.Message(" tries to dig into the wall with the pick axe but fails to make any progress!");
						}
					}
					else
					{
						dungeonLevel.Message(" smashes into the wall!");
					}
					return false;
				}
			}
			return false;
		}
	}

	[Serializable]
	public class TunnelCharacterRepresentation : ICharacterRepresentation
	{
		public char GetCharacterRepresentation(DungeonLevel dungeonLevel, int x, int y)
		{
			return '#';
		}
	}

	[Serializable]
	public class DisplayRoomSpecialMessageBeingInteraction : IBeingInteraction
	{
		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			if (being is Player player)
			{
				IRoom roomAt = dungeonLevel.GetRoomAt(player.X, player.Y);
				if (roomAt?.SpecialMessage != null)
				{
					dungeonLevel.Message(roomAt.SpecialMessage.GetMessageFor(dungeonLevel, roomAt));
				}
			}
			return true;
		}
	}

	[Serializable]
	public class WinBeingInteraction : IBeingInteraction
	{
		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			if (being is Player)
			{
				Game.Instance.Win();
			}
			return false;
		}
	}

	[Serializable]
	public class DescendLevelBeingInteraction : IBeingInteraction
	{
		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			if (being is Player)
			{
				Game.Instance.DungeonMaster.DescendLevel();
			}
			return false;
		}
	}

	[Serializable]
	public class AscendLevelBeingInteraction : IBeingInteraction
	{
		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			if (being is Player)
			{
				Game.Instance.DungeonMaster.AscendLevel();
			}
			return false;
		}
	}

	[Serializable]
	public class WallOfRoomCharacterRepresentation : ICharacterRepresentation
	{
		public char GetCharacterRepresentation(DungeonLevel dungeonLevel, int x, int y)
		{
			if (dungeonLevel.GetRoomAt(dungeonLevel.Grog.X, dungeonLevel.Grog.Y) != dungeonLevel.GetRoomAt(x, y))
			{
				return dungeonLevel.MemoryOf(x, y);
			}
			return dungeonLevel.GetWallCharacter(x, y);
		}
	}

	[Serializable]
	public class WallCharacterRepresentation : ICharacterRepresentation
	{
		public char GetCharacterRepresentation(DungeonLevel dungeonLevel, int x, int y)
		{
			if (Math.Abs(x - dungeonLevel.Grog.X) <= 1 && Math.Abs(y - dungeonLevel.Grog.Y) <= 1)
			{
				return dungeonLevel.GetWallCharacter(x, y);
			}
			return dungeonLevel.MemoryOf(x, y);
		}
	}

	private readonly ICharacterRepresentation _getCharacter;

	private readonly IItemProcessor _itemProcessor;

	private static readonly Dictionary<int, Tile> _tileByIndex = new Dictionary<int, Tile>();

	public static Tile Wall = new Tile(0, new WallCharacterRepresentation(), isSolid: true, null, null, null, null, null, new MoveIntoWallMoveIntoInteraction());

	public static Tile Floor = new Tile(1, '.', isSolid: false);

	public static Tile Tunnel = new Tile(2, new TunnelCharacterRepresentation(), isSolid: false);

	public static Tile Door = new Tile(5, '/', isSolid: false, "A door.", null, null, new DisplayRoomSpecialMessageBeingInteraction());

	public static Tile StairUp = new Tile(6, '<', isSolid: false, "A stair is leading upwards.", new AscendLevelBeingInteraction());

	public static Tile StairDown = new Tile(7, '>', isSolid: false, "A stair is leading downwards.", new DescendLevelBeingInteraction());

	public static Tile ThroneOfImmortality = new Tile(8, '*', isSolid: false, "The Throne of Immortality.", new WinBeingInteraction());

	public static Tile WallOfRoom = new Tile(9, new WallOfRoomCharacterRepresentation(), isSolid: true, null, null, null, null, null, new MoveIntoWallMoveIntoInteraction());

	public static Tile SecretDoor = new SecretDoor(10, new WallOfRoomCharacterRepresentation(), isSolid: true, new MoveIntoWallMoveIntoInteraction());

	public IBeingInteraction WhenEntering { get; }

	public int Index { get; }

	public string Description { get; }

	public IBeingInteraction Interaction { get; }

	public bool IsSolid { get; }

	public bool IsOpen => !IsSolid;

	public override char Character(DungeonLevel dungeonLevel, int x, int y)
	{
		return _getCharacter.GetCharacterRepresentation(dungeonLevel, x, y);
	}

	protected Tile(int index, char c, bool isSolid, string description = null, IBeingInteraction interaction = null, IItemProcessor itemProcessor = null, IBeingInteraction whenEntering = null, IBeingInteraction whenSpendingTurnOnMapElement = null, IMoveIntoInteraction moveInto = null)
		: this(index, new ConstantCharacterRepresentation(c), isSolid, description, interaction, itemProcessor, whenEntering, whenSpendingTurnOnMapElement, moveInto)
	{
	}

	protected Tile(int index, ICharacterRepresentation getCharacter, bool isSolid, string description = null, IBeingInteraction interaction = null, IItemProcessor itemProcessor = null, IBeingInteraction whenEntering = null, IBeingInteraction whenSpendingTurnOnMapElement = null, IMoveIntoInteraction moveInto = null)
		: base(' ', whenSpendingTurnOnMapElement)
	{
		Index = index;
		IsSolid = isSolid;
		Description = description;
		Interaction = interaction;
		WhenEntering = whenEntering;
		MoveInto = moveInto;
		_getCharacter = getCharacter;
		_itemProcessor = itemProcessor;
		_tileByIndex[Index] = this;
	}

	public Item GetProcessedItem(DungeonLevel dungeonLevel, Player grog, Item item)
	{
		if (_itemProcessor == null)
		{
			return item;
		}
		return _itemProcessor.Process(dungeonLevel, grog, item);
	}

	public static Tile GetTile(int index)
	{
		return _tileByIndex[index];
	}
}
