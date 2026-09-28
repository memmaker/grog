using System;
using System.Collections.Generic;
using Grog.Dressings.Beings;
using Grog.Kernel;

namespace Grog.Systems.Revenge;

public class RevengeLevelBonus
{
	private readonly int _numberOfAttackDiceModifier;

	private readonly int _attackDiceSideModifier;

	private readonly int _singleAttackDiceDamageBonus;

	private readonly int _numberOfAttacksModifier;

	public int HitPointBonus { get; }

	public int ArmorClassBonus { get; }

	public int ToHitBonus { get; }

	public int ToDamageBonus { get; }

	public RevengeMonsterPower RevengeMonsterPower { get; }

	public Action<Being> ModifyBeing { get; }

	public string Title { get; }

	public RevengeLevelBonus(string title, int hitPointBonus, int armorClassBonus, int toHitBonus, int toDamageBonus, RevengeMonsterPower revengeMonsterPower, int numberOfAttackDiceModifier, int attackDiceSideModifier, int singleAttackDiceDamageBonus, int numberOfAttacksModifier, Action<Being> modifyBeing = null)
	{
		_numberOfAttackDiceModifier = numberOfAttackDiceModifier;
		_attackDiceSideModifier = attackDiceSideModifier;
		_singleAttackDiceDamageBonus = singleAttackDiceDamageBonus;
		_numberOfAttacksModifier = numberOfAttacksModifier;
		Title = title;
		HitPointBonus = hitPointBonus;
		ArmorClassBonus = armorClassBonus;
		ToHitBonus = toHitBonus;
		ToDamageBonus = toDamageBonus;
		RevengeMonsterPower = revengeMonsterPower;
		ModifyBeing = modifyBeing;
	}

	public Roll[] GetModifiedNaturalDamage(Roll[] naturalDamage)
	{
		Roll[] array = new Roll[naturalDamage.Length];
		for (int i = 0; i < naturalDamage.Length; i++)
		{
			array[i] = new Roll(naturalDamage[i].NumberOfDice + _numberOfAttackDiceModifier, naturalDamage[i].DieSides + _attackDiceSideModifier, naturalDamage[i].DieBonus);
		}
		if (_singleAttackDiceDamageBonus != 0)
		{
			int num = Game.Instance.Random(array.Length);
			array[num] = new Roll(array[num].NumberOfDice, array[num].DieSides, array[num].DieBonus + _singleAttackDiceDamageBonus);
		}
		if (_numberOfAttacksModifier != 0)
		{
			List<Roll> list = new List<Roll>(array);
			for (int j = 0; j < _numberOfAttacksModifier; j++)
			{
				Roll roll = array[Game.Instance.Random(array.Length)];
				list.Add(new Roll(roll));
			}
			array = list.ToArray();
		}
		return array;
	}
}
