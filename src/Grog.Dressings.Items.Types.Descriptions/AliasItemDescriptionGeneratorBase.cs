using System;
using Grog.Kernel;

namespace Grog.Dressings.Items.Types.Descriptions;

[Serializable]
public abstract class AliasItemDescriptionGeneratorBase : ItemDescriptionGeneratorBase
{
	public AliasItemDescriptionGeneratorBase(string equippedText)
		: base(equippedText)
	{
	}

	protected override string GetUnidentifiedDescription(Item item)
	{
		return Game.Instance.ItemPool.GetAliasFor(item);
	}
}
