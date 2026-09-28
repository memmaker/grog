namespace Grog.Systems.Revenge.NameGenerators;

public class BruteNameGenerator : RevengeMonsterNameGenerator
{
	private static readonly string[] _patterns = new string[4] { "01", "21", "1-1", "11" };

	private static readonly string[][] _syllables = new string[3][]
	{
		new string[15]
		{
			"br", "kr", "br", "gro", "sh", "zan", "dr", "wr", "o", "og",
			"or", "osh", "ug", "ur", "tr"
		},
		new string[16]
		{
			"ok", "ak", "ick", "ogg", "agg", "igg", "ygg", "ugh", "agh", "udd",
			"ik", "uff", "ugg", "ush", "urk", "ack"
		},
		new string[9] { "dan", "gar", "drar", "brar", "gror", "dog", "dod", "kak", "krish" }
	};

	public BruteNameGenerator()
		: base(_patterns, _syllables)
	{
	}
}
