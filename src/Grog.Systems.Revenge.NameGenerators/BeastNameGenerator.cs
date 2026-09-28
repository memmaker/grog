namespace Grog.Systems.Revenge.NameGenerators;

public class BeastNameGenerator : RevengeMonsterNameGenerator
{
	private static readonly string[] _patterns = new string[4] { "012", "02", "0'1", "0-2" };

	private static readonly string[][] _syllables = new string[3][]
	{
		new string[6] { "nox", "ma", "bru", "gro", "shak", "zan" },
		new string[6] { "or", "kar", "tor", "dor", "tar", "tyr" },
		new string[6] { "don", "dor", "in", "ras", "ryn", "ek" }
	};

	public BeastNameGenerator()
		: base(_patterns, _syllables)
	{
	}
}
