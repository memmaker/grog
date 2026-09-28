namespace Grog.Systems.Revenge.NameGenerators;

public class CommonNameGenerator : RevengeMonsterNameGenerator
{
	private static readonly string[] _patterns = new string[5] { "012", "01", "10", "102", "0-2" };

	private static readonly string[][] _syllables = new string[3][]
	{
		new string[8] { "an", "on", "ga", "ra", "ul", "po", "ka", "ka" },
		new string[16]
		{
			"dor", "fa", "da", "ko", "ba", "le", "ta", "me", "fa", "be",
			"si", "li", "ba", "du", "wo", "ko"
		},
		new string[13]
		{
			"denes", "dantes", "kans", "wens", "bessem", "nali", "mendes", "lans", "tennes", "olin",
			"ulin", "eryn", "aran"
		}
	};

	public CommonNameGenerator()
		: base(_patterns, _syllables)
	{
	}
}
