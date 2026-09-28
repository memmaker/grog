using System;
using System.Collections.Generic;
using Grog.Dungeons.Generators.Rooms.SpecialMessages;
using Grog.Kernel;
using Grog.Kernel.Interactions;

namespace Grog.Dungeons.Generators.Rooms;

public interface IRoom
{
	bool IsSpecial { get; set; }

	bool IsDark { get; set; }

	ISpecialMessageProvider SpecialMessage { get; set; }

	SpecialRoomPower SpecialPower { get; set; }

	int BoundingX1 { get; }

	int BoundingY1 { get; }

	int BoundingX2 { get; }

	int BoundingY2 { get; }

	int DoorCount { get; }

	IBeingInteraction WhenMovingWithinRoom { get; set; }

	bool IsOutsideOfRoom(int x, int y);

	bool IsWallOfRoom(int x, int y);

	bool IsInsideOfRoom(int x, int y);

	Position GetTrulyInsidePosition();

	Position GetInsidePosition();

	bool IsPartOfRoom(int x, int y);

	Position GetInsidePositionForThing(DungeonLevel dungeonLevel);

	void DefineDoor(int x, int y, Direction direction);

	bool HasDoorTo(Direction direction);

	Position GetDoorLeadingTo(Direction direction);

	void DefineTunnel(Direction direction);

	bool HasTunnelTo(Direction direction);

	Position GetTrulyInsidePositionForThing(DungeonLevel dungeonLevel);

	Position GetInsidePositionForItem(DungeonLevel dungeonLevel);

	bool ForEachTrulyInsidePosition(Func<IRoom, Position, bool> isTrueCondition);

	List<Position> GetTrulyInsidePositions(bool copy = false);

	List<Position> GetInsidePositions(bool copy = false);

	Position GetUnusedTrulyInsidePosition(DungeonLevel dungeonLevel);

	bool IsBoring(DungeonLevel dungeonLevel);

	bool IsDoorAt(int x, int y);

	List<Position> GetDoorPositions();

	Position GetPositionForThingAround(DungeonLevel dungeonLevel, int x, int y);

	Position GetInsidePositionForFeature(DungeonLevel dungeonLevel);
}
