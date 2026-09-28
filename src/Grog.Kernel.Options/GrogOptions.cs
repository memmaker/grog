using System;
using System.Collections.Generic;
using Grog.Dressings.Beings;
using Grog.Dungeons;
using Grog.Kernel.FileAccess;
using Grog.Kernel.GCurses;
using Grog.Kernel.Interactions;

namespace Grog.Kernel.Options;

[Serializable]
public class GrogOptions
{
	private static readonly string[] ScreenColorModes = new string[5] { "Black on White", "White on Black", "Green on Black", "Yellow on Black", "White on Blue" };

	private static readonly string[] DamageNotationModes = new string[2] { "Dice (xdy+z)", "Range (a-b)" };

	private static readonly string[] SixthSenseModes = new string[2] { "Pause", "Do not pause" };

	public ScreenColor ScreenColor { get; private set; }

	public DamageNotation DamageNotation { get; private set; }

	public SixthSense SixthSense { get; private set; }

	public GrogOptions()
	{
		ScreenColor = ScreenColor.BlackOnWhite;
		DamageNotation = DamageNotation.Dice;
		SixthSense = SixthSense.NonPausing;
	}

	public List<Interaction> GetOptions(DungeonLevel dungeonLevel)
	{
		return new List<Interaction>
		{
			new Interaction('c', "Screen color ........... " + ScreenColorModes[(int)ScreenColor], dungeonLevel, SwitchScreenColor),
			new Interaction('d', "Damage notation ........ " + DamageNotationModes[(int)DamageNotation], dungeonLevel, SwitchDamageNotation),
			new Interaction('s', "Sixth sense messages ... " + SixthSenseModes[(int)SixthSense], dungeonLevel, SwitchSixthSense)
		};
	}

	private bool SwitchScreenColor(DungeonLevel dungeonLevel, Player grog, int x, int y)
	{
		ScreenColor = (ScreenColor)((int)(ScreenColor + 1) % 5);
		InitializeGameBasedOnOptions();
		dungeonLevel.Render();
		return false;
	}

	private bool SwitchDamageNotation(DungeonLevel arg1, Player arg2, int arg3, int arg4)
	{
		DamageNotation = (DamageNotation)((int)(DamageNotation + 1) % 2);
		return false;
	}

	private bool SwitchSixthSense(DungeonLevel arg1, Player arg2, int arg3, int arg4)
	{
		SixthSense = (SixthSense)((int)(SixthSense + 1) % 2);
		return false;
	}

	public void Configure(DungeonLevel dungeonLevel)
	{
		dungeonLevel.InitiateComplexInteraction("Configure options:", () => GetOptions(dungeonLevel));
		Save();
		InitializeGameBasedOnOptions();
		dungeonLevel.Render();
	}

	private void Save()
	{
		FileManager.WriteObject("grog_v1.opt", this);
	}

	public static GrogOptions Load()
	{
		return FileManager.ReadObject("grog_v1.opt", () => new GrogOptions());
	}

	public void InitializeGameBasedOnOptions()
	{
		switch (ScreenColor)
		{
		case ScreenColor.BlackOnWhite:
			Curses.Instance.InitializeColorSystem(ConsoleColor.Black, ConsoleColor.White);
			break;
		case ScreenColor.WhiteOnBlack:
			Curses.Instance.InitializeColorSystem(ConsoleColor.White, ConsoleColor.Black);
			break;
		case ScreenColor.GreenOnBlack:
			Curses.Instance.InitializeColorSystem(ConsoleColor.Green, ConsoleColor.Black);
			break;
		case ScreenColor.YellowOnBlack:
			Curses.Instance.InitializeColorSystem(ConsoleColor.Yellow, ConsoleColor.Black);
			break;
		case ScreenColor.WhiteOnBlue:
			Curses.Instance.InitializeColorSystem(ConsoleColor.White, ConsoleColor.Blue);
			break;
		}
	}
}
