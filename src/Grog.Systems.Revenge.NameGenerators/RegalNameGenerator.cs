namespace Grog.Systems.Revenge.NameGenerators;

public class RegalNameGenerator : RevengeMonsterNameGenerator
{
	private static readonly string[] _patterns = new string[3] { "0-1", "23", "04" };

	private static readonly string[][] _syllables = new string[4][]
	{
		new string[6] { "sha", "xy", "nak", "thot", "ra", "kith" },
		new string[7] { "darr", "thenes", "pakar", "amak", "amun", "shadazz", "yadun" },
		new string[5] { "gar", "tor", "hein", "ulf", "ul" },
		new string[5] { "wulf", "rich", "garr", "thun", "gorr" }
	};

	public RegalNameGenerator()
		: base(_patterns, _syllables)
	{
	}
}
