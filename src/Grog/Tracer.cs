using System;
using Grog.Kernel.GCurses;
using System.IO;
using System.Text;
using Grog.Kernel.FileAccess;

namespace Grog;

public static class Tracer
{
	private static StreamWriter _traceFile;

	public static void Trace(string message)
	{
	}

	private static void EnsureInitialization()
	{
		if (_traceFile == null)
		{
			try
			{
				string fullPath = FileManager.GetFullPath("grogtrace.txt");
				File.Delete(fullPath);
				_traceFile = new StreamWriter(new FileStream(fullPath, FileMode.Create, FileAccess.ReadWrite, FileShare.None), Encoding.UTF8);
			}
			catch (Exception ex)
			{
				Term.ForegroundColor = ConsoleColor.Black;
				Term.BackgroundColor = ConsoleColor.White;
				Term.WriteLine("\n\nFailed to create the trace file:\n\n" + ex.StackTrace);
				Term.ReadLine();
			}
			AppDomain.CurrentDomain.ProcessExit += CloseTraceOnExit;
		}
	}

	private static void CloseTraceOnExit(object sender, EventArgs e)
	{
		_traceFile?.Close();
		_traceFile = null;
	}
}
