using Grog.Dungeons;

namespace Grog.Kernel.Representations;

public interface ICharacterRepresentation
{
	char GetCharacterRepresentation(DungeonLevel dungeonLevel, int x, int y);
}
