using Grog.Dressings.Items.Types;

namespace Grog.Dressings.Items.Modifiers;

public interface IItemModifier
{
	int MinimumLevel { get; }

	ItemAbility ItemAbility { get; }

	string Attribute { get; }

	int ValueAdjustmentPercentage { get; }

	ItemType[] GetApplicableItemTypes();

	bool CouldBeAppliedTo(Item item);

	void WhenAddedTo(Item item);

	void WhenRemovedFrom(Item item);
}
