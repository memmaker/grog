using System;
using System.Text;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;
using Grog.Kernel.Interactions;

namespace Grog.Systems.Prayers;

public class Prayer : Interaction
{
	private readonly bool _supportedByAltar;

	public Prayer(char key, string description, DungeonLevel dungeonLevel, Player grog, int x, int y, Func<DungeonLevel, Player, int, int, bool> interactionCode, bool supportedByAltar)
		: base(key, description, dungeonLevel, grog, x, y, interactionCode)
	{
		_supportedByAltar = supportedByAltar;
	}

	public override bool Execute()
	{
		if (base.Execute())
		{
			Grog.Moves++;
			Grog.NumberOfPrayersUttered++;
			if (!_supportedByAltar)
			{
				Grog.NumberOfFreePrayers--;
			}
			if (Grog.NumberOfPrayersUttered >= Grog.NumberOfFreePrayers - 2)
			{
				if (Grog.NumberOfPrayersUttered == Grog.NumberOfFreePrayers - 2)
				{
					DungeonLevel.Message("Ta'ker'na seems to get tired of " + Grog.Name + "'s requests!", more: true);
				}
				else if (Grog.NumberOfPrayersUttered == Grog.NumberOfFreePrayers - 1)
				{
					DungeonLevel.Message("Ta'ker'na grumbles unhappily!", more: true);
				}
				else
				{
					DungeonLevel.Message("Ta'ker'na tires of " + Grog.Name + "'s requests and consumes part of " + Grog.Name + "'s essence as compensation!", more: true);
					int num = 1 << Grog.NumberOfPrayersUttered - Grog.NumberOfFreePrayers;
					int num2 = 0;
					int num3 = 0;
					int num4 = 0;
					while (num-- > 0)
					{
						switch (Game.Instance.Random(3))
						{
						case 0:
							num2++;
							break;
						case 1:
							num3++;
							break;
						case 2:
							num4++;
							break;
						}
					}
					int num5 = 0;
					if (num2 > 0)
					{
						num5++;
					}
					if (num3 > 0)
					{
						num5++;
					}
					if (num4 > 0)
					{
						num5++;
					}
					switch (num5)
					{
					case 3:
						DungeonLevel.Message(" loses " + PointsOf(num2) + "strength, " + PointsOf(num3) + "dexterity and " + PointsOf(num4) + "constitution!", more: true);
						break;
					case 2:
					{
						StringBuilder stringBuilder2 = new StringBuilder();
						if (num2 > 0)
						{
							stringBuilder2.Append(PointsOf(num2)).Append("strength");
						}
						if (num3 > 0)
						{
							if (stringBuilder2.Length > 0)
							{
								stringBuilder2.Append(" and ");
							}
							stringBuilder2.Append(PointsOf(num3)).Append("dexterity");
						}
						if (num4 > 0)
						{
							if (stringBuilder2.Length > 0)
							{
								stringBuilder2.Append(" and ");
							}
							stringBuilder2.Append(PointsOf(num4)).Append("constitution");
						}
						DungeonLevel.Message(" loses " + stringBuilder2?.ToString() + "!", more: true);
						break;
					}
					default:
					{
						StringBuilder stringBuilder = new StringBuilder();
						if (num2 > 0)
						{
							stringBuilder.Append(PointsOf(num2)).Append("strength");
						}
						if (num3 > 0)
						{
							stringBuilder.Append(PointsOf(num3)).Append("dexterity");
						}
						if (num4 > 0)
						{
							stringBuilder.Append(PointsOf(num4)).Append("constitution");
						}
						DungeonLevel.Message(" loses " + stringBuilder?.ToString() + "!", more: true);
						break;
					}
					}
					Grog.DrainStrength(DungeonLevel, null, num2, isSilent: true);
					Grog.DrainDexterity(DungeonLevel, null, num2, isSilent: true);
					Grog.DrainConstitution(DungeonLevel, null, num2, isSilent: true);
				}
			}
		}
		return Grog.IsAlive;
	}

	private string PointsOf(int value)
	{
		if (value != 1)
		{
			return value + " points of ";
		}
		return value + " point of ";
	}
}
