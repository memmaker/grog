using System;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Dungeons.Generators.Monsters;
using Grog.Dungeons.Generators.Rooms;
using Grog.Dungeons.Generators.Rooms.SpecialMessages;
using Grog.Kernel;

namespace Grog.Systems.SpecialRooms.Modifiers;

public class MonsterPitRoomModifier : RoomModifierBase
{
	private readonly bool _singleMonsterType;

	private readonly Func<int, int> _dangerLevelModifier;

	private readonly int _probabilityPerTileForTreasure;

	private readonly string _welcomeMessage;

	private readonly bool _allowRevengeMonsters;

	public MonsterPitRoomModifier(int minimumLevel, int rarity, bool singleMonsterType, Func<int, int> dangerLevelModifier, int probabilityPerTileForTreasure, string welcomeMessage, bool allowRevengeMonsters)
		: base(minimumLevel, rarity)
	{
		_singleMonsterType = singleMonsterType;
		_dangerLevelModifier = dangerLevelModifier;
		_probabilityPerTileForTreasure = probabilityPerTileForTreasure;
		_welcomeMessage = welcomeMessage;
		_allowRevengeMonsters = allowRevengeMonsters;
	}

	public override bool Modify(DungeonLevel dungeonLevel, IRoom room)
	{
		int level = _dangerLevelModifier(dungeonLevel.Level);
		room.SpecialMessage = new OneTimeSpecialMessageProvider(_welcomeMessage);
		MonsterList availableMonsters = Game.Instance.MonsterPool.GetAvailableMonsters(level);
		MonsterDefinition monsterDefinition = null;
		foreach (Position insidePosition in room.GetInsidePositions())
		{
			if (monsterDefinition == null)
			{
				monsterDefinition = availableMonsters.FindRandomAvailableMonsterDefinition();
			}
			if (dungeonLevel.IsOpenForNewThing(insidePosition))
			{
				Being being = availableMonsters.CreateMonsterBasedOnDefinition(dungeonLevel, monsterDefinition, _allowRevengeMonsters);
				being.Mood = Mood.Ambushing;
				dungeonLevel.SetBeing(insidePosition.X, insidePosition.Y, being);
			}
		}
		return true;
	}
}
