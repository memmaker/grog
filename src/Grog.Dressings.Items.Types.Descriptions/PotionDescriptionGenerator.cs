using System;

namespace Grog.Dressings.Items.Types.Descriptions;

[Serializable]
public class PotionDescriptionGenerator : AliasItemDescriptionGeneratorBase
{
	public static readonly PotionDescriptionGenerator Instance = new PotionDescriptionGenerator();

	public PotionDescriptionGenerator()
		: base(null)
	{
	}

	protected override string GetIdentifiedSpecification(Item item)
	{
		return "";
	}
}
