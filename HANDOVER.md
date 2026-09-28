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

**Next:** stage 2 (explore + stairs). Open: trimmed build breaks BinaryFormatter
(`Converter` type initializer) → shipped untrimmed; the page is one Map window with the whole
screen (panes/sub-windows are stage 5).
