using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;
using Grog.Kernel.Interactions;

namespace Grog.Dressings.Features;

[Serializable]
public class MagicLiquidFeature : Feature
{
	[Serializable]
	public class DisplayNameUponEnteringBeingInteraction : IBeingInteraction
	{
		private readonly string _name;

		public DisplayNameUponEnteringBeingInteraction(string name)
		{
			_name = name;
		}

		public bool Interact(DungeonLevel dungeonLevel, Being being)
		{
			if (being is Player)
			{
				dungeonLevel.Message(_name + ".");
			}
			return true;
		}
	}

	[Serializable]
	public class DrinkMagicLiquidInteraction : IInteractionInteraction
	{
		private readonly string _name;

		private int _sips;

		public DrinkMagicLiquidInteraction(string name, int sips)
		{
			_name = name;
			_sips = sips;
		}

		public void Interact(DungeonLevel dungeonLevel, Player grog, int x, int y)
		{
			dungeonLevel.Message(" takes a sip from the " + _name + ".");
			switch (Game.Instance.Random(12))
			{
			case 0:
				dungeonLevel.Message(" grows stronger!");
				grog.Strength++;
				break;
			case 1:
				dungeonLevel.Message(" grows more agile!");
				grog.Dexterity++;
				break;
			case 2:
				dungeonLevel.Message(" grows more healthy!");
				grog.Constitution++;
				break;
			case 3:
				dungeonLevel.Message(" feels weakened!");
				grog.DrainStrength(dungeonLevel, null);
				break;
			case 4:
				dungeonLevel.Message(" feels clumsy!");
				grog.DrainDexterity(dungeonLevel, null);
				break;
			case 5:
				dungeonLevel.Message(" feels sick!");
				grog.DrainConstitution(dungeonLevel, null);
				break;
			case 6:
				dungeonLevel.Message(" feels more robust!");
				grog.MaxHitPoints++;
				grog.HitPoints++;
				break;
			case 7:
				dungeonLevel.Message(" feels fragile!");
				grog.MaxHitPoints--;
				grog.SufferDamage(dungeonLevel, null, 1, "by spoiled water");
				break;
			case 8:
				if (grog.IsConfused == 0)
				{
					dungeonLevel.Message(" suddenly can't focus anymore!");
				}
				else
				{
					dungeonLevel.Message(" feels even more swirly!");
				}
				grog.IsConfused += Game.Instance.Roll(4, 4);
				break;
			case 9:
				dungeonLevel.Say("Whoah! This is the finest booze you ever tasted!");
				grog.IsDrunk += Game.Instance.Roll(10, 10);
				break;
			case 10:
				if (grog.IsBlind == 0)
				{
					dungeonLevel.Message(" suddenly can't see anything!");
				}
				else
				{
					dungeonLevel.Message("'s eyes feel itchy!");
				}
				grog.IsBlind += Game.Instance.Roll(2, 20);
				break;
			case 11:
				if (grog.IsSick == 0)
				{
					dungeonLevel.Message(" suddenly feels very ill!");
				}
				else
				{
					dungeonLevel.Message(" feels even more sick!");
				}
				grog.IsSick += Game.Instance.Roll(4, 10);
				break;
			}
			_sips--;
			if (_sips < 1)
			{
				dungeonLevel.Message("The " + _name + " dries up!");
				dungeonLevel.SetFeatureAt(x, y, null);
			}
		}
	}

	private readonly string _name;

	public MagicLiquidFeature(string name, int sips)
		: base('*', new DrinkMagicLiquidInteraction(name, sips), new DisplayNameUponEnteringBeingInteraction(name))
	{
		_name = name;
	}
}
