using System;
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
				Console.ForegroundColor = ConsoleColor.Black;
				Console.BackgroundColor = ConsoleColor.White;
				Console.WriteLine("\n\nFailed to create the trace file:\n\n" + ex.StackTrace);
				Console.ReadLine();
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
