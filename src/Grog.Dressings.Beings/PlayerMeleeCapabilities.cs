using Grog.Kernel;

namespace Grog.Dressings.Beings;

public class PlayerMeleeCapabilities
{
	public MeleeCapability Type { get; }

	public Roll[] DamageRolls { get; }

	public PlayerMeleeCapabilities(MeleeCapability type, Roll[] damageRolls)
	{
		Type = type;
		DamageRolls = damageRolls;
	}
}
