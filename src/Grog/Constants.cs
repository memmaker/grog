namespace Grog;

public static class Constants
{
	public const string HighscoreFileName = "grog_v1.hsc";

	public const string DefaultsFileName = "grog_v1.dft";

	public const string RevengeMonstersFileName = "grog_v1.rmf";

	public const string GhostFile = "grog_v1.gst";

	public const string OptionsFileName = "grog_v1.opt";

	public static readonly string[] HelpText = new string[]
	{
		"--- Online Help ---", "|", "| Grog is controlled by case-sensitive key presses:", "|", "|   <Ctrl> + q ................ Quit the game", "|   Q ......................... Save & quit", "|   o ......................... Configure options", "|", "|   w or Cursor-Up (*) ........ Move north", "|   a or Cursor-Left (*) ...... Move west",
		"|   s or Cursor-Down (*) ...... Move south", "|   d or Cursor-Right (*) ..... Move east", "|   <Shift> + <Movement-Key> .. Move quickly", "|", "|   <SPACE> ................... Wait for one turn", "|   . ......................... Wait until fully healed", "|", "|   r ......................... Interact (e.g. use stairs)", "|   <ENTER> ................... Command menu", "|   g ......................... Explore (stops on danger/news)", "|   < or > .................... Walk to stairs (again: take)", "|   e or i .................... Manage your inventory", "|",
		"|   c or , .................... Pick up items", "|   f or u .................... Use item", "|   z ......................... Zap wand", "|   x or t .................... Throw an item", "|   p ......................... Pray for help", "|", "|   m ......................... Enemy (monster) list", "|", "|   <Ctrl> + r ................ Redraw the screen", "|   h or ? or <ESC> or <HELP> . Show the help menu",
		"|   v ......................... Display game version", "|", "| (*) Cursor keys behave weirdly on some terminals.", "|", "--- More ---", "|   At various places in the game '---more---' prompts", "|   show up. They are inserted to give you the time needed", "|   to consume the information presented. Passing these", "|   prompts in the message line at the top of the game", "|   screen can be sped up by pressing ENTER which skips",
		"|   all further prompts.", "|   ", "--- Character Generation ---", "|   Your name, gender and type deterministically generate", "|   your initial attributes and starting equipment. Feel", "|   free to experiment to find an adventurer that suits", "|   your particular needs.", "|   ", "--- Rules ---", "|   Your character is described by a number of statistics",
		"|   shown in the status bar at the bottom of the screen.", "|   These are:", "|   * Armor Class (AC): How hard it is for monsters to hit", "|     your character. Higher is better.", "|   * Hit Points (H): Your life energy. When they drop to", "|     zero your character dies and is gone forever.", "|   * Strength (St): Raw physical might. Affects the", "|     number of items you can carry as well as hit", "|     probabilities and damage in melee combat.", "|   * Dexterity (Dx): Affects Armor Class (AC) and how",
		"|     fast you move in comparison to monsters.", "|   * Constitution (Cn): Health and toughness. Affects", "|     your Hit Points (H).", "|   ", "|   Additionally the game shows your current dungeon level", "|   (D), the amount of gold carried ($), the number of", "|   moves executed so far (M) and your experience (XP).", "|   Experience shows your current level, followed by a", "|   slash followed by the number of experience points", "|   needed to advance to next level. Experience is gained",
		"|   from killing monsters.", "|   ", "|   Finally 'Piety' is shown - which basically is an", "|   indicator on whether the player character can safely", "|   utter more prayers (via the 'p' command) or not.", "|   ", "--- Enemy (monster) list ---", "|   The enemy list shows data for all enemies in sight.", "|   Depending on your experiences with the monster in question", "|   the following data might be available:",
		"|   * Name", "|   * AC (Armor Class, how hard it is to hit the monster)", "|   * HD (Hit Dice, the number of dice thrown to determine", "|     the hit points of the monster", "|   * H: the actual and maximum hit points", "|   * #A: the number of melee attacks made by the monster", "|     during a single turn", "|   * D: the amount of damage caused by the available", "|     melee attacks", "|   ",
		"--- Hints ---", "|   * Run if you can't win.", "|   * Think smart. There are surprisingly many strategies", "|     available to get out of tight situations.", "|   * Pray in times of need. But pray sparingly because", "|     your god might get annoyed when pestered too often.", "|   * Carrying items for prolonged periods of time will", "|     identify them.", "|   * Remember that you also can throw stuff at monsters.", "|   ",
		"--- Credits ---", "|   * Thanks to the organizers of Roguelike Celebration", "|     for allowing me to experience the original Rogue", "|     on an original PDP-11 (during Roguelike Celebration", "|     2018). It was a marvelous experience and the", "|     morning afterwards Grog was born as a nod to this", "|     amazing game.", "|   * Slashie (Santiago Zapata) for being the torchbearer", "|     of the roguelike world, many great discussions about", "|     game design and friendship across continents.",
		"|   * Ignacio Bergkamp for his lightning talk at Roguelike", "|     Celebration 2018 (there you go again ;-) ) providing", "|     the idea of allowing players to add custom flavor", "|     texts to highscore entries - Grog probably was the", "|     game to deliver on this idea.", "|   * Andrew Clifton for his talk at Roguelike Celebration", "|     called 'Don't generate, hash!' from which I took", "|     inspiration and code in order to create grog player", "|     characters in a stable and platform-independent", "|     manner.",
		"|   * Krzysztof Dycha for relentless play testing, bug", "|     squashing and invaluable design discussions.", "|", "|  --- Axes High!", "|      Thomas Biskup, { [Ultimate] ADOM | Grog } Maintainer"
	};
}
