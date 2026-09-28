using System;
using System.Collections.Generic;
using Grog.Dressings.Items.Implementations;
using Grog.Dressings.Items.Modifiers;
using Grog.Dressings.Items.Types;
using Grog.Dungeons;
using Grog.Kernel;

namespace Grog.Dressings.Items;

[Serializable]
public class ItemPool
{
	private ItemDefinition[] _items;

	private Dictionary<ItemId, ItemDefinition> _definitions = new Dictionary<ItemId, ItemDefinition>();

	private Dictionary<ItemType, Dictionary<ItemId, string>> _aliasDefinitions = new Dictionary<ItemType, Dictionary<ItemId, string>>();

	private PotionImplementations _potionImplementations = new PotionImplementations();

	private ScrollImplementations _scrollImplementations = new ScrollImplementations();

	private FoodImplementations _foodImplementations = new FoodImplementations();

	private RingImplementations _ringImplementations = new RingImplementations();

	private WandImplementations _wandImplementations = new WandImplementations();

	private HashSet<ItemId> _identifiedItems = new HashSet<ItemId>();

	private static readonly Dictionary<ItemType, string> PluralItemTypeNames = new Dictionary<ItemType, string>
	{
		{
			ItemType.Armor,
			"armors"
		},
		{
			ItemType.Food,
			"food"
		},
		{
			ItemType.Potion,
			"potions"
		},
		{
			ItemType.Ring,
			"rings"
		},
		{
			ItemType.Scroll,
			"scrolls"
		},
		{
			ItemType.Shield,
			"shields"
		},
		{
			ItemType.Wand,
			"wands"
		},
		{
			ItemType.MeleeWeapon,
			"melee weapons"
		}
	};

	public ItemModifiers ItemModifiers { get; }

	public ItemPool()
	{
		ItemModifiers = new ItemModifiers();
		foreach (ItemType value in Enum.GetValues(typeof(ItemType)))
		{
			if (!PluralItemTypeNames.ContainsKey(value) && value != ItemType.None)
			{
				throw new GrogException("Invalid setup. Item type '" + value.ToString() + "' does not have a plural name!");
			}
		}
	}

