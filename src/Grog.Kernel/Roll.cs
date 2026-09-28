using System;
using Grog.Kernel.Options;

namespace Grog.Kernel;

[Serializable]
public class Roll
{
	public int Minimum => NumberOfDice + DieBonus;

	public int Maximum => NumberOfDice * DieSides + DieBonus;

	public int Average => (NumberOfDice + NumberOfDice * DieSides) / NumberOfDice + DieBonus;

	public int NumberOfDice { get; set; }

	public int DieSides { get; set; }

	public int DieBonus { get; set; }

	public Roll(int numberOfDice, int dieSides, int dieBonus = 0)
	{
		NumberOfDice = numberOfDice;
		DieSides = dieSides;
		DieBonus = dieBonus;
	}

	public Roll(Roll roll)
		: this(roll.NumberOfDice, roll.DieSides, roll.DieBonus)
	{
	}

	public int GetDieResult()
	{
		int num = DieBonus;
		for (int i = 0; i < NumberOfDice; i++)
		{
			num += Game.Instance.Random(DieSides) + 1;
		}
		return num;
	}

	public string GetSpecification(int bonus)
	{
		if (Game.Instance.Options.DamageNotation == DamageNotation.Range)
		{
			int num = Math.Max(1, NumberOfDice + DieBonus + bonus);
			int num2 = Math.Max(1, NumberOfDice * DieSides + DieBonus + bonus);
			return num + "-" + num2;
		}
		int num3 = DieBonus + bonus;
		string text = NumberOfDice.ToString();
		string text2 = DieSides.ToString();
		string? text3;
		if (num3 < 0)
		{
			text3 = num3.ToString();
		}
		else
		{
			text3 = ((num3 > 0) ? ("+" + num3) : "");
		}
		return text + "d" + text2 + text3;
	}
}
