using System;

namespace Grog.Dungeons.Generators.Rooms.SpecialMessages;

[Serializable]
public class OneTimeSpecialMessageProvider : ISpecialMessageProvider
{
	private readonly string _message;

	public OneTimeSpecialMessageProvider(string message)
	{
		_message = message;
	}

	public string GetMessageFor(DungeonLevel dungeonLevel, IRoom room)
	{
		room.SpecialMessage = null;
		return _message;
	}
}
