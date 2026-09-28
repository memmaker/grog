using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;

namespace Grog.Systems.Traps.TrapTypes;

[Serializable]
public abstract class TrapBase : ITrap
{
	public string Name { get; }

	public bool IsAutoIdentifying { get; }

	protected TrapBase(string name, bool isAutoIdentifying = true)
	{
		Name = name;
		IsAutoIdentifying = isAutoIdentifying;
	}

	public abstract void Activate(DungeonLevel dungeonLevel, Being being, int x, int y, bool willfulActivation);
}
