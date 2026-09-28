using System.Text;
using Grog.Kernel;

namespace Grog.Systems.Revenge;

public class RevengeMonsterNameGenerator
{
	private readonly string[] _patterns;

	private readonly string[][] _syllables;

	public RevengeMonsterNameGenerator(string[] patterns, string[][] syllables)
	{
		_patterns = patterns;
		_syllables = syllables;
	}

	public string GenerateRandomName()
	{
		string obj = _patterns[Game.Instance.Random(_patterns.Length)];
		StringBuilder stringBuilder = new StringBuilder();
		bool capitalize = true;
		string text = obj;
		foreach (char c in text)
		{
			if (c >= '0' && c <= '9')
			{
				int num = c - 48;
				stringBuilder.Append(GetCapitalized(_syllables[num][Game.Instance.Random(_syllables[num].Length)], capitalize));
				capitalize = false;
			}
			else if (c == '*')
			{
				int num2 = Game.Instance.Random(_syllables.Length);
				stringBuilder.Append(GetCapitalized(_syllables[num2][Game.Instance.Random(_syllables[num2].Length)], capitalize));
				capitalize = false;
			}
			else
			{
				stringBuilder.Append(c);
				capitalize = true;
			}
		}
		return stringBuilder.ToString();
	}

	private string GetCapitalized(string syllable, bool capitalize)
	{
		if (!capitalize)
		{
			return syllable;
		}
		return char.ToUpper(syllable[0]) + syllable.Substring(1);
	}
}
