using System;

namespace Grog.Dungeons.Generators.Rooms.SpecialMessages;

[Serializable]
public class ConstantSpecialMessageProvider : ISpecialMessageProvider
{
	private readonly string _message;

	public ConstantSpecialMessageProvider(string message)
	{
		_message = message;
	}

	public string GetMessageFor(DungeonLevel dungeonLevel, IRoom room)
	{
		return _message;
	}
}
