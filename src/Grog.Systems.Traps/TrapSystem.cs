using System;
using System.Collections.Generic;
using Grog.Dungeons;
using Grog.Kernel;
using Grog.Systems.Traps.TrapTypes;

namespace Grog.Systems.Traps;

[Serializable]
public class TrapSystem
{
	private static readonly TrapDefinition[] TrapDefinitions = new TrapDefinition[11]
	{
		new TrapDefinition(1, 25, 25, () => new PitTrap()),
		new TrapDefinition(1, 25, 100, () => new ArrowTrap()),
		new TrapDefinition(3, 25, 60, () => new SpearTrap()),
		new TrapDefinition(4, 25, 80, () => new FallingRockTrap()),
		new TrapDefinition(2, 25, 30, () => new TeleportTrap()),
		new TrapDefinition(2, 25, 70, () => new SqueakyBoardTrap()),
		new TrapDefinition(5, 25, 20, () => new AcidTrap()),
		new TrapDefinition(5, 25, 20, () => new ExplosionTrap()),
		new TrapDefinition(2, 25, 100, () => new ConfusionGasTrap()),
		new TrapDefinition(3, 25, 100, () => new BlindnessGasTrap()),
		new TrapDefinition(6, 25, 10, () => new ParalyzationGasTrap())
	};

	public ITrap CreateTrapFor(DungeonLevel dungeonLevel, int x, int y)
	{
		List<TrapDefinition> list = new List<TrapDefinition>();
		int num = 0;
		TrapDefinition[] trapDefinitions = TrapDefinitions;
		foreach (TrapDefinition trapDefinition in trapDefinitions)
		{
			if (dungeonLevel.Level >= trapDefinition.MinimumLevel && dungeonLevel.Level <= trapDefinition.MaximumLevel)
			{
				list.Add(trapDefinition);
				num += trapDefinition.Rarity;
			}
		}
		if (num == 0)
		{
			Game.RaiseError("Failed to generate a trap!");
			return null;
		}
		int num2 = Game.Instance.Random(num);
		foreach (TrapDefinition item in list)
		{
			num2 -= item.Rarity;
			if (num2 <= 0)
			{
				return item.Instantiate();
			}
		}
		Game.RaiseError("Failed to randomly generate a trap!");
		return null;
	}
}
