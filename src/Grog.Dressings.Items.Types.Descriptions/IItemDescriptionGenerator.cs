namespace Grog.Dressings.Items.Types.Descriptions;

public interface IItemDescriptionGenerator
{
	string GetItemDescription(Item item);

	string GetShortItemDescription(Item item);
}
