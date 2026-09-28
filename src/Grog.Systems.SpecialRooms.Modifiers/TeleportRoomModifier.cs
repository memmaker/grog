using System;
using System.Collections.Generic;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Rooms;
using Grog.Dungeons.Generators.Rooms.SpecialMessages;
using Grog.Kernel;
using Grog.Kernel.Interactions;

namespace Grog.Systems.SpecialRooms.Modifiers;

public class TeleportRoomModifier : RoomModifierBase
{
	[Serializable]
	public class TeleportToOtherDoorBeingInteraction : IBeingInteraction
	{
		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			List<Position> doorPositions = dungeonLevel.GetRoomAt(being.X, being.Y).GetDoorPositions();
			Position newPosition = doorPositions[Game.Instance.Random(doorPositions.Count)];
			dungeonLevel.Message(being, " is whisked through the ether to another place!");
			dungeonLevel.Teleport(being, newPosition);
			return true;
		}
	}

	public TeleportRoomModifier(int minimumLevel, int rarity)
		: base(minimumLevel, rarity)
	{
	}

	public override bool Modify(DungeonLevel dungeonLevel, IRoom room)
	{
		if (room.DoorCount == 1)
		{
			return false;
		}
		room.SpecialMessage = new ConstantSpecialMessageProvider("This room is flickering with ethereal lights!");
		room.WhenMovingWithinRoom = new TeleportToOtherDoorBeingInteraction();
		return true;
	}
}
