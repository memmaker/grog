using System;
using System.Collections.Generic;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel;
using Grog.Kernel.FileAccess;

namespace Grog.Systems.Ghosts;

[Serializable]
public class GhostSystem
{
	private List<Ghost> _ghosts;

	private Ghost _activeGhost;

	public int Count => _ghosts.Count;

	public GhostSystem()
	{
		_ghosts = FileManager.ReadObject("grog_v1.gst", () => new List<Ghost>());
	}

	private void CreateNewGhost(Player player)
	{
		_ghosts.Add(new Ghost(player));
		SaveGhosts();
	}

	private void SaveGhosts()
	{
		FileManager.WriteObject("grog_v1.gst", _ghosts);
	}

	public Ghost CreateGhostMonsterIfAppropriate(DungeonLevel dungeonLevel, bool enforceGhostCreation = false)
	{
		if (_ghosts.Count == 0)
		{
			return null;
		}
		if (_activeGhost != null)
		{
			if (_activeGhost.IsAlive)
			{
				return null;
			}
			_activeGhost = null;
		}
		if (!enforceGhostCreation && !Game.Instance.Probability(Math.Max(dungeonLevel.Level, 5)))
		{
			return null;
		}
		List<Ghost> list = new List<Ghost>();
		foreach (Ghost ghost in _ghosts)
		{
			if (ghost.Level >= dungeonLevel.Level - 1 && ghost.Level <= dungeonLevel.Level + 1)
			{
				list.Add(ghost);
			}
		}
		if (list.Count == 0)
		{
			if (!enforceGhostCreation)
			{
				return null;
			}
			list = new List<Ghost>(_ghosts);
		}
		_activeGhost = list[Game.Instance.Random(list.Count)];
		_activeGhost.Activate();
		_ghosts.Remove(_activeGhost);
		return _activeGhost;
	}

	public void StoreActiveGhostsForLater()
	{
		if (_activeGhost != null && _activeGhost.IsAlive)
		{
			_ghosts.Add(_activeGhost);
			SaveGhosts();
		}
	}

	public void CheckForGhostCreation(DungeonLevel dungeonLevel, Player player, Being attacker, bool enforceGhostCreation = false)
	{
		int num = 5 + dungeonLevel.Level + dungeonLevel.Level / 2 + player.Level / 2;
		if (attacker != null)
		{
			if (attacker.Race == Race.Demon || attacker.Race == Race.Devil)
			{
				num += num / 2;
			}
			else if (attacker.Race == Race.Undead)
			{
				num = Math.Min(15, num * 2);
			}
		}
		if (enforceGhostCreation || Game.Instance.Probability(num))
		{
			dungeonLevel.Render();
			dungeonLevel.Message(" turns into a ghost!", more: true);
			CreateNewGhost(player);
		}
	}

	public void MakeGhostsUniqueAgain(Dictionary<long, Being> beingsByUid)
	{
		if (_activeGhost != null && beingsByUid.ContainsKey(_activeGhost.UID))
		{
			_activeGhost = (Ghost)beingsByUid[_activeGhost.UID];
		}
	}

	public void AddInformation(List<string> information)
	{
		information.Add("Ghosts: " + Count);
	}
}
