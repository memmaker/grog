namespace Grog.Dungeons.Generators.Rooms.Decorators.DecorationGenerators;

public interface IRoomDecorationGenerator
{
	IRoomDecorator GetNextDecorator(IRoom room);
}
