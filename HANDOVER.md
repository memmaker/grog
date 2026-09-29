# Grog: handover

Grog (2018) by **Thomas Biskup** (author of ADOM), https://www.roguelike.games/grog/.
Shipped only as a .NET Framework 4 console exe; the user approved decompiling it:
`src/` = ilspycmd 11.1.0.9782 output of `grog.exe` (sha256
e3a6b5ad49a1d3e5ce333cbe8d58b16a257f679141dd98df47bfa97266fb63d2, in-game version
1.0.2 Release 3 Build 897, assembly 1.0.0.0), first commit `1a0362c` = untouched.
`grog.exe` is not in git (`~/Downloads/grog.exe`).

## RVIP progress

### Stage 1 — Get + build (done)

- Folder `~/Games/grog`, branch `main`, no remote yet. Case **O** (C# console
  game, like O-Forays).
- **Native:** `cd src && dotnet build` (net10.0, `System.Runtime.Serialization.Formatters`
  10.0.0 + `EnableUnsafeBinaryFormatterSerialization`). Plays in a real 80x26 terminal only
  (non-tty reports 0x0 and quits).
- **Web:** `sh web/build.sh` → `web/dist` (5.4 MB, trimmed `TrimMode=partial`, mscorlib rooted for BinaryFormatter; was 23 MB untrimmed). Project
  `web/wasm/GrogWeb.csproj` (browser-wasm, compiles `src/**/*.cs` + `web/wasm/WebBackend.cs`).
  Runtime in a module worker (`web/worker.js`), keys via SharedArrayBuffer + `Atomics.wait`.
  Page `web/index.html` + `web/grog.js` (whole 80x26 screen on a canvas in the Map window;
  stage 1 only). Local test: `python3 web/serve.py 8431` → http://127.0.0.1:8431/grog/
  (sends COOP/COEP itself, maps `/rvip-*.js` to rvip-tools/web). `coi-sw.js` for the server.
- **Frontend file:** `src/Grog.Kernel.GCurses/Term.cs` (the only System.Console user;
  `ITermBackend`, ConsoleColor palette, browser key → ConsoleKeyInfo). The game draws through
  its own `Curses` (`Grog.Kernel.GCurses/Curses.cs`, double-buffered `CursesTerminal`);
  `Curses.Refresh` hands the cell buffer to `Term.Present` when a backend is set.
- **Files** (all BinaryFormatter + gzip, now all under `Term.DataRoot` = `/grog`, mirrored to
  IndexedDB `/grog/files`): saves `grog<N>_v1.sg`/`.sgs` (slots 0-9; 42 = crash autosave,
  `Game.Save(42)`), `grog_v1.opt` options, `grog_v1.dft` defaults (name/gender/type),
  `grog_v1.hsc` high scores + `grog.lck` (CommonApplicationData natively), `grog_v1.kpc`,
  `grog_v1.gst` ghosts, `grog_v1.rmf` revenge monsters, `gcrash*.txt` error dumps.
  Loading a save deletes it (upstream). Page settings `web-layout.json` in the same store.
- **Game uses:** Console colours (16 ConsoleColor, default black on white), fixed 80x26
  (`SetWindowSize`, min 70x24), `Console.ReadLine` for long input (bypassed with a backend),
  `Thread.Sleep` in missile/ray animations (→ `Term.Sleep`), `Console.Beep`, CancelKeyPress
  (skipped with a backend: throws on wasm), `Console.Title`. No P/Invoke, no Registry.
- **ASan substitute:** `sh web/native/run-seeds.sh [first] [count] [keys]` (headless backend
  `web/native/Headless.cs`, seeded random keys, then `Game.Save(42)` and a second process loads
  it). 30+ seeds × 3000–30000 keys clean after the fixes.
- **Quirks / fixes** (separate commits): delegates in saves (.NET Core BinaryFormatter refuses
  them → surrogate + holder, `DelegateSerialization.cs`); HashSet/Dictionary written as arrays
  by the same surrogate selector (wasm); item list cursor past column 79 (upstream bug).
  `ReadObject` failures now logged to gcrash (logOnly).

**Stage 1 open:** (trimming solved: root `mscorlib`, whose Assembly.Load in `Converter..cctor`
failed; tested save/load of untrimmed saves, autosave 42, options, high score, ghost/revenge files,
Export/Import; Import also fixed in `grog.js`) the page is one Map window with the whole
screen (panes/sub-windows are stage 5).