	public void Initialize()
	{
		_items = new ItemDefinition[138]
		{
			new ItemDefinition(new Armor(ItemId.Tunic, "tunic", 0, 3, new Roll(1, 2), isMetallic: false), 1, 50),
			new ItemDefinition(new Armor(ItemId.PaddedArmor, "padded armor", 1, 10, new Roll(1, 2), isMetallic: false), 1),
			new ItemDefinition(new Armor(ItemId.LeatherArmor, "leather armor", 2, 20, new Roll(1, 2), isMetallic: false), 1),
			new ItemDefinition(new Armor(ItemId.StuddedLeatherArmor, "studded leather armor", 3, 35, new Roll(1, 2), isMetallic: false), 1),
			new ItemDefinition(new Armor(ItemId.ChainMail, "chain mail", 4, 50, new Roll(1, 3), isMetallic: true), 2),
			new ItemDefinition(new Armor(ItemId.ChainHauberk, "chain hauberk", 5, 75, new Roll(1, 4), isMetallic: true), 3, 50),
			new ItemDefinition(new Armor(ItemId.SplintMail, "splint mail", 6, 120, new Roll(1, 5), isMetallic: true), 4, 40),
			new ItemDefinition(new Armor(ItemId.PlateMail, "plate mail", 7, 350, new Roll(1, 8), isMetallic: true), 6, 25),
			new ItemDefinition(new Armor(ItemId.FieldPlateArmor, "field plate armor", 8, 1000, new Roll(1, 10), isMetallic: true), 10, 10),
			new ItemDefinition(new Armor(ItemId.FullPlateArmor, "full plate armor", 9, 5000, new Roll(2, 6), isMetallic: true), 14, 8),
			new ItemDefinition(new Armor(ItemId.ElvenChainMail, "elven chain mail", 8, 25000, new Roll(1, 2), isMetallic: true, canHavePrefixModifier: false), 14, 1),
			new ItemDefinition(new Armor(ItemId.MithrilChainHauberk, "mithril chain hauberk", 10, 50000, new Roll(1, 4), isMetallic: false, canHavePrefixModifier: false), 14, 1),
			new ItemDefinition(new MeleeWeapon(ItemId.Knife, "knife", 1, 1, 0, new Roll(1, 3), isMetallic: true), 1, 50),
			new ItemDefinition(new MeleeWeapon(ItemId.Dagger, "dagger", 1, 2, 0, new Roll(1, 4), isMetallic: true), 1, 50),
			new ItemDefinition(new MeleeWeapon(ItemId.Club, "club", 1, 3, 0, new Roll(1, 6), isMetallic: false), 1),
			new ItemDefinition(new MeleeWeapon(ItemId.HandAxe, "hand axe", 1, 5, 1, new Roll(1, 6), isMetallic: true), 1),
			new ItemDefinition(new MeleeWeapon(ItemId.Mace, "mace", 1, 10, 0, new Roll(1, 6, 1), isMetallic: true), 1),
			new ItemDefinition(new MeleeWeapon(ItemId.BroadSword, "broadsword", 1, 12, 0, new Roll(2, 4), isMetallic: true), 1),
			new ItemDefinition(new MeleeWeapon(ItemId.ShortSword, "short sword", 1, 8, 0, new Roll(1, 6), isMetallic: true), 1),
			new ItemDefinition(new MeleeWeapon(ItemId.LongSword, "longsword", 1, 15, 1, new Roll(1, 8), isMetallic: true), 1),
			new ItemDefinition(new MeleeWeapon(ItemId.BattleAxe, "battle axe", 1, 15, 0, new Roll(1, 8, 1), isMetallic: true), 1),
			new ItemDefinition(new MeleeWeapon(ItemId.Warhammer, "warhammer", 1, 12, 2, new Roll(2, 3), isMetallic: true), 1),
			new ItemDefinition(new MeleeWeapon(ItemId.TwoHandedSword, "great sword", 2, 120, 2, new Roll(2, 6, 2), isMetallic: true), 4),
			new ItemDefinition(new MeleeWeapon(ItemId.TwoHandedAxe, "great axe", 2, 70, -2, new Roll(2, 7, 1), isMetallic: true), 3),
			new ItemDefinition(new MeleeWeapon(ItemId.PickAxe, "pick axe", 2, 100, -3, new Roll(1, 6, 1), isMetallic: true), 9, 25),
			new ItemDefinition(new MeleeWeapon(ItemId.Maul, "maul", 2, 50, 1, new Roll(1, 12, 2), isMetallic: false), 2),
			new ItemDefinition(new MeleeWeapon(ItemId.HeavyMace, "heavy mace", 2, 80, 1, new Roll(4, 4), isMetallic: true), 4),
			new ItemDefinition(new MeleeWeapon(ItemId.ElvenKnife, "elven knife", 1, 2000, 6, new Roll(4, 2), isMetallic: true, canHavePrefixModifier: false), 8, 1),
			new ItemDefinition(new MeleeWeapon(ItemId.DwarvenAxe, "dwarven axe", 1, 1200, 2, new Roll(3, 5), isMetallic: true, canHavePrefixModifier: false), 6, 1),
			new ItemDefinition(new MeleeWeapon(ItemId.ElvenLongSword, "elven longsword", 1, 12000, 4, new Roll(2, 5), isMetallic: true, canHavePrefixModifier: false), 10, 1),
			new ItemDefinition(new MeleeWeapon(ItemId.HolyAvenger, "holy avenger", 1, 16000, 5, new Roll(2, 6), isMetallic: true, canHavePrefixModifier: false), 15, 1),
			new ItemDefinition(new Shield(ItemId.BucklerShield, "buckler", 2, 10, new Roll(1, 3), isMetallic: true), 1, 30),
			new ItemDefinition(new Shield(ItemId.HeaterShield, "heater shield", 4, 20, new Roll(1, 6), isMetallic: true), 2, 20),
			new ItemDefinition(new Shield(ItemId.RoundShield, "round shield", 3, 18, new Roll(1, 6), isMetallic: true), 1, 25),
			new ItemDefinition(new Shield(ItemId.KiteShield, "kite shield", 5, 35, new Roll(1, 8), isMetallic: true), 4, 10),
			new ItemDefinition(new Potion(ItemId.PotionOfMinorHealing, "potion of minor healing", "amber potion", 30, _potionImplementations.UsePotionOfMinorHealing), 1),
			new ItemDefinition(new Potion(ItemId.PotionOfMajorHealing, "potion of major healing", "misty potion", 75, _potionImplementations.UsePotionOfMajorHealing), 2, 80),
			new ItemDefinition(new Potion(ItemId.PotionOfFullHealing, "potion of full healing", "glowing potion", 120, _potionImplementations.UsePotionOfFullHealing), 4, 40),
			new ItemDefinition(new Potion(ItemId.PotionOfHealth, "potion of health", "red potion", 50, _potionImplementations.UsePotionOfHealth), 1),
			new ItemDefinition(new Potion(ItemId.PotionOfClearThoughts, "potion of clear thoughts", "green potion", 40, _potionImplementations.UsePotionOfClearThoughts), 1, 120),
			new ItemDefinition(new Potion(ItemId.PotionOfSickness, "potion of sickness", "blue potion", 5, _potionImplementations.UsePotionOfSickness), 1, 20),
			new ItemDefinition(new Potion(ItemId.PotionOfConfusion, "potion of confusion", "white potion", 10, _potionImplementations.UsePotionOfConfusion), 1, 20),
			new ItemDefinition(new Potion(ItemId.PotionOfParalyzation, "potion of paralyzation", "black potion", 15, _potionImplementations.UsePotionOfParalyzation), 1, 20),
			new ItemDefinition(new Potion(ItemId.PotionOfStrength, "potion of strength", "orange potion", 400, _potionImplementations.UsePotionOfStrength), 6, 5, 4),
			new ItemDefinition(new Potion(ItemId.PotionOfDexterity, "potion of dexterity", "slimy potion", 200, _potionImplementations.UsePotionOfDexterity), 4, 7, 4),
			new ItemDefinition(new Potion(ItemId.PotionOfResilience, "potion of resilience", "bubbling potion", 300, _potionImplementations.UsePotionOfResilience), 5, 6, 4),
			new ItemDefinition(new Potion(ItemId.PotionOfWeakness, "potion of weakness", "yellow potion", 5, _potionImplementations.UsePotionOfWeakness), 6, 7, 3),
			new ItemDefinition(new Potion(ItemId.PotionOfFumbling, "potion of fumbling", "murky potion", 5, _potionImplementations.UsePotionOfFumbling), 4, 5, 3),
			new ItemDefinition(new Potion(ItemId.PotionOfExhaustion, "potion of exhaustion", "hazy potion", 5, _potionImplementations.UsePotionOfExhaustion), 5, 6, 3),
			new ItemDefinition(new Potion(ItemId.PotionOfToughness, "potion of toughness", "milky potion", 80, _potionImplementations.UsePotionOfToughness), 3, 10, 5),
			new ItemDefinition(new Potion(ItemId.PotionOfMeekness, "potion of meekness", "translucent potion", 5, _potionImplementations.UsePotionOfMeekness), 3, 5, 4),
			new ItemDefinition(new Potion(ItemId.PotionOfMinorMonsterSense, "potion of minor monster detection", "golden potion", 20, _potionImplementations.UsePotionOfMinorMonsterSense), 1, 150),
			new ItemDefinition(new Potion(ItemId.PotionOfMajorMonsterSense, "potion of major monster detection", "silvery potion", 50, _potionImplementations.UsePotionOfMajorMonsterSense), 1),
			new ItemDefinition(new Potion(ItemId.PotionOfHolyWater, "potion of holy water", "sparkling potion", 20, _potionImplementations.UsePotionOfHolyWater), 1),
			new ItemDefinition(new Potion(ItemId.PotionOfHasteSelf, "potion of haste self", "hot potion", 100, _potionImplementations.UsePotionOfHasteSelf), 1, 50),
			new ItemDefinition(new Potion(ItemId.PotionOfRaiseLevel, "potion of raise level", "cold potion", 5000, _potionImplementations.UsePotionOfRaiseLevel), 10, 5, 1),
			new ItemDefinition(new Potion(ItemId.PotionOfBlindness, "potion of blindness", "violet potion", 50, _potionImplementations.UsePotionOfBlindess), 1, 20),
			new ItemDefinition(new Potion(ItemId.PotionOfLevitation, "potion of levitation", "magenta potion", 20, _potionImplementations.UsePotionOfLevitation), 1, 40),
			new ItemDefinition(new Potion(ItemId.PotionOfTrapDetection, "potion of trap detection", "brown potion", 40, _potionImplementations.UsePotionOfTrapDetection), 1, 150),
			new ItemDefinition(new Potion(ItemId.PotionOfPoison, "potion of poison", "pink potion", 20, _potionImplementations.UsePotionOfPoison), 1, 40),
			new ItemDefinition(new Potion(ItemId.PotionOfDwarvenSpirits, "dwarven spirits", "ruby potion", 35, _potionImplementations.UsePotionOfDwarvenSpirits), 2, 10),
			new ItemDefinition(new Potion(ItemId.PotionOfElvenWine, "elven wine", "watery potion", 55, _potionImplementations.UsePotionOfElvenWine), 3, 10),
			new ItemDefinition(new Scroll(ItemId.ScrollOfMonsterConfusion, "scroll of monster confusion", "scroll labeled \"FROTZ\"", 30, _scrollImplementations.UseScrollOfMonsterConfusion), 1, 5),
			new ItemDefinition(new Scroll(ItemId.ScrollOfMagicMapping, "scroll of magic mapping", "scroll labeled \"ELBERETH\"", 30, _scrollImplementations.UseScrollOfMagicMapping), 1, 20),
			new ItemDefinition(new Scroll(ItemId.ScrollOfHoldMonster, "scroll of hold monster", "scroll labeled \"COWABOONGA\"", 40, _scrollImplementations.UseScrollOfHoldMonster), 3, 10),
			new ItemDefinition(new Scroll(ItemId.ScrollOfSleep, "scroll of sleep", "scroll labeled \"BRIE-YAAARRK\"", 2, _scrollImplementations.UseScrollOfSleep), 2, 5),
			new ItemDefinition(new Scroll(ItemId.ScrollOfEnchantArmor, "scroll of enchant armor", "scroll labeled \"GONDWANA\"", 200, _scrollImplementations.UseScrollOfEnchantArmor), 3, 7),
			new ItemDefinition(new Scroll(ItemId.ScrollOfIdentify, "scroll of identify", "scroll labeled \"CALAFREA\"", 99, _scrollImplementations.UseScrollOfIdentify), 1, 12),
			new ItemDefinition(new Scroll(ItemId.ScrollOfKnowledge, "scroll of knowledge", "scroll labeled \"CRHCRSHCRSH\"", 99, _scrollImplementations.UseScrollOfIdentify), 5, 60),
			new ItemDefinition(new Scroll(ItemId.ScrollOfEnlightenment, "scroll of enlightenment", "scroll labeled \"EGAMAD\"", 99, _scrollImplementations.UseScrollOfIdentify), 10, 120),
			new ItemDefinition(new Scroll(ItemId.ScrollOfGreaterIdentify, "scroll of greater identify", "scroll labeled \"TERGA-TRIL\"", 150, _scrollImplementations.UseScrollOfGreaterIdentify), 4, 10),
			new ItemDefinition(new Scroll(ItemId.ScrollOfUltimateIdentify, "scroll of ultimate identify", "scroll labeled \"MOORYA\"", 250, _scrollImplementations.UseScrollOfUltimateIdentify), 10, 8),
			new ItemDefinition(new Scroll(ItemId.ScrollOfScareMonster, "scroll of scare monster", "scroll labeled \"CALMYUS\"", 40, _scrollImplementations.UseScrollOfScareMonster), 4, 4),
			new ItemDefinition(new Scroll(ItemId.ScrollOfFoodDetection, "scroll of food detection", "scroll labeled \"YAKYAKYAK\"", 45, _scrollImplementations.UseScrollOfFoodDetection), 1, 50),
			new ItemDefinition(new Scroll(ItemId.ScrollOfStairDetection, "scroll of stair detection", "scroll labeled \"YAMARAMA\"", 45, _scrollImplementations.UseScrollOfStairDetection), 1, 15),
			new ItemDefinition(new Scroll(ItemId.ScrollOfSecretPassageDetection, "scroll of secret passage detection", "scroll labeled \"DAEDSINIWRAD\"", 45, _scrollImplementations.UseScrollOfSecretDoorDetection), 1, 12),
			new ItemDefinition(new Scroll(ItemId.ScrollOfTeleportation, "scroll of teleportation", "scroll labeled \"NONEST\"", 120, _scrollImplementations.UseScrollOfTeleportation), 1, 5),
			new ItemDefinition(new Scroll(ItemId.ScrollOfEnchantWeapon, "scroll of enchant weapon", "scroll labeled \"XISCAPE\"", 150, _scrollImplementations.UseScrollOfEnchantWeapon), 3, 11),
			new ItemDefinition(new Scroll(ItemId.ScrollOfMonsterCreation, "scroll of monster creation", "scroll labeled \"ACABRIS\"", 4, _scrollImplementations.UseScrollOfMonsterCreation), 1, 10),
			new ItemDefinition(new Scroll(ItemId.ScrollOfMonsterAggravation, "scroll of monster aggravation", "scroll labeled \"ACRABAS\"", 3, _scrollImplementations.UseScrollOfMonsterAggravation), 1, 20),
			new ItemDefinition(new Scroll(ItemId.ScrollOfHordeSummoning, "scroll of horde summoning", "scroll labeled \"ACRABAM\"", 7, _scrollImplementations.UseScrollOfHordeSummoning), 4, 4),
			new ItemDefinition(new Scroll(ItemId.ScrollOfAnnihilation, "scroll of annihilation", "scroll labeled \"ABRACADABRA\"", 5, _scrollImplementations.UseScrollOfAnnihilation), 3, 2),
			new ItemDefinition(new Scroll(ItemId.ScrollOfPrecision, "scroll of precision", "scroll labeled \"SSSSIKKT\"", 80, _scrollImplementations.UseScrollOfPrecision), 1, 10),
			new ItemDefinition(new Scroll(ItemId.ScrollOfPain, "scroll of pain", "scroll labeled \"HEXHEX\"", 100, _scrollImplementations.UseScrollOfPain), 1, 10),
			new ItemDefinition(new Scroll(ItemId.ScrollOfProtection, "scroll of protection", "scroll labeled \"UMBAR\"", 300, _scrollImplementations.UseScrollOfProtection), 1, 8),
			new ItemDefinition(new Scroll(ItemId.ScrollOfMight, "scroll of might", "scroll labeled \"OOMPH\"", 300, _scrollImplementations.UseScrollOfMight), 4, 7),
			new ItemDefinition(new Scroll(ItemId.PiousLessons, "scroll of pious lessons", "scroll labeled \"OMMM\"", 50, _scrollImplementations.UseScrollOfPiousLessons), 1, 7),
			new ItemDefinition(new Scroll(ItemId.MostHolyPreachings, "scroll of most holy preachings", "scroll labeled \"LALALA\"", 100, _scrollImplementations.UseScrollOfMostHolyPreachings), 4, 5),
			new ItemDefinition(new Scroll(ItemId.TreasureMap, "treasure map", "scroll labeled \"SIMSALABIM\"", 200, _scrollImplementations.UseTreasureMap), 1, 20),
			new ItemDefinition(new Scroll(ItemId.ScrollOfBlindness, "scroll of blindness", "scroll labeled \"MIBALASMIS\"", 2, _scrollImplementations.UseScrollOfBlindness), 1, 4),
			new ItemDefinition(new Scroll(ItemId.ScrollOfInvisibility, "scroll of invisibility", "scroll labeled \"ISIB\"", 100, _scrollImplementations.UseScrollOfInvisibility), 6, 5),
			new ItemDefinition(new Scroll(ItemId.HolyScriptures, "holy scriptures", "scroll labeled \"PUKSIBZNARF\"", 200, _scrollImplementations.UseHolyScriptures), 2, 7),
			new ItemDefinition(new Ring(ItemId.RingOfProtection1, "ring of protection +1", "bronze ring", ItemAbility.ArmorClassBonus, 300, 1), 2, 15),
			new ItemDefinition(new Ring(ItemId.RingOfProtection2, "ring of protection +2", "silver ring", ItemAbility.ArmorClassBonus, 600, 2), 5, 10),
			new ItemDefinition(new Ring(ItemId.RingOfProtection3, "ring of protection +3", "golden ring", ItemAbility.ArmorClassBonus, 900, 3), 9, 5),
			new ItemDefinition(new Ring(ItemId.RingOfStrength1, "ring of strength +1", "diamond ring", ItemAbility.StrengthPlusOne, 200), 1, 8),
			new ItemDefinition(new Ring(ItemId.RingOfStrength2, "ring of strength +2", "opal ring", ItemAbility.StrengthPlusTwo, 400), 5, 8),
			new ItemDefinition(new Ring(ItemId.RingOfStrength3, "ring of strength +3", "amethyst ring", ItemAbility.StrengthPlusThree, 600), 10, 8),
			new ItemDefinition(new Ring(ItemId.RingOfSustainStrength, "ring of sustain strength", "ruby ring", ItemAbility.SustainStrength, 150), 1, 15),
			new ItemDefinition(new Ring(ItemId.RingOfSeeInvisible, "ring of see invisible", "electrum ring", ItemAbility.SeeInvisible, 300), 1, 10),
			new ItemDefinition(new Ring(ItemId.RingOfAdornment, "ring of adornment", "shiny ring", ItemAbility.None, 5000), 7, 10),
			new ItemDefinition(new Ring(ItemId.RingOfAggravateMonster, "ring of monster aggravation", "rusty ring", ItemAbility.MonsterAggravation, 30), 3, 20),
			new ItemDefinition(new Ring(ItemId.RingOfDexterity1, "ring of dexterity +1", "jade ring", ItemAbility.DexterityPlusOne, 150), 1, 12),
			new ItemDefinition(new Ring(ItemId.RingOfDexterity2, "ring of dexterity +2", "agathe ring", ItemAbility.DexterityPlusTwo, 300), 4, 9),
			new ItemDefinition(new Ring(ItemId.RingOfDexterity3, "ring of dexterity +3", "copper ring", ItemAbility.DexterityPlusThree, 450), 7, 6),
			new ItemDefinition(new Ring(ItemId.RingOfIncreaseDamage2, "ring of damage +2", "black ring", ItemAbility.DamagePlusTwo, 500), 3, 5),
			new ItemDefinition(new Ring(ItemId.RingOfIncreaseDamage4, "ring of damage +4", "coal ring", ItemAbility.DamagePlusFour, 750), 7, 5),
			new ItemDefinition(new Ring(ItemId.RingOfIncreaseDamage6, "ring of damage +6", "sapphire ring", ItemAbility.DamagePlusSix, 1000), 12, 5),
			new ItemDefinition(new Ring(ItemId.RingOfRegeneration, "ring of regeneration", "tin ring", ItemAbility.Regenerates, 750), 10, 5),
			new ItemDefinition(new Ring(ItemId.RingOfSlowDigestion, "ring of slow digestion", "topaz ring", ItemAbility.SlowDigestion, 500), 15, 3),
			new ItemDefinition(new Ring(ItemId.RingOfTeleportation, "ring of teleportation", "moonstone ring", ItemAbility.None, 200, 0, 0, _ringImplementations.UseTeleportation), 5, 15),
			new ItemDefinition(new Ring(ItemId.RingOfStealth, "ring of stealth", "granate ring", ItemAbility.Stealth, 200), 1, 12),
			new ItemDefinition(new Ring(ItemId.RingOfFreeMovement, "ring of free movement", "wooden ring", ItemAbility.MoveFreely, 100), 1, 12),
			new ItemDefinition(new Ring(ItemId.RingOfMaintenance, "ring of maintenance", "turquoise ring", ItemAbility.ResistRust, 300), 8, 12),
			new ItemDefinition(new Ring(ItemId.RingOfTrapEvasion, "ring of trap evasion", "moonstone ring", ItemAbility.TrapEvasion, 250), 7, 7),
			new ItemDefinition(new Ring(ItemId.RingOfColdResistance, "ring of cold resistance", "mithril ring", ItemAbility.ResistCold, 120), 3, 10),
			new ItemDefinition(new Ring(ItemId.RingOfFireResistance, "ring of fire resistance", "adamantium ring", ItemAbility.ResistFire, 120), 3, 10),
			new ItemDefinition(new Food(ItemId.IronRation, "iron ration", 15, _foodImplementations.UseIronRation), 1),
			new ItemDefinition(new Food(ItemId.Apple, "apple", 5, _foodImplementations.UseApple), 1, 10),
			new ItemDefinition(new Food(ItemId.SlimeMold, "slime mold", 10, _foodImplementations.UseSlimeMold), 2, 15),
			new ItemDefinition(new Food(ItemId.FortuneCookie, "fortune cookie", 10, _foodImplementations.UseFortuneCookie), 1, 25),
			new ItemDefinition(new Wand(ItemId.WandOfLight, "wand of light", "ebony wand", 20, _wandImplementations.ZapWandOfLight), 1, 24, int.MaxValue, new Roll(1, 10, 9)),
			new ItemDefinition(new Wand(ItemId.WandOfInvisibility, "wand of invisibility", "glass wand", 100, _wandImplementations.ZapWandOfInvisibility), 4, 12, int.MaxValue, new Roll(1, 5, 2)),
			new ItemDefinition(new Wand(ItemId.WandOfLightning, "wand of lightning", "ebony wand", 200, _wandImplementations.ZapWandOfLightning), 5, 6, int.MaxValue, new Roll(1, 5, 2)),
			new ItemDefinition(new Wand(ItemId.WandOfFire, "wand of fire", "iron wand", 150, _wandImplementations.ZapWandOfFire), 3, 6, int.MaxValue, new Roll(1, 5, 2)),
			new ItemDefinition(new Wand(ItemId.WandOfCold, "wand of cold", "mithril wand", 200, _wandImplementations.ZapWandOfCold), 4, 6, int.MaxValue, new Roll(1, 5, 2)),
			new ItemDefinition(new Wand(ItemId.WandOfPolymorph, "wand of polymorph", "crystal wand", 300, _wandImplementations.ZapWandOfPolymorph), 6, 30, int.MaxValue, new Roll(1, 3)),
			new ItemDefinition(new Wand(ItemId.WandOfMagicMissiles, "wand of magic missiles", "ash wand", 50, _wandImplementations.ZapWandOfMagicMissiles), 1, 20, int.MaxValue, new Roll(1, 5, 2)),
			new ItemDefinition(new Wand(ItemId.WandOfHasteMonster, "wand of haste monster", "oak wand", 30, _wandImplementations.ZapWandOfHasteMonster), 1, 20, int.MaxValue, new Roll(1, 5, 2)),
			new ItemDefinition(new Wand(ItemId.WandOfSlowMonster, "wand of slow monster", "thin wand", 100, _wandImplementations.ZapWandOfSlowMonster), 1, 22, int.MaxValue, new Roll(1, 5, 2)),
			new ItemDefinition(new Wand(ItemId.WandOfDrainLife, "wand of life draining", "twisted wand", 80, _wandImplementations.ZapWandOfDrainLife), 2, 18, int.MaxValue, new Roll(1, 5, 2)),
			new ItemDefinition(new Wand(ItemId.WandOfNothing, "wand of uselessness", "rune-covered wand", 1, _wandImplementations.ZapWandOfNothing), 1, 2, int.MaxValue, new Roll(1, 5, 2)),
			new ItemDefinition(new Wand(ItemId.WandOfTeleportAway, "wand of teleportation", "pointy wand", 130, _wandImplementations.ZapWandOfTeleportAway), 1, 12, int.MaxValue, new Roll(1, 5, 2)),
			new ItemDefinition(new Wand(ItemId.WandOfTeleportBy, "wand of recall", "thick wand", 50, _wandImplementations.ZapWandOfTeleportBy), 1, 12, int.MaxValue, new Roll(1, 5, 2)),
			new ItemDefinition(new Wand(ItemId.WandOfCancellation, "wand of cancellation", "colorful wand", 20, _wandImplementations.ZapWandOfCancellation), 10, 10, int.MaxValue, new Roll(1, 5, 2)),
			new ItemDefinition(new Wand(ItemId.WandOfMidas, "wand of Midas", "golden wand", 77, _wandImplementations.ZapWandOfMidas), 1, 10, int.MaxValue, new Roll(1, 5, 2)),
			new ItemDefinition(new Wand(ItemId.WandOfDigging, "wand of digging", "bone wand", 120, _wandImplementations.ZapWandOfDigging), 3, 10, int.MaxValue, new Roll(1, 5, 2)),
			new ItemDefinition(new Wand(ItemId.WandOfWebbing, "wand of webbing", "dusty wand", 60, _wandImplementations.ZapWandOfWebbing), 1, 10, int.MaxValue, new Roll(1, 5, 2))
		};
		ItemDefinition[] items = _items;
		foreach (ItemDefinition itemDefinition in items)
		{
			_definitions.Add(itemDefinition.Template.Id, itemDefinition);
		}
		ShuffleAliasses();
	}

