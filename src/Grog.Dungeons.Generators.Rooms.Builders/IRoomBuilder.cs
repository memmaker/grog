namespace Grog.Dungeons.Generators.Rooms.Builders;

public interface IRoomBuilder
{
	IRoom DigRoom(DungeonLevel dungeonLevel, int x, int y, int maxWidth, int maxHeight);
}
