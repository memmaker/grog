using System;
using Grog.Dressings.Beings;
using Grog.Dressings.Items;
using Grog.Dressings.Items.Types;
using Grog.Dressings.MapElements;
using Grog.Dungeons;
using Grog.Kernel;
using Grog.Kernel.Interactions;
using Grog.Kernel.Representations;

namespace Grog.Dressings;

[Serializable]
public class Feature : MapElementBase
{
	[Serializable]
	public class DigUpGraveInteraction : IInteractionInteraction
	{
		public void Interact(DungeonLevel dungeonLevel, Player grog, int x, int y)
		{
			if (!dungeonLevel.Sure("dig up the grave"))
			{
				return;
			}
			if (Game.Instance.Probability(50))
			{
				Being being = Game.Instance.MonsterPool.CreateRandomMonster(dungeonLevel, Race.Undead);
				if (being == null)
				{
					dungeonLevel.Message("The grave appears to be completely empty...");
				}
				else
				{
					Position positionForThingAround = dungeonLevel.GetRoomAt(x, y).GetPositionForThingAround(dungeonLevel, x, y);
					if (!positionForThingAround.Equals(Position.Undefined))
					{
						dungeonLevel.SetBeing(positionForThingAround.X, positionForThingAround.Y, being);
						being.Mood = Mood.Hunting;
						dungeonLevel.Message(being, grog.Name + " is attacked by the " + being.Name + " which jumps out of the grave!");
					}
					else
					{
						dungeonLevel.Message("An icy gust of wind escapes the grave but otherwise it seems empty...");
					}
				}
				if (Game.Instance.Probability(30))
				{
					Item item = Game.Instance.ItemPool.CreateRandomItem(dungeonLevel);
					item.IsCursed = item.IsEquippable && Game.Instance.Probability(80);
					dungeonLevel.Message("Something valuable also was buried beneath the rags and bones contained in the grave.");
					dungeonLevel.AddItem(x, y, item);
				}
			}
			else
			{
				dungeonLevel.Message("The grave contains nothing but some ancient rotten bones...");
			}
			dungeonLevel.SetFeatureAt(x, y, null);
		}
	}

	[Serializable]
	public class WhenEnteringGravestoneBeingInteraction : IBeingInteraction
	{
		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			if (being is Player player)
			{
				if ((player.X + 7) * (player.Y + 117) % 42 == 0 && Game.Instance.HighscoreManager.GetFailedPlayerCount() > 0)
				{
					long num = Game.Instance.HighscoreManager.GetFailedPlayerCount();
					dungeonLevel.Message("An ancient gravestone with " + Game.Instance.GetSpelledOutNumber(num) + " scratch mark" + ((num == 1) ? "s" : "") + ".");
				}
				else
				{
					dungeonLevel.Message("An ancient gravestone.");
				}
			}
			return true;
		}
	}

	[Serializable]
	public class WhenEnteringMudBeingInteraction : IBeingInteraction
	{
		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			if (being is Player)
			{
				dungeonLevel.Message("Yikes - sticky mud!");
			}
			return true;
		}
	}

	[Serializable]
	public class WhenTryingToLeaveMudBeingInteraction : IBeingInteraction
	{
		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			if (being is Player player && Game.Instance.Probability(50 - player.Strength + player.Inventory.ItemsCarried))
			{
				dungeonLevel.Message(player.Name + " remains stuck in the ground!");
				return false;
			}
			return true;
		}
	}

	[Serializable]
	public class WaterItemProcessor : IItemProcessor
	{
		public Item Process(DungeonLevel dungeonLevel, Player grog, Item item)
		{
			if (item.ItemType == ItemType.Scroll)
			{
				dungeonLevel.Message("The " + item.Name + " is destroyed by totally drenching it!", more: true);
				return null;
			}
			return item;
		}
	}

	[Serializable]
	public class WaterBeingInteraction : IBeingInteraction
	{
		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			Player grog = being as Player;
			if (grog != null)
			{
				grog.PotentiallyDestroyItems(dungeonLevel, dungeonLevel.Level + 5, (Item item) => item.ItemType == ItemType.Scroll, " is completely drenched!", (Item i) =>
				{
					grog.Inventory.Remove(i);
				}, int.MaxValue);
			}
			return true;
		}
	}

	[Serializable]
	public class WhenTryingToLeaveSpiderWebsBeingInteraction : IBeingInteraction
	{
		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			if (being.CanMoveFreely)
			{
				return true;
			}
			if (Game.Instance.Random(being.Strength + 20) >= being.Strength)
			{
				dungeonLevel.Message(being, " is stuck in icky spiderwebs!");
				return false;
			}
			dungeonLevel.Message(being, " rips through tattered cobwebs!");
			dungeonLevel.ClearFeatureAt(being.X, being.Y);
			return true;
		}
	}

	[Serializable]
	public class WhenEnteringSpiderWebsBeingInteraction : IBeingInteraction
	{
		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			if (being is Player)
			{
				dungeonLevel.Message("Yikes! Icky cobwebs... lots of them!");
			}
			return true;
		}
	}

	private readonly ICharacterRepresentation _getCharacter;

	private readonly IItemProcessor _itemProcessor;

	public static readonly Feature Gravestone = new Feature('+', new DigUpGraveInteraction(), new WhenEnteringGravestoneBeingInteraction());

	public static readonly Feature SpiderWeb = new Feature('§', null, new WhenEnteringSpiderWebsBeingInteraction(), new WhenTryingToLeaveSpiderWebsBeingInteraction());

	public static readonly Feature Water = new Feature('0', null, new WaterBeingInteraction(), null, new WaterItemProcessor(), new WaterBeingInteraction());

	public static readonly Feature Mud = new Feature('~', null, new WhenEnteringMudBeingInteraction(), new WhenTryingToLeaveMudBeingInteraction());

	public IInteractionInteraction Interaction { get; }

	public IBeingInteraction WhenTryingToLeave { get; }

	public IBeingInteraction WhenEntering { get; }

	public char Character(DungeonLevel dungeonLevel, int x, int y)
	{
		return _getCharacter.GetCharacterRepresentation(dungeonLevel, x, y);
	}

	protected Feature(char c, IInteractionInteraction interaction = null, IBeingInteraction whenEntering = null, IBeingInteraction whenTryingToLeave = null, IItemProcessor itemProcessor = null, IBeingInteraction whenSpendingTurnOnMapElement = null)
		: this(new ConstantCharacterRepresentation(c), interaction, whenEntering, whenTryingToLeave, itemProcessor, whenSpendingTurnOnMapElement)
	{
	}

	protected Feature(ICharacterRepresentation getCharacter, IInteractionInteraction interaction = null, IBeingInteraction whenEntering = null, IBeingInteraction whenTryingToLeave = null, IItemProcessor itemProcessor = null, IBeingInteraction whenSpendingTurnOnMapElement = null)
		: base(whenSpendingTurnOnMapElement)
	{
		Interaction = interaction;
		WhenTryingToLeave = whenTryingToLeave;
		WhenEntering = whenEntering;
		_getCharacter = getCharacter;
		_itemProcessor = itemProcessor;
	}

	public Item GetProcessedItem(DungeonLevel dungeonLevel, Player grog, Item item)
	{
		if (_itemProcessor == null)
		{
			return item;
		}
		return _itemProcessor.Process(dungeonLevel, grog, item);
	}
}
