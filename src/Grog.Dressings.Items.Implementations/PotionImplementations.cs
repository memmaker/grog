using System;
using Grog.Dressings.Beings;
using Grog.Dressings.Features;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Dressings.Items.Implementations;

[Serializable]
public class PotionImplementations
{
	public bool UsePotionOfMinorHealing(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user.Race == Race.Undead)
		{
			dungeonLevel.Message(user, " is slightly burned by the liquid!");
			user.SufferDamage(dungeonLevel, applier, Game.Instance.Roll(3, 4));
		}
		else if (user.HitPoints < user.MaxHitPoints)
		{
			user.HitPoints = Math.Min(user.MaxHitPoints, user.HitPoints + Game.Instance.Roll(3, 4));
			if (dungeonLevel.Message(user, " is " + ((user.HitPoints < user.MaxHitPoints) ? "slightly" : "fully") + " healed!"))
			{
				Game.Instance.ItemPool.Identify(potion);
			}
		}
		else
		{
			dungeonLevel.Message(user, " feels revitalized.");
		}
		if (user is Player player)
		{
			player.Moves++;
			player.Satiation += 20;
		}
		return true;
	}

	public bool UsePotionOfMajorHealing(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user.Race == Race.Undead)
		{
			dungeonLevel.Message(user, " is severely burned by the liquid!");
			user.SufferDamage(dungeonLevel, applier, Game.Instance.Roll(3, 8, 3));
		}
		else if (user.HitPoints < user.MaxHitPoints)
		{
			user.HitPoints = Math.Min(user.MaxHitPoints, user.HitPoints + Game.Instance.Roll(3, 8, 3));
			if (dungeonLevel.Message(user, " is " + ((user.HitPoints < user.MaxHitPoints) ? "greatly" : "completely") + " healed!"))
			{
				Game.Instance.ItemPool.Identify(potion);
			}
		}
		else
		{
			dungeonLevel.Message(user, " appears greatly refreshed.");
		}
		if (user is Player player)
		{
			player.Moves++;
			player.Satiation += 40;
		}
		return true;
	}

	public bool UsePotionOfFullHealing(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user.Race == Race.Undead)
		{
			dungeonLevel.Message(user, " is critically burned by the liquid!");
			user.SufferDamage(dungeonLevel, applier, Game.Instance.Roll(8, 8, 8));
		}
		else if (user.HitPoints < user.MaxHitPoints)
		{
			user.HitPoints = user.MaxHitPoints;
			if (dungeonLevel.Message(user, " is fully healed!"))
			{
				Game.Instance.ItemPool.Identify(potion);
			}
		}
		else
		{
			dungeonLevel.Message(user, " appears fully refreshed.");
		}
		if (user is Player player)
		{
			player.Moves++;
			player.Satiation += 100;
		}
		return true;
	}

	public bool UsePotionOfHealth(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user.IsSick != 0)
		{
			user.IsSick = 0;
			if (dungeonLevel.Message(user, " is healthy again!"))
			{
				Game.Instance.ItemPool.Identify(potion);
			}
		}
		else
		{
			dungeonLevel.Message(user, " appears more healthy!");
		}
		if (user is Player player)
		{
			player.Moves++;
			player.Satiation += 100;
		}
		return true;
	}

	public bool UsePotionOfClearThoughts(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user.IsConfused != 0)
		{
			user.IsConfused = 0;
			if (dungeonLevel.Message(user, " has a clear mind once more!"))
			{
				Game.Instance.ItemPool.Identify(potion);
			}
		}
		else
		{
			dungeonLevel.Message(user, " feels determined.");
		}
		if (user is Player player)
		{
			player.Moves++;
			player.Satiation += 5;
		}
		return true;
	}

	public bool UsePotionOfSickness(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user.ResistsSickness)
		{
			dungeonLevel.Message(user, " for a brief moment seems unhealthy but then resists!");
			if (user is Player player)
			{
				player.Moves++;
			}
			return true;
		}
		bool flag = false;
		flag = ((user.IsSick == 0) ? dungeonLevel.Message(user, " suddenly appears to be very sick!") : dungeonLevel.Message(user, " appears even more sick than before!"));
		if (user.IsSick >= 0)
		{
			user.IsSick += Game.Instance.Roll(20, 20, 20);
		}
		if (flag)
		{
			Game.Instance.ItemPool.Identify(potion);
		}
		if (user is Player player2)
		{
			player2.Moves++;
			player2.Satiation += 2;
		}
		return true;
	}

	public bool UsePotionOfConfusion(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user.ResistsConfusion)
		{
			dungeonLevel.Message(user, " seems unsteady for a brief moment but the continues as if nothing had happened.");
			if (user is Player player)
			{
				player.Moves++;
			}
			return true;
		}
		bool flag = false;
		flag = ((user.IsConfused == 0) ? dungeonLevel.Message(user, " suddenly seems to be very confused!") : dungeonLevel.Message(user, " appears even more dizzy!"));
		if (user.IsConfused >= 0)
		{
			user.IsConfused += Game.Instance.Roll(4, 4);
		}
		if (flag)
		{
			Game.Instance.ItemPool.Identify(potion);
		}
		if (user is Player player2)
		{
			player2.Moves++;
			player2.Satiation += 2;
		}
		return true;
	}

	public bool UsePotionOfParalyzation(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user.ResistsParalyzation)
		{
			dungeonLevel.Message(user, " for a brief moment stops moving but then continues as if nothing had happened!");
			if (user is Player player)
			{
				player.Moves++;
			}
			return true;
		}
		if (user.IsParalyzed != 0)
		{
			dungeonLevel.Message(user, " does not seem to be affected...");
		}
		else if (dungeonLevel.Message(user, " suddenly is paralyzed and can't move any longer!"))
		{
			Game.Instance.ItemPool.Identify(potion);
		}
		if (user.IsParalyzed >= 0)
		{
			user.IsParalyzed += Game.Instance.Roll(3, 4);
		}
		if (user is Player player2)
		{
			player2.Moves++;
			player2.Satiation += 2;
		}
		return true;
	}

	public bool UsePotionOfStrength(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (dungeonLevel.Message(user, "'s muscles bulge!"))
		{
			Game.Instance.ItemPool.Identify(potion);
		}
		user.Strength++;
		if (user is Player player)
		{
			player.Moves++;
			player.Satiation += 15;
		}
		return true;
	}

	public bool UsePotionOfDexterity(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (dungeonLevel.Message(user, " suddenly moves more gracefully!"))
		{
			Game.Instance.ItemPool.Identify(potion);
		}
		user.Dexterity++;
		if (user is Player player)
		{
			player.Moves++;
			player.Satiation += 5;
		}
		return true;
	}

	public bool UsePotionOfResilience(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (dungeonLevel.Message(user, " seems to toughen up!"))
		{
			Game.Instance.ItemPool.Identify(potion);
		}
		int num = user.Constitution - 10 >> 1;
		user.Constitution++;
		if (user.Constitution - 10 >> 1 > num)
		{
			user.MaxHitPoints += user.Level;
			user.HitPoints += user.Level;
		}
		if (user is Player player)
		{
			player.Moves++;
			player.Satiation += 20;
		}
		return true;
	}

	public bool UsePotionOfWeakness(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (dungeonLevel.Message(user, " suddenly looks much less muscular!"))
		{
			Game.Instance.ItemPool.Identify(potion);
		}
		user.Strength--;
		if (user is Player player)
		{
			player.Moves++;
			player.Satiation++;
		}
		return true;
	}

	public bool UsePotionOfFumbling(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (dungeonLevel.Message(user, " suddenly moves somewhat like a Klotz!"))
		{
			Game.Instance.ItemPool.Identify(potion);
		}
		user.Dexterity--;
		if (user is Player player)
		{
			player.Moves++;
			player.Satiation += 2;
		}
		return true;
	}

	public bool UsePotionOfExhaustion(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user.Race == Race.Undead)
		{
			dungeonLevel.Message(user, " cackles with glee!");
			return true;
		}
		if (dungeonLevel.Message(user, " suddenly appears seriously exhausted!"))
		{
			Game.Instance.ItemPool.Identify(potion);
		}
		int num = user.Constitution - 10 >> 1;
		user.Constitution--;
		if (user.Constitution - 10 >> 1 < num)
		{
			user.MaxHitPoints -= user.Level;
			user.SufferDamage(dungeonLevel, applier, user.Level);
		}
		if (user is Player player)
		{
			player.Moves++;
		}
		return true;
	}

	public bool UsePotionOfToughness(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (dungeonLevel.Message(user, " suddenly appears tougher!"))
		{
			Game.Instance.ItemPool.Identify(potion);
		}
		user.MaxHitPoints++;
		user.HitPoints++;
		if (user is Player player)
		{
			player.Moves++;
			player.Satiation += 25;
		}
		return true;
	}

	public bool UsePotionOfMeekness(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (dungeonLevel.Message(user, " suddenly appears more meek!"))
		{
			Game.Instance.ItemPool.Identify(potion);
		}
		user.MaxHitPoints--;
		user.SufferDamage(dungeonLevel, applier, 1);
		if (user is Player player)
		{
			player.Moves++;
			player.Satiation -= 5;
		}
		return true;
	}

	public bool UsePotionOfMinorMonsterSense(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user is Player)
		{
			if (dungeonLevel.LearnMoreAboutMap((DungeonLevel dl, int x, int y) => dl.IsBeingAt(x, y), (DungeonLevel dl2, int x2, int y2) => dl2.GetThingAt(x2, y2).Character(dl2, x2, y2)))
			{
				dungeonLevel.Message(" briefly senses the presence of monsters.");
				Game.Instance.ItemPool.Identify(potion);
			}
			else
			{
				dungeonLevel.Message(" feels a brief headache!");
			}
		}
		else
		{
			dungeonLevel.Message(user, " stares inwards for a split-second.");
		}
		if (user is Player player)
		{
			player.Moves++;
			player.Satiation += 5;
		}
		return true;
	}

	public bool UsePotionOfMajorMonsterSense(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user is Player)
		{
			if (!dungeonLevel.IsAttunedToGrog)
			{
				dungeonLevel.Message(" feels deeply attuned to this dungeon level!");
				Game.Instance.ItemPool.Identify(potion);
				dungeonLevel.IsAttunedToGrog = true;
			}
			else
			{
				dungeonLevel.Message(" does not gain any new insights.");
			}
		}
		else
		{
			dungeonLevel.Message(user, " stares intensely inwards for a couple of seconds.");
		}
		if (user is Player player)
		{
			player.Moves++;
			player.Satiation += 5;
		}
		return true;
	}

	public bool UsePotionOfHolyWater(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user is Player)
		{
			dungeonLevel.Message(user, "Tastes like fresh water.");
		}
		else if (user.Race == Race.Undead)
		{
			dungeonLevel.Message(user, " is burned by the liquid and hisses in pain!");
			user.SufferDamage(dungeonLevel, applier, Game.Instance.Roll(4, 8, 8));
			Game.Instance.ItemPool.Identify(potion);
		}
		else
		{
			dungeonLevel.Message(user, " is drenched with water and becomes really angry!");
			user.Mood = Mood.Hunting;
		}
		if (user is Player player)
		{
			player.Moves++;
		}
		return true;
	}

	public bool UsePotionOfHasteSelf(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		bool flag = dungeonLevel.Message(user, " seems to move like greased lightning!");
		user.IsHastened += Game.Instance.Roll(4, 2);
		if (flag)
		{
			Game.Instance.ItemPool.Identify(potion);
		}
		if (user is Player player)
		{
			player.Moves++;
			player.Satiation += 10;
		}
		return true;
	}

	public bool UsePotionOfRaiseLevel(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user is Player player)
		{
			if (player.CanGainLevel)
			{
				int xp = player.ExperienceForNextLevel - player.Experience;
				player.GainExperience(dungeonLevel, xp);
				Game.Instance.ItemPool.Identify(potion);
			}
			else
			{
				dungeonLevel.Message(player, "Your mind ruminates uselessly...");
			}
		}
		else
		{
			if (dungeonLevel.Message(user, " seems to become more experienced!"))
			{
				Game.Instance.ItemPool.Identify(potion);
			}
			user.Level++;
			user.MaxHitPoints += 6;
			user.HitPoints += 6;
		}
		if (user is Player player2)
		{
			player2.Moves++;
			player2.Satiation += 10;
		}
		return true;
	}

	public bool UsePotionOfBlindess(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user.ResistsBlindness)
		{
			dungeonLevel.Message(user, " twitches for a brief moment!");
			if (user is Player player)
			{
				player.Moves++;
			}
			return true;
		}
		if (user.IsBlind != 0)
		{
			dungeonLevel.Message(user, " seems dazzled for a brief moment!");
		}
		else if (dungeonLevel.Message(user, " suddenly turns blind!"))
		{
			Game.Instance.ItemPool.Identify(potion);
		}
		if (user.IsBlind >= 0)
		{
			user.IsBlind += Game.Instance.Roll(4, 6);
		}
		if (user is Player player2)
		{
			player2.Moves++;
			player2.Satiation += 10;
		}
		return true;
	}

	public bool UsePotionOfLevitation(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user.IsLevitating != 0)
		{
			dungeonLevel.Message(user, " shakes for a brief moment!");
		}
		else if (dungeonLevel.Message(user, " is raised into the air!"))
		{
			Game.Instance.ItemPool.Identify(potion);
		}
		if (user.IsLevitating >= 0)
		{
			user.IsLevitating += Game.Instance.Roll(10, 6);
		}
		if (user is Player player)
		{
			player.Moves++;
			player.Satiation += 10;
		}
		return true;
	}

	public bool UsePotionOfTrapDetection(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user is Player player)
		{
			if (dungeonLevel.LearnMoreAboutMap((DungeonLevel dl, int x, int y) => dl.IsTrapAt(x, y), (DungeonLevel dl2, int x2, int y2) =>
			{
				KnownTrapFeature knownTrapFeature = new KnownTrapFeature();
				dl2.SetFeatureAt(x2, y2, knownTrapFeature);
				return knownTrapFeature.Character(dl2, x2, y2);
			}))
			{
				dungeonLevel.Message(" briefly senses the presence of monsters.");
				Game.Instance.ItemPool.Identify(potion);
			}
			else
			{
				dungeonLevel.Message(" feels kind of relaxed.");
			}
			player.Moves++;
			player.Satiation += 10;
		}
		else
		{
			dungeonLevel.Message(user, " chuckles.");
		}
		return true;
	}

	public bool UsePotionOfPoison(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user is Player player)
		{
			dungeonLevel.Message(user, "Aaargh! Vile poison!");
			user.IsPoisoned += Game.Instance.Random(10 + dungeonLevel.Level * 2) + 1;
			Game.Instance.ItemPool.Identify(potion);
			player.Moves++;
			player.Satiation += 5;
		}
		else
		{
			dungeonLevel.Message(user, " appears pretty annoyed about being splashed with this vile liquid!");
			user.Mood = Mood.Hunting;
		}
		return true;
	}

	public bool UsePotionOfDwarvenSpirits(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user.ResistsIntoxication && dungeonLevel.Message(user, " gulps down strong dwarven spirits without any adverse effects!"))
		{
			Game.Instance.ItemPool.Identify(potion);
			if (user is Player player)
			{
				player.Moves++;
				player.Satiation += 5;
			}
			return true;
		}
		bool flag = false;
		flag = ((user.IsDrunk == 0) ? dungeonLevel.Message(user, " gets drunk on dwarven spirits!") : dungeonLevel.Message(user, " gets even more drunk on dwarven spirits!"));
		if (user.IsDrunk >= 0)
		{
			user.IsDrunk += Game.Instance.Roll(4, 8, 10);
		}
		if (flag)
		{
			Game.Instance.ItemPool.Identify(potion);
		}
		if (user is Player player2)
		{
			player2.Moves++;
		}
		return true;
	}

	public bool UsePotionOfElvenWine(DungeonLevel dungeonLevel, Being applier, Being user, Item potion)
	{
		if (user.ResistsIntoxication && dungeonLevel.Message(user, " gulps down powerful elven wine without any adverse effects!"))
		{
			Game.Instance.ItemPool.Identify(potion);
			if (user is Player player)
			{
				player.Moves++;
				player.Satiation += 20;
			}
			return true;
		}
		bool flag = false;
		flag = ((user.IsDrunk == 0) ? dungeonLevel.Message(user, " gets drunk on elven wine!") : dungeonLevel.Message(user, " gets even more drunk on elven wine!"));
		if (user.IsDrunk >= 0)
		{
			user.IsDrunk += Game.Instance.Roll(2, 6, 4);
		}
		if (flag)
		{
			Game.Instance.ItemPool.Identify(potion);
		}
		if (user is Player player2)
		{
			player2.Moves++;
		}
		return true;
	}
}
