using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;

namespace Grog.Kernel.FileAccess;

public static class FileManager
{
	public const Environment.SpecialFolder GlobalDataFolder = Environment.SpecialFolder.CommonApplicationData;

	private const string GrogDirectory = "Grog";

	private const string GrogLockFileName = "grog.lck";

	public static void LockFileAccess()
	{
		using FileStream fileStream = new FileStream(GetFullPath("grog.lck", Environment.SpecialFolder.CommonApplicationData), FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, FileShare.Delete);
		fileStream.WriteByte(42);
	}

	public static void UnlockFileAccess()
	{
		string fullPath = GetFullPath("grog.lck", Environment.SpecialFolder.CommonApplicationData);
		using (new FileStream(fullPath, FileMode.OpenOrCreate, System.IO.FileAccess.ReadWrite, FileShare.Delete))
		{
			File.Delete(fullPath);
		}
	}

	public static OT ReadObject<OT>(string fileName, Func<OT> getDefault, Environment.SpecialFolder folder = Environment.SpecialFolder.Personal)
	{
		string fullPath = GetFullPath(fileName, folder);
		if (File.Exists(fullPath))
		{
			try
			{
				Stream stream = new GZipStream(File.OpenRead(fullPath), CompressionMode.Decompress);
				try
				{
					return (OT)new BinaryFormatter().Deserialize(stream);
				}
				finally
				{
					stream.Close();
				}
			}
			catch (Exception)
			{
				return getDefault();
			}
		}
		return getDefault();
	}

	public static string GetFullPath(string fileName, Environment.SpecialFolder folder = Environment.SpecialFolder.Personal)
	{
		string text = Path.Combine(Environment.GetFolderPath(folder), "Grog");
		if (!Directory.Exists(text))
		{
			Exception ex = null;
			DirectoryInfo directoryInfo = null;
			try
			{
				directoryInfo = Directory.CreateDirectory(text);
			}
			catch (Exception ex2)
			{
				ex = ex2;
			}
			if (ex != null || !directoryInfo.Exists)
			{
				Game.RaiseError("Failed to create game directory '" + text + "'. Storing everything in the current path." + ((ex != null) ? ("\n\n" + ex.StackTrace) : ""), logOnly: true);
				return Path.GetFullPath(fileName);
			}
		}
		return Path.Combine(text, fileName);
	}

	public static void WriteObject<OT>(string fileName, OT data, Environment.SpecialFolder folder = Environment.SpecialFolder.Personal)
	{
		string fullPath = GetFullPath(fileName, folder);
		try
		{
			Stream stream = new GZipStream(File.OpenWrite(fullPath), CompressionMode.Compress);
			try
			{
				new BinaryFormatter().Serialize(stream, data);
			}
			finally
			{
				stream.Close();
			}
		}
		catch (Exception ex)
		{
			Game.RaiseError("Error while trying to write '" + fullPath + "': " + ex.Message + "\n" + ex.StackTrace);
		}
	}

	public static void DeleteFile(string fileName, Environment.SpecialFolder folder = Environment.SpecialFolder.Personal)
	{
		File.Delete(GetFullPath(fileName, folder));
	}

	public static void WriteText(string fileName, string text, Environment.SpecialFolder folder = Environment.SpecialFolder.Personal)
	{
		string fullPath = GetFullPath(fileName, folder);
		try
		{
			File.WriteAllText(fullPath, text);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error while trying to write '" + fullPath + "': " + ex.Message + "\n" + ex.StackTrace);
			Console.WriteLine("\nPress ENTER to continue...");
			Console.ReadLine();
		}
	}
}