	private void ShuffleAliasses()
	{
		Dictionary<ItemType, Dictionary<ItemId, string>> dictionary = new Dictionary<ItemType, Dictionary<ItemId, string>>();
		foreach (ItemType key in _aliasDefinitions.Keys)
		{
			dictionary[key] = GetShuffledTypeMap(_aliasDefinitions[key]);
		}
		_aliasDefinitions = dictionary;
	}

	private Dictionary<ItemId, string> GetShuffledTypeMap(Dictionary<ItemId, string> aliasDefinition)
	{
		List<ItemId> list = new List<ItemId>(aliasDefinition.Keys);
		List<string> list2 = new List<string>(aliasDefinition.Values);
		if (list2.Count > 1)
		{
			for (int i = 0; i < list2.Count; i++)
			{
				int num;
				for (num = Game.Instance.Random(list2.Count); num == i; num = Game.Instance.Random(list2.Count))
				{
				}
				int index = i;
				List<string> list3 = list2;
				int index2 = num;
				string text = list2[num];
				string text2 = list2[i];
				string text3 = (list2[index] = text);
				text3 = (list3[index2] = text2);
			}
		}
		Dictionary<ItemId, string> dictionary = new Dictionary<ItemId, string>();
		for (int j = 0; j < list.Count; j++)
		{
			dictionary[list[j]] = list2[j];
		}
		return dictionary;
	}

