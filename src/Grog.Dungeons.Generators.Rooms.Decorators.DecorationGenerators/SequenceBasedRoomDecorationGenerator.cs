namespace Grog.Dungeons.Generators.Rooms.Decorators.DecorationGenerators;

public class SequenceBasedRoomDecorationGenerator : IRoomDecorationGenerator
{
	private readonly IRoomDecorator _defaultDecorator;

	private readonly IRoomDecorator[] _sequence;

	private int _index;

	public SequenceBasedRoomDecorationGenerator(IRoomDecorator defaultDecorator, params IRoomDecorator[] sequence)
	{
		_defaultDecorator = defaultDecorator;
		_sequence = sequence;
		_index = 0;
	}

	public IRoomDecorator GetNextDecorator(IRoom room)
	{
		if (_sequence != null && _index < _sequence.Length)
		{
			_index++;
			return _sequence[_index - 1];
		}
		return _defaultDecorator;
	}
}
