namespace Grog.Dungeons.Generators;

public class SequentialGenerator : IGenerator
{
	private readonly IGenerator[] _generators;

	public SequentialGenerator(params IGenerator[] generators)
	{
		_generators = generators;
	}

	public void Generate(DungeonLevel dungeonLevel)
	{
		IGenerator[] generators = _generators;
		for (int i = 0; i < generators.Length; i++)
		{
			generators[i].Generate(dungeonLevel);
		}
	}
}