	public Item CreateItem(ItemId itemId)
	{
		return _definitions[itemId].CreateItem();
	}

	public Item CreateRandomItem(DungeonLevel dungeonLevel, bool allowForCursedItems = false)
	{
		ItemDefinition randomItemDefinition = GetAvailableItems(dungeonLevel).GetRandomItemDefinition();
		return GetItemCheckedForCursedStatus(dungeonLevel, ItemModifiers.GetModifiedItem(dungeonLevel, randomItemDefinition?.CreateItem()), allowForCursedItems);
	}

	private Item GetItemCheckedForCursedStatus(DungeonLevel dungeonLevel, Item item, bool allowForCursedItems = false)
	{
		if (allowForCursedItems && item != null && item.IsEquippable && Game.Instance.Probability(dungeonLevel.Level + 7))
		{
			item.IsCursed = true;
		}
		return item;
	}

	public ListOfItemDefinitions GetItemDefinitionsByType(ItemType itemType)
	{
		ListOfItemDefinitions listOfItemDefinitions = new ListOfItemDefinitions();
		ItemDefinition[] items = _items;
		foreach (ItemDefinition itemDefinition in items)
		{
			if (itemDefinition.Template.ItemType == itemType)
			{
				listOfItemDefinitions.Add(itemDefinition);
			}
		}
		return listOfItemDefinitions;
	}