### Stage 2 — Explore + stairs (done)

- **Keys:** `g` = auto-explore, `<` / `>` = walk to the nearest known stairs of that kind and
  take them (on them: take at once). Help text (`src/Grog/Constants.cs` HelpText) lists both.
- **Code:** `src/Grog.Kernel/AutoExploreAction.cs` (`IAutomaticAction`, the game's own
  run/rest mechanism: one step per main-loop turn, a key press cancels it in the loop).
  **Hook:** `src/Grog/Grog.cs` command chain (first `if`: `g` `<` `>` → `AutoExploreAction.Start`)
  and `AutoExploreAction.Note(level, grog)` right before the command `ReadKey`.
- **Stops:** visible monster (`GetAllBeingsVisibleTo`), any new message
  (`DungeonLevel.MessageSerial`, bumped in `Message()` except tile descriptions like
  "A door." / "A stair is leading upwards.", `Tile.IsTileDescription`), any key. Avoids
  `KnownTrapFeature` cells. Grog has no closed/locked doors and no harmful terrain.
  `MoveThing` stops the game's runs at room changes and next to doors → the action re-arms
  itself after its own step.
- **"Known grid" test:** `MemoryOf(x,y) != ' '` or `Grog.CanSee(x,y)` or inside a lit room
  whose corner wall is remembered (Grog keeps only walls in memory, floors are drawn live) or
  next to a cell Grog has stood on (tunnel sides are never drawn; per-level
  `ConditionalWeakTable`, not saved). Frontier = walkable known cell with an unknown 4-neighbour;
  BFS 4-way (Grog moves 4-way only).
- **Tests:** headless `web/native` now takes `KEYS="..."` (scripted keys, passes ---more---)
  and `NOMON=1` (removes monsters each key): `NOMON=1 DUMP=1 KEYS="$(printf 'g%.0s' $(seq 40))>>>>>" dotnet web/native/bin/Release/net10.0/GrogNative.dll 1 0`
  → D:2 on 4/4 seeds (explore finishes the level, `>` walks and descends). Browser pane
  (web build): `g` stops on a monster / "feels endangered" message, resumes on `g`, walks
  corridors and doors; `<` walked back through corridor and room, stopped when a bat came into
  view. Note: the pane's `type` action sends no keydown; dispatch `KeyboardEvent` for `<` `>`.

Open from stage 2: the "stood next to" part of the known grid is lost on save/load (explore
may revisit tunnel ends once); stage 1 open items unchanged.

### Stage 3 — Enter menu + inventory (done)

- **Files:** `src/Grog.Kernel.GCurses/Popup.cs` (`Popup.Choose(title, List<PopupEntry>, cursor)`:
  game-drawn floating box, content-sized, centred, scrolls only past screen height; arrows /
  numpad 8 2 move, Enter / 5 / 6 choose, Esc / 4 / 0 / . close, entry key chooses),
  `src/Grog/CommandMenu.cs` (`CommandMenu.Show()`, every help command grouped Game / Movement /
  Resting / Dungeon / Items / Information incl. `g` `<` `>`), `src/Grog.Dungeons/DungeonLevel.Rvip.cs`
  (`ItemCursorKey`, `ProcessInventoryMain`, `InventoryItemMenu`, `AfterItemAction`, `Examine`,
  `AutoMore`, `MessageLog`). `DungeonLevel` is now `partial`.
