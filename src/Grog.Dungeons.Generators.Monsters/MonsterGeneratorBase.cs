using Grog.Kernel;

namespace Grog.Dungeons.Generators.Monsters;

public abstract class MonsterGeneratorBase : IGenerator
{
	public void Generate(DungeonLevel dungeonLevel)
	{
		MonsterList availableMonsters = Game.Instance.MonsterPool.GetAvailableMonsters(dungeonLevel);
		GenerateMonsters(availableMonsters, dungeonLevel);
	}

	protected abstract void GenerateMonsters(MonsterList availableMonsters, DungeonLevel dungeonLevel);
}
