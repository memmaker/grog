using System;

namespace Grog.Dressings.Items.Types.Descriptions;

[Serializable]
public class RingDescriptionGenerator : AliasItemDescriptionGeneratorBase
{
	public static readonly RingDescriptionGenerator Instance = new RingDescriptionGenerator();

	public RingDescriptionGenerator()
		: base("worn")
	{
	}

	protected override string GetIdentifiedSpecification(Item item)
	{
		return "";
	}
}
