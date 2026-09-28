using System;

namespace Grog.Dressings.Items.Types.Descriptions;

[Serializable]
public class WandDescriptionGenerator : AliasItemDescriptionGeneratorBase
{
	public static readonly WandDescriptionGenerator Instance = new WandDescriptionGenerator();

	public WandDescriptionGenerator()
		: base(null)
	{
	}

	protected override string GetIdentifiedSpecification(Item item)
	{
		return "(" + item.Charges + ")";
	}
}
