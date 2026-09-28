using System;

namespace Grog.Dressings.Items.Types.Descriptions;

[Serializable]
public class ScrollDescriptionGenerator : AliasItemDescriptionGeneratorBase
{
	public static readonly ScrollDescriptionGenerator Instance = new ScrollDescriptionGenerator();

	public ScrollDescriptionGenerator()
		: base(null)
	{
	}

	protected override string GetIdentifiedSpecification(Item item)
	{
		return "";
	}
}
