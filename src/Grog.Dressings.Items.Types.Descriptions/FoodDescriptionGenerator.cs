using System;

namespace Grog.Dressings.Items.Types.Descriptions;

[Serializable]
public class FoodDescriptionGenerator : ItemDescriptionGeneratorBase
{
	public static readonly FoodDescriptionGenerator Instance = new FoodDescriptionGenerator();

	public FoodDescriptionGenerator()
		: base(null)
	{
	}

	protected override string GetIdentifiedSpecification(Item item)
	{
		return "";
	}
}