- **Enter** at the command prompt = menu only (`Grog.cs`); interact is `r` (help text updated).
  Chosen command runs through the **key queue**: `Term.Push(key)` (`Term.ReadKey` reads pushed keys
  before the backend's).
- **Item cursor** in the game's one item prompt `ProcessItemSelectionList` (inventory, use, zap,
  throw, pick up): `>` marker, arrows / numpad 8 2, Enter / 5 chooses (= the item's letter),
  Space pages when paged. Grog has one list (equipped items highlighted), so no list switching (4/6 no-op).
- **Inventory (`i`/`e`)**: letter = main action (equip toggle, else use, else examine), Shift = drop,
  Ctrl = use, numpad + / - / * = main / drop / examine, 0 or . close; Enter / Space / 5 = item menu
  (u use, e equip, t throw, d drop, x/* examine). Item actions are **direct calls** of the game's
  `ProcessUseKey` / `ProcessInventoryKey` / `ProcessThrowKey`. After an action that took a turn the
  list closes (so monsters act) and `i` is pushed again unless a monster is in view. Any other key
  is pushed and runs as a command.
- **3d:** with a backend (`DungeonLevel.AutoMore`) `---more---` in the message line never waits
  (`Render`, `ProcessMore`); every message goes into `DungeonLevel.MessageLog` (200) for the stage 5
  message window. Paged screens (help, death `[Press '/']`, farewell) still wait.
- Headless `KEYS`: `←↑→↓` (U+2190–2193) = arrows, `①`–`⑨` = numpad 1–9. Note the start script's
  4th `\r` now opens the Enter menu: begin `KEYS` with Escape.

- **Tested:** headless (`KEYS=$'\x1b\r↓↓'` menu; `$'\x1bi↓\re'` item menu → unequip → list
  reopens; `-` drops; `m` in the list runs the monster list), 6 seeds × 5000 random keys + save/load
  clean; web build: Enter menu and inventory + numpad 2/5 item menu in the browser pane.

**Open:** no mouse in menus yet (stage 5 page); the message of an action is not visible when the
list reopens (it is in `MessageLog`). Next: stage 4 (tiles).

- Stage 4 (tiles) skipped: the user chose text only (Grog is a monochrome console game, no own tiles).

### Stage 5 — Web page (done)

- **Live:** https://ruzzoli.de/roguelikes/grog/ (GitHub memmaker/grog, remote `memmaker`).
  Deploy: `sh web/build.sh && sh web/deploy.sh` (guard: clean tree, pushed HEAD).
- **Windows** (rvip-wm): Map (canvas, screen rows 2..23, hero centred), Messages
  (`DungeonLevel.MessageLog`), Status (the two status lines), Inventory (one list, equipped
  bold), Visible (monsters from `GetAllBeingsVisibleTo`). One window = the whole 80x26 screen on
  the map canvas. Prompt line = screen row 0 over the map (multi only). Layout in IndexedDB
  `/grog/files` `web-layout.json`.
- **Game → page:** `Term.Info` (set in `WebMain`) = `DungeonLevel.RvipInfo()` JSON with every
  present: `main` (map screen up: set in `Render`, cleared by `Curses.Clear`, `Popup.Choose`,
  item lists), `atCmd` (`Grog.cs` command ReadKey), `click`, `prompt` (row 0), `hero`, `map`,
  `status`, `inv`, `vis`, `log` (only when changed). Whole-screen views (menus, lists, help,
  death) show as HTML text (`#full pre`) over the windows.
- **Mouse:** a click on a row of the Enter menu / item menus / item lists sends key
  `RvipRow` (→ `ConsoleKey.F24`, KeyChar 0xE000+row); `Popup.Choose` and
  `ProcessItemSelectionList` (`ItemClick`) map it to the entry.
- **Help** button: `web/make-help.py` (game's HelpText + browser notes) → `dist/help.html`.
  File ▾: Export / Import (all game files in one bundle) / New game (rvip-app.js).

**Open:** untrimmed 23 MB (BinaryFormatter + trimming, not retried); no autosave from the page
(Grog saves only with Q = save & quit; the crash autosave slot 42 exists); no Docs page entry
(step 6); the status HP bar (inverse part) is not shown in the Status window; single-window
mode shows the whole screen without hero centring when it fits.


### Stage 6 — Docs + sound (done)

- **Docs:** `~/Desktop/Games/Roguelikes/Docs` (not a git repo): `parse_grog` + `GAMES` entry
  `grog.html` in `build-docs.py` (keys parsed from `Constants.cs` HelpText + 8 extra rows; the
  builder asserts > 30 rows), guide + `SAVING['grog.html']` in `guides.py`. `web/make-help.py`
  adds Saving, guide, Tips and Credits from the Docs when present (game text only otherwise).
  Credits: Thomas Biskup, free download, all rights reserved (title screen), binary only.
- **Sound:** `Term.Sound(name)` → `Term.SoundOut` (set in `WebBackend`) → worker `sound` →
  page plays `sound/<name>.wav` when Audio ▾ → Sound effects is on (off by default, kept in
  `web-layout.json`). Names: hit, hurt (`Being.SufferDamage`), death (`Grog.cs` "You died"),
  pickup (`DungeonLevel` pick up, `GoldFeature`), stairs (`DungeonMaster.Descend/AscendLevel`),
  levelup (`Player.GainExperience`), beep (`Console.Beep`). WAVs synthesized by
  `web/make-sounds.py` (run by `build.sh`). No music.
- **Autosave:** `Grog.cs` before the command `ReadKey`: with a backend, no automatic action,
  no key pending and `Moves` changed → `Game.Save(42)` (the game's own crash slot). The main
  loop's end already deletes slot 42 (death, quit, save). At start the slot-42 prompt reads
  "found your autosaved game" in the browser. Tested: reload continues, Ctrl+q deletes it.

**Next:** stage 7 (publish).

### Stage 7 — Publish (done)

- Repo memmaker/grog (remote `memmaker`, branch `main`; base = decompiled grog.exe 1.0.2 @ `1a0362c`).
- Card + tree on the selection page (roguelikes-index `7a2e525`): year 2023 (1.0.2, roguelike.games),
  tree `insp` under Rogue ("1.0.0 2022, begun 2018"). Card image `img/grog.png` = 68x18 cells of a
  headless run (`NOMON=1 KEYS=<Esc>ggg… CELLS=file`, seed 5; `Headless.cs` writes raw cells), drawn
  in Menlo with the game's palette. og block in `web/index.html`.
- Next: stage 8 shrine (no manual file; title screen "All rights reserved").

### Stage 8 — Shrine (done)

- Page `roguelikes-index/shrine/grog.html` + `shrine/grog/manual.html` (the original 1.0.2
  `HelpText` from `1a0362c`, one `<pre>` per topic; no manual file exists, game is "all rights
  reserved" freeware, so nothing else copied). Card Info + tree ✦ (roguelikes-index `c8d95af`),
  game title links to it (`web/index.html`).
- Cheats: `CheatMode.cs` (Ctrl+x) opens only for a male "Brannalbin" of type "wizard";
  kept in our build; cheated runs get no high score.
- No walkthrough found. Next: stage 9 (graveyard + leaderboard).

### Stage 9 — Graveyard + leaderboard (done)

- Hook: `src/Grog/Grog.cs`, end of the run in the not-saved branch (before ghosts/revenge monsters
  are stored) → `Term.Beacon` (`Term.cs`, builds + URL-encodes the query, `Term.BeaconOut`) →
  `WebBackend.JsBeacon` → worker `beacon` postMessage → `web/grog.js` `RvipWM.report`.
- ev: death = `!IsAlive`; win = alive + `HasLeftDungeon` + `HasGainedImmortality` (up the level-1
  stairs after the throne); quit = Ctrl+Q or fleeing up the level-1 stairs mortal. Save & quit sends nothing.
- Fields: g, ev, name (ChristenedName), killer (death only; `Player.RvipKiller` set in
  `GetKillerName`: "<christened> the <type>" or type, so revenge monsters keep their name,
  never the dark-room "something"; non-monster deaths send the game's cause, e.g. "starvation"),
  depth (current level), score (`HighscoreManager.GetScoreFor − InitialScore`, as the list),
  turns (Moves), lvl. Nothing missing. Cheated runs are reported too.
- Tests: headless (`Headless.cs` prints `BEACON …`; `IMMORTAL=1` env for the win path):
  `IMMORTAL=1 NOMON=1 KEYS="<Esc>g×40<y/" + Esc/space` → ev=win; deaths with killers
  "Baulnali the zombie" (revenge), "kobold", "starvation". Browser (local): Ctrl+Q quit → beacon
  URL; outbox 503 keeps it, 204 + `RvipWM.flush()` empties it. `web/coi-sw.js` already had the null-body fix.
- Headless needs `export PATH="$HOME/.dotnet:$PATH" DOTNET_ROOT="$HOME/.dotnet"` (exit 127 = no dotnet on PATH).
- Killer art: `roguelikes-index/killers/grog/` (53, glyph black on white in Menlo, `make.py grog`).
- Open: upstream score formula subtracts for a fast immortal exit (`(Moves − 15000) * 50` when
  Moves < 15000) → a quick win can score negative; sent as the game computes it.
