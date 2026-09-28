namespace Grog.Dungeons.Generators.Rooms.Generators;

public class SingleBigRoomGenerator : RoomGeneratorBase
{
	public SingleBigRoomGenerator(DungeonLevel dungeonLevel)
		: base(dungeonLevel)
	{
	}

	public override void Generate(DungeonLevel dungeonLevel)
	{
		dungeonLevel.AddRoom(0, 0, dungeonLevel.Width - 2, dungeonLevel.Height - 3);
	}
}
