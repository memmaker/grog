# Grog: handover

Grog (2018) by **Thomas Biskup** (author of ADOM), https://www.roguelike.games/grog/.
Shipped only as a .NET Framework 4 console exe; the user approved decompiling it:
`src/` = ilspycmd 11.1.0.9782 output of `grog.exe` (sha256
e3a6b5ad49a1d3e5ce333cbe8d58b16a257f679141dd98df47bfa97266fb63d2, in-game version
1.0.2 Release 3 Build 897, assembly 1.0.0.0), first commit `1a0362c` = untouched.
`grog.exe` is not in git (`~/Downloads/grog.exe`).

All RVIP stages done. Live https://ruzzoli.de/roguelikes/grog/, repo
https://github.com/memmaker/grog (remote `memmaker`, branch `main`), shrine
https://ruzzoli.de/roguelikes/shrine/grog.html. Case **O** (C# console game).
Text only. Card/tree year 2023 (1.0.2 release; confirmed by the user).

## Quick reference

- dotnet: `export PATH="$HOME/.dotnet:$PATH" DOTNET_ROOT="$HOME/.dotnet"` (exit 127 = no dotnet on PATH).
- Web: `sh web/build.sh` → `web/dist` (5.4 MB, ~1.7 MB brotli; `TrimMode=partial` +
  `TrimmerRootAssembly mscorlib`, whose Assembly.Load in `Converter..cctor` BinaryFormatter needs).
  Deploy `web/deploy.sh` (guard: clean tree, pushed HEAD; dirty tree → deploy from a fresh clone).
- Local: `python3 web/serve.py 8431` → http://127.0.0.1:8431/grog/ (sends COOP/COEP itself, maps
  `/rvip-*.js` to rvip-tools/web). `coi-sw.js` for the server.
- Native: `cd src && dotnet build` (net10.0, `System.Runtime.Serialization.Formatters` 10.0.0 +
  `EnableUnsafeBinaryFormatterSerialization`). Plays in a real 80x26 terminal only.
- Headless (ASan substitute): `sh web/native/run-seeds.sh [first] [count] [keys]` (`web/native/Headless.cs`,
  seeded random keys, then `Game.Save(42)` and a second process loads it); env `KEYS=` (scripted keys,
  passes ---more---; `←↑→↓` = arrows, `①`–`⑨` = numpad; begin with Escape, the start script's 4th `\r`
  opens the Enter menu), `NOMON=1`, `IMMORTAL=1` (win path), `DUMP=1`, `CELLS=`; prints `BEACON …`.
  Example: `NOMON=1 DUMP=1 KEYS="$(printf 'g%.0s' $(seq 40))>>>>>" dotnet web/native/bin/Release/net10.0/GrogNative.dll 1 0`.

## Open

- Grog's own score formula gives fast wins a negative score (`(Moves − 15000) * 50` when Moves < 15000);
  the beacon sends it unchanged. Ask the user before changing.
- Not tested live: a real win (only forced `IMMORTAL=1`), browser death beacon, ghost/revenge file written by
  the web build, fresh save → reload in the trimmed build, Ctrl+Q at zero score writes no high score (likely by design).
- Status window omits the inverted HP bar.
- The "stood next to" part of the explore known-grid is not saved (explore may revisit tunnel ends once
  after load).
- The message of an item action is not visible when the item list reopens (it is in `MessageLog`).
- Sound: the WAVs are synthesized (`web/make-sounds.py`); Stage 6 allows only upstream audio for
  non-Angband games, and Grog has none but `Console.Beep` (web search still to note).

## Port notes

- **Frontend:** `src/Grog.Kernel.GCurses/Term.cs` (the only System.Console user; `ITermBackend`,
  ConsoleColor palette, browser key → ConsoleKeyInfo, key queue `Term.Push`). The game draws through
  its own `Curses` (`Curses.cs`, double-buffered); `Curses.Refresh` hands the cells to `Term.Present`.
  Web: `web/wasm/GrogWeb.csproj` (browser-wasm, `src/**/*.cs` + `web/wasm/WebBackend.cs`), runtime in a
  module worker (`web/worker.js`), keys via SharedArrayBuffer + `Atomics.wait`; page `web/grog.js`.
  With a backend: `Console.ReadLine` bypassed, `Thread.Sleep` → `Term.Sleep`, CancelKeyPress skipped.
