/* RVIP web backend: runs inside a module Web Worker (web/worker.js).
   Keys: the page writes them into a SharedArrayBuffer ring; waitKey() blocks in
   Atomics.wait, so Grog's loop stays synchronous. Screen: Curses.Refresh ->
   Term.Present hands the 80x26 cell buffer (char, fg, bg; colours from the game's
   ConsoleColor palette in Term.cs) to the page. Files: everything the game writes
   lives in /grog (Term.DataRoot) and is mirrored to IndexedDB when it changes. */
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using Grog.Kernel.GCurses;
namespace GrogWeb{
	public partial class WebBackend : ITermBackend{
		[JSImport("present","grog")] internal static partial void JsPresent([JSMarshalAs<JSType.MemoryView>] Span<int> cells,int cols,int rows,int cx,int cy,bool visible,string info);
		[JSImport("waitKey","grog")] internal static partial string JsWaitKey(int timeout_ms);
		[JSImport("keyAvailable","grog")] internal static partial bool JsKeyAvailable();
		[JSImport("sleep","grog")] internal static partial void JsSleep(int ms);
		[JSImport("storeFile","grog")] internal static partial void JsStoreFile(string name,[JSMarshalAs<JSType.MemoryView>] Span<byte> data);
		[JSImport("deleteFile","grog")] internal static partial void JsDeleteFile(string name);
		[JSImport("initialFiles","grog")] internal static partial string JsInitialFiles(); //names joined by '\n'
		[JSImport("initialFile","grog")] internal static partial byte[] JsInitialFile(string name);
		[JSImport("quit","grog")] internal static partial void JsQuit();
		[JSImport("sound","grog")] internal static partial void JsSound(string name);

		public const string Dir = "/grog";
		Dictionary<string,long> stamps = new Dictionary<string,long>();

		public WebBackend(){
			Directory.CreateDirectory(Dir);
			foreach(string name in JsInitialFiles().Split('\n')){
				if(name == "" || name.StartsWith("web-") || name.Contains("/")) continue; //web-*: page settings, not game files
				File.WriteAllBytes(Path.Combine(Dir,name),JsInitialFile(name));
			}
			foreach(string f in Directory.GetFiles(Dir)) stamps[Path.GetFileName(f)] = Stamp(f);
		}
		static long Stamp(string path){
			FileInfo fi = new FileInfo(path);
			return fi.Exists ? fi.LastWriteTimeUtc.Ticks ^ (fi.Length << 40) : -1;
		}
		public void SyncFiles(){
			var seen = new HashSet<string>();
			foreach(string f in Directory.GetFiles(Dir)){
				string name = Path.GetFileName(f);
				seen.Add(name);
				long st = Stamp(f);
				if(stamps.TryGetValue(name,out long old) && old == st) continue;
				byte[] data;
				try{ data = File.ReadAllBytes(f); }
				catch(IOException){ continue; } //still open; next time
				stamps[name] = st;
				JsStoreFile(name,data);
			}
			foreach(string name in new List<string>(stamps.Keys)){
				if(seen.Contains(name)) continue;
				stamps.Remove(name);
				JsDeleteFile(name);
			}
		}
		public bool KeyAvailable(){ return JsKeyAvailable(); }
		public ConsoleKeyInfo ReadKey(){
			SyncFiles();
			while(true){
				string[] p = JsWaitKey(-1).Split('\t'); //"code\tkey\tmods" (mods: s c a)
				if(p.Length < 3) continue;
				if(Term.FromBrowser(p[0],p[1],p[2].Contains("s"),p[2].Contains("c"),p[2].Contains("a"),out ConsoleKeyInfo k)) return k;
			}
		}
		public void Sleep(int ms){ JsSleep(ms); }
		public void Beep(){ JsSound("beep"); }
		internal static void JsSoundStatic(string n){ if(n != null) JsSound(n); }
		public void Present(int[] cells,int cols,int rows,int cx,int cy,bool visible){ JsPresent(cells,cols,rows,cx,cy,visible,Term.Info != null ? Term.Info() : ""); }
	}
	public static class WebMain{
		public static void Main(string[] args){
			var b = new WebBackend();
			Term.Backend = b;
			Term.DataRoot = WebBackend.Dir;
			Term.Info = global::Grog.Dungeons.DungeonLevel.RvipInfo;
			Term.SoundOut = WebBackend.JsSoundStatic;
			try{ global::Grog.Grog.Main(new string[0]); }
			catch(Exception e){ Console.WriteLine("Grog crashed: " + e); throw; }
			b.SyncFiles();
			WebBackend.JsQuit();
		}
	}
}
