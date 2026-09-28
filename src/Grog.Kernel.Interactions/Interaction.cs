using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;

namespace Grog.Kernel.Interactions;

public class Interaction
{
	protected readonly DungeonLevel DungeonLevel;

	protected readonly Player Grog;

	protected readonly int X;

	protected readonly int Y;

	protected readonly Func<DungeonLevel, Player, int, int, bool> InteractionCode;

	public char Key { get; }

	public string Description { get; }

	public Interaction(char key, string description, DungeonLevel dungeonLevel, Func<DungeonLevel, Player, int, int, bool> interactionCode)
		: this(key, description, dungeonLevel, dungeonLevel.Grog, dungeonLevel.Grog.X, dungeonLevel.Grog.Y, interactionCode)
	{
	}

	public Interaction(char key, string description, DungeonLevel dungeonLevel, Player grog, int x, int y, Func<DungeonLevel, Player, int, int, bool> interactionCode)
	{
		DungeonLevel = dungeonLevel;
		Grog = grog;
		X = x;
		Y = y;
		InteractionCode = interactionCode;
		Key = key;
		Description = description;
	}

	public Interaction(int keyIndex, string description, DungeonLevel dungeonLevel, Player grog, int x, int y, Func<DungeonLevel, Player, int, int, bool> interactionCode)
		: this((char)((keyIndex < 26) ? (97 + keyIndex) : (65 + keyIndex)), description, dungeonLevel, grog, x, y, interactionCode)
	{
	}

	public virtual bool Execute()
	{
		return InteractionCode(DungeonLevel, Grog, X, Y);
	}
}
