using System;

namespace Grog.Dressings.Items.Types.Descriptions;

[Serializable]
public class ArmorDescriptionGenerator : ItemDescriptionGeneratorBase
{
	public static readonly ArmorDescriptionGenerator Instance = new ArmorDescriptionGenerator();

	public ArmorDescriptionGenerator()
		: base("worn")
	{
	}

	protected override string GetIdentifiedSpecification(Item item)
	{
		int num = item.AcBonus + item.ItemBonus;
		string? text;
		if (item.ItemBonus != 0)
		{
			text = ((item.ItemBonus < 0) ? (item.ItemBonus + " ") : ("+" + item.ItemBonus + " "));
		}
		else
		{
			text = "";
		}
		return text + "[AC: " + ((num < 0) ? (num.ToString() ?? "") : ("+" + num)) + "]";
	}
}
