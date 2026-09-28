using System;
using System.Collections.Generic;
using Grog.Dressings;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Systems.Revenge;

[Serializable]
public class RevengeMonster : Being
{
	[Serializable]
	public class ActDependingOnMoodAndRevengeLevelThingAction : IThingAction
	{
		private readonly IThingAction _act;

		public ActDependingOnMoodAndRevengeLevelThingAction(IThingAction act)
		{
			_act = act;
		}

		public void Act(DungeonLevel dungeonLevel, Thing thing)
		{
			if (thing is RevengeMonster revengeMonster)
			{
				if ((!revengeMonster.IsAdjacentTo(Game.Instance.Grog) || (revengeMonster.Mood != Mood.Hunting && revengeMonster.Mood != Mood.FightStatically && revengeMonster.Mood != Mood.WaitForPlayer) || Game.Instance.Probability(95)) && (revengeMonster.Moves % Math.Max(6, Game.Instance.RevengeSystem.GetMaximumRevengeProgressionLevelFor(revengeMonster) + 6 - revengeMonster.RevengeLevel) != 0 || revengeMonster.Level + revengeMonster.RevengeLevel <= Game.Instance.Grog.Level))
				{
					_act.Act(dungeonLevel, revengeMonster);
				}
				revengeMonster.Moves++;
			}
		}
	}

	private HashSet<RevengeMonsterPower> _revengeMonsterPowers = new HashSet<RevengeMonsterPower>();

	public override int ToHitBonus => base.ToHitBonus + RevengeToHitBonus;

	public override int ToDamageBonus => base.ToDamageBonus + RevengeToDamageBonus;

	public int RevengeToDamageBonus { get; set; }

	public int RevengeToHitBonus { get; set; }

	public int RevengeLevel { get; set; }

	public override string Type
	{
		get
		{
			if (base.Type.Contains(" ") || string.IsNullOrEmpty(Title))
			{
				return base.Type;
			}
			return GetTitledType(base.Type, Title);
		}
	}

	public string Title { get; set; }

	public IEnumerable<RevengeMonsterPower> RevengeMonsterPowers => _revengeMonsterPowers;

	private string GetTitledType(string type, string title)
	{
		bool flag = title.StartsWith("-");
		if (title.StartsWith("-") || title.StartsWith("+"))
		{
			title = title.Substring(1);
		}
		if (!flag)
		{
			return type + " " + title;
		}
		return title + " " + type;
	}

	public RevengeMonster(Being template)
		: base(template)
	{
		Act = new ActDependingOnMoodAndRevengeLevelThingAction(Act);
		IsInverted = true;
	}

	private bool IsAdjacentTo(Being being)
	{
		if (X != being.X || Math.Abs(being.Y - Y) != 1)
		{
			if (Y == being.Y)
			{
				return Math.Abs(being.X - X) == 1;
			}
			return false;
		}
		return true;
	}

	public override bool HasRevengePower(RevengeMonsterPower power)
	{
		return _revengeMonsterPowers.Contains(power);
	}

	public void AddRevengeMonsterPower(RevengeMonsterPower power)
	{
		_revengeMonsterPowers.Add(power);
	}

	public void Activate()
	{
		UID = Game.Instance.NextUID;
	}
}