- **Files** (BinaryFormatter + gzip, under `Term.DataRoot` = `/grog`, IndexedDB `/grog/files`):
  saves `grog<N>_v1.sg`/`.sgs` (slots 0-9; 42 = autosave), `grog_v1.opt`, `grog_v1.dft`, `grog_v1.hsc`
  + `grog.lck`, `grog_v1.kpc`, `grog_v1.gst` ghosts, `grog_v1.rmf` revenge monsters, `gcrash*.txt`.
  Loading a save deletes it (upstream). Page settings `web-layout.json` in the same store.
- **Save fixes:** delegates in saves (surrogate + holder, `DelegateSerialization.cs`); HashSet/Dictionary
  as arrays (wasm); `Being.AutomaticAction` is `[field: NonSerialized]` (saving during a run crashed).
  `ReadObject` failures logged to gcrash. Upstream bug: item list cursor past column 79.
- **Explore/stairs:** `g`, `<`/`>` walk to the nearest known stairs, the key again takes them.
  `src/Grog.Kernel/AutoExploreAction.cs` (the game's `IAutomaticAction`, one step per turn,
  `Curses.Refresh()` + `Term.Sleep(40)` per step), hooked in `src/Grog/Grog.cs`. Stops on a visible
  monster, a new message (`DungeonLevel.MessageSerial`, not tile descriptions), any key. Known grid:
  `MemoryOf != ' '`, `CanSee`, lit room with remembered corner wall, or next to a cell stood on
  (Grog remembers only walls). BFS 4-way.
- **Enter menu / inventory:** `Popup.cs` (`Popup.Choose`), `src/Grog/CommandMenu.cs`,
  `src/Grog.Dungeons/DungeonLevel.Rvip.cs` (item cursor in `ProcessItemSelectionList`, inventory
  actions as direct calls of `ProcessUseKey`/`ProcessInventoryKey`/`ProcessThrowKey`, `AutoMore`,
  `MessageLog`). Enter = menu only; interact moved to `r`.
- **Page:** RvipWM windows Map (canvas, rows 2..23), Messages, Status, Inventory, Visible; prompt line
  = row 0. `Term.Info` = `DungeonLevel.RvipInfo()` JSON (`main`, `atCmd`, `click`, `prompt`, `hero`,
  `map`, `status`, `inv`, `vis`, `log`). Whole-screen views (menus, lists, help, death) are HTML text
  (`#full pre`, sized by Messages A−/A+). Mouse: a row click sends `RvipRow` (F24, KeyChar 0xE000+row).
  Help: `web/make-help.py`. Note: while the browser pane is hidden `requestAnimationFrame` does not run;
  take a small screenshot before checking `#full`.
- **Autosave:** before the command `ReadKey` when `Moves` or the level changed → `Game.Save(42)`;
  the main loop's end deletes slot 42.
- **Beacon:** end of the run in the not-saved branch of `Grog.cs` → `Term.Beacon` → worker →
  `RvipWM.report`. win = alive + `HasLeftDungeon` + `HasGainedImmortality`; killer from
  `Player.RvipKiller` (falls back to `Game.DeathCause`, then "starvation"). Killer art:
  `roguelikes-index/killers/grog/`.
- **Docs:** `parse_grog` + `grog.html` entry in `~/Desktop/Games/Roguelikes/Docs/build-docs.py`,
  guide + Saving in `guides.py`.
- **Shrine:** `roguelikes-index/shrine/grog.html` + `shrine/grog/manual.html` (the 1.0.2 HelpText).
  Cheats: `CheatMode.cs` (Ctrl+x, male "Brannalbin" wizard); cheated runs get no high score.
