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
- **Web:** `sh web/build.sh` → `web/dist` (23 MB, **untrimmed**). Project
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

**Stage 1 open:** trimmed build breaks BinaryFormatter
(`Converter` type initializer) → shipped untrimmed; the page is one Map window with the whole
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
