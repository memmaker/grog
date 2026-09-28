using System.Collections.Generic;
using Grog.Dressings;

namespace Grog.Dungeons.Generators.Rooms.Decorators.Features.Lib.Base;

public abstract class FeatureRoomDecoratorBase : DecorateAndConsumeSingleRoomBase
{
	private readonly bool _useEmptyRoomsOnly;

	protected FeatureRoomDecoratorBase(bool useEmptyRoomsOnly)
	{
		_useEmptyRoomsOnly = useEmptyRoomsOnly;
	}

	protected override IRoom FindAppropriateRoom(DungeonLevel dungeonLevel, List<IRoom> rooms)
	{
		if (_useEmptyRoomsOnly)
		{
			List<IRoom> list = new List<IRoom>();
			foreach (IRoom room in rooms)
			{
				if (room.ForEachTrulyInsidePosition((IRoom r, Position p) => !dungeonLevel.HasFeatureAt(p.X, p.Y) && dungeonLevel.GetTileAt(p.X, p.Y) == Tile.Floor))
				{
					list.Add(room);
				}
			}
			if (list.Count != 0)
			{
				return base.FindAppropriateRoom(dungeonLevel, list);
			}
			return null;
		}
		return base.FindAppropriateRoom(dungeonLevel, rooms);
	}
}
