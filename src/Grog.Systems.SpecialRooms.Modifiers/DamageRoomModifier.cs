using System;
using Grog.Dressings.Beings;
using Grog.Dressings.Items;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Rooms;
using Grog.Dungeons.Generators.Rooms.SpecialMessages;
using Grog.Kernel;
using Grog.Kernel.Interactions;

namespace Grog.Systems.SpecialRooms.Modifiers;

public class DamageRoomModifier : RoomModifierBase
{
	[Serializable]
	public class DamageWhenMovingInRoomBeingInteraction : IBeingInteraction
	{
		private readonly string _damageMessage;

		private readonly string _resistanceMessage;

		private readonly string _deathCause;

		private readonly INumberConverter _getDamage;

		private readonly ItemAbility _applicableResistance;

		public DamageWhenMovingInRoomBeingInteraction(string damageMessage, string resistanceMessage, string deathCause, INumberConverter getDamage, ItemAbility applicableResistance)
		{
			_damageMessage = damageMessage;
			_resistanceMessage = resistanceMessage;
			_deathCause = deathCause;
			_getDamage = getDamage;
			_applicableResistance = applicableResistance;
		}

		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			if (being is Player player)
			{
				if (player.Inventory.HasEquippedItemWithAbility(_applicableResistance))
				{
					dungeonLevel.Message(" " + _resistanceMessage);
				}
				else
				{
					dungeonLevel.Message(" " + _damageMessage);
					player.SufferDamage(dungeonLevel, null, _getDamage.GetConvertedNumber(dungeonLevel.Level), _deathCause);
				}
			}
			return true;
		}
	}

	private readonly string _specialMessage;

	private readonly string _damageMessage;

	private readonly string _resistanceMessage;

	private readonly string _deathCause;

	private readonly INumberConverter _getDamage;

	private readonly ItemAbility _applicableResistance;

	public DamageRoomModifier(int minimumLevel, int rarity, string specialMessage, string damageMessage, string resistanceMessage, string deathCause, INumberConverter getDamage, ItemAbility applicableResistance)
		: base(minimumLevel, rarity)
	{
		_specialMessage = specialMessage;
		_damageMessage = damageMessage;
		_resistanceMessage = resistanceMessage;
		_deathCause = deathCause;
		_getDamage = getDamage;
		_applicableResistance = applicableResistance;
	}

	public override bool Modify(DungeonLevel dungeonLevel, IRoom room)
	{
		room.SpecialMessage = new ConstantSpecialMessageProvider(_specialMessage);
		room.WhenMovingWithinRoom = new DamageWhenMovingInRoomBeingInteraction(_damageMessage, _resistanceMessage, _deathCause, _getDamage, _applicableResistance);
		return true;
	}
}
