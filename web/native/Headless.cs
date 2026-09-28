/* RVIP ASan substitute: runs Grog natively with a seeded random key feed.
   usage: GrogNative <seed> <keys> [save]   (DataRoot = current folder)
   "save": after <keys> keys, Escape x3, Q (save & quit), y, slot 0, Y.
   A second run with a saved game answers y + 0 at "load a saved game" first.
   Exit 0 = ok, 2 = unhandled exception, 3 = the game raised an internal error. */
using System;
using System.IO;
using System.Collections.Generic;
using Grog.Kernel.GCurses;
namespace GrogHeadless{
	class KeysDone : Exception {}
	class Headless : ITermBackend{
		Random rng; int left; bool save; Queue<ConsoleKeyInfo> script = new Queue<ConsoleKeyInfo>();
		public int[] last; public int cols, presents; public bool internalError;
		static readonly string pool = "abcdefghijklmnoprstuvwxyzABCDEFGHIJKLMNOPRSTUVWXYZ0123456789<>.,;:?/!@#$%^&*()-=+[]{}'\" \r\x1b";
		public Headless(int seed,int keys,bool save_){ rng = new Random(seed); left = keys; save = save_; }
		public static ConsoleKeyInfo K(char ch){
			ConsoleKeyInfo k;
			if(ch == '\r') Term.FromBrowser("Enter","Enter",false,false,false,out k);
			else if(ch == '\x1b') Term.FromBrowser("Escape","Escape",false,false,false,out k);
			else Term.FromBrowser("",ch.ToString(),false,false,false,out k);
			return k;
		}
		public void Script(string s){ foreach(char c in s) script.Enqueue(K(c)); }
		static readonly string[] specials = {"ArrowUp","ArrowDown","ArrowLeft","ArrowRight","Numpad1","Numpad2","Numpad3","Numpad4","Numpad5","Numpad6","Numpad7","Numpad8","Numpad9","Home","End","PageUp","PageDown","Backspace"};
		public bool KeyAvailable(){ return false; }
		public ConsoleKeyInfo ReadKey(){
			if(Environment.GetEnvironmentVariable("NOMON") != null){ //scripted tests: no monsters on the level
				var lv = global::Grog.Kernel.Game.Instance?.DungeonMaster?.CurrentDungeonLevel;
				if(lv != null && lv.Grog != null) foreach(var t in new System.Collections.Generic.List<global::Grog.Dressings.Thing>(lv.Things)) if(t is global::Grog.Dressings.Beings.Being && t != lv.Grog && t.X >= 0) lv.RemoveThing(t.X, t.Y);
			}
			string scr = Dump();
			if(scr.Contains("Internal Error")) internalError = true;
			if(script.Count > 0 && scr.Contains("---more---") && Environment.GetEnvironmentVariable("KEYS") != null) return K(' '); //scripted tests: pass --more-- prompts
			if(script.Count > 0) return script.Dequeue();
			if(scr.Contains("play again")) HeadlessMain.Finish("game over");
			if(left <= 0){
				string how = "keys used up";
				if(save && global::Grog.Kernel.Game.Instance?.Grog != null && !(save = false)) how = "autosave " + global::Grog.Kernel.Game.Save(42);
				HeadlessMain.Finish(how); //the game catches every exception in its loop: exit from here
			}
			--left;
			if(scr.Contains("[Press '!' to continue]")) return K('!');
			int roll = rng.Next(100);
			if(roll < 60){ ConsoleKeyInfo k; Term.FromBrowser(specials[rng.Next(specials.Length)],"",false,false,false,out k); return k; }
			return K(pool[rng.Next(pool.Length)]);
		}
		public void Sleep(int ms){}
		public void Beep(){}
		public void Present(int[] cells,int c,int r,int x,int y,bool vis){ last = cells; cols = c; ++presents; }
		public string Dump(){
			if(last == null) return "";
			var sb = new System.Text.StringBuilder();
			for(int i=0;i<last.Length/3;++i){ int ch = last[i*3]; sb.Append(ch < 32 ? ' ' : (char)ch); if((i+1)%cols==0) sb.Append('\n'); }
			return sb.ToString();
		}
	}
	static class HeadlessMain{
		static int Main(string[] args){
			int seed = args.Length > 0 ? int.Parse(args[0]) : 1;
			int keys = args.Length > 1 ? int.Parse(args[1]) : 2000;
			bool save = args.Length > 2 && args[2] == "save";
			var h = new Headless(seed,keys,save);
			Term.Backend = h;
			Term.DataRoot = Directory.GetCurrentDirectory();
			H = h; Seed = seed; Loaded = File.Exists("grog42_v1.sg");
			h.Script(Loaded ? " " : "\r\r\r\r"); h.Script(Environment.GetEnvironmentVariable("KEYS") ?? ""); //KEYS: scripted test keys
			try{ global::Grog.Grog.Main(new string[0]); }
			catch(Exception e){ Console.WriteLine("UNHANDLED: " + e); Environment.Exit(2); }
			Finish("main returned");
			return 0;
		}
		static Headless H; static int Seed; static bool Loaded;
		public static void Finish(string how){
			var h = H; int rc = 0; int seed = Seed;
			if(h.internalError || Directory.GetFiles(".","gcrash*").Length > 0){
				rc = 3;
				foreach(var f in Directory.GetFiles(".","gcrash*")) Console.WriteLine(f + ": " + File.ReadAllText(f));
			}
			Console.WriteLine("seed " + seed + " " + how + " loaded " + Loaded + " presents " + h.presents + " files " + string.Join(",", Array.ConvertAll(Directory.GetFiles("."), Path.GetFileName)) + " rc " + rc);
			if(Environment.GetEnvironmentVariable("DUMP") != null) Console.WriteLine(h.Dump());
			Environment.Exit(rc);
		}
	}
}
