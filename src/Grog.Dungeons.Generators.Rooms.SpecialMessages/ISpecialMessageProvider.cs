namespace Grog.Dungeons.Generators.Rooms.SpecialMessages;

public interface ISpecialMessageProvider
{
	string GetMessageFor(DungeonLevel dungeonLevel, IRoom room);
}
