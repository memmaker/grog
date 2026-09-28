using System;
using Grog.Dungeons;

namespace Grog.Kernel.Representations;

[Serializable]
public class ConstantCharacterRepresentation : ICharacterRepresentation
{
	private readonly char _c;

	public ConstantCharacterRepresentation(char c)
	{
		_c = c;
	}

	public char GetCharacterRepresentation(DungeonLevel dungeonLevel, int x, int y)
	{
		return _c;
	}
}