	public Item CreateRandomItemOfType(DungeonLevel dungeonLevel, ItemType type, bool allowForCursedItems = false)
	{
		ListOfItemDefinitions availableItems = GetAvailableItems(dungeonLevel);
		availableItems.FilterBy(type);
		ItemDefinition randomItemDefinition = availableItems.GetRandomItemDefinition();
		return GetItemCheckedForCursedStatus(dungeonLevel, ItemModifiers.GetModifiedItem(dungeonLevel, randomItemDefinition?.CreateItem()), allowForCursedItems);
	}

	private ListOfItemDefinitions GetAvailableItems(DungeonLevel dungeonLevel)
	{
		ListOfItemDefinitions listOfItemDefinitions = new ListOfItemDefinitions();
		ItemDefinition[] items = _items;
		foreach (ItemDefinition itemDefinition in items)
		{
			if (itemDefinition.IsAvailable(dungeonLevel))
			{
				listOfItemDefinitions.Add(itemDefinition);
			}
		}
		return listOfItemDefinitions;
	}

	public void RegisterAlias(ItemType itemType, ItemId id, string alias)
	{
		Dictionary<ItemId, string> dictionary = (_aliasDefinitions.ContainsKey(itemType) ? _aliasDefinitions[itemType] : null);
		if (dictionary == null)
		{
			dictionary = new Dictionary<ItemId, string>();
			_aliasDefinitions[itemType] = dictionary;
		}
		dictionary.Add(id, alias);
	}

	public string GetAliasFor(Item item)
	{
		return _aliasDefinitions[item.ItemType][item.Id];
	}

	public void Identify(Item item)
	{
		_identifiedItems.Add(item.Id);
		item.IsIdentified = true;
	}

	public bool IsIdentified(Item item)
	{
		return _identifiedItems.Contains(item.Id);
	}

	public void AddInformation(List<string> information)
	{
		information.Add("Item types: " + _items.Length);
	}

	public string GetPluralItemTypeName(ItemType itemType)
	{
		return PluralItemTypeNames[itemType];
	}
}
