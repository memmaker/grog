using System;
using Grog.Kernel.FileAccess;

namespace Grog.Kernel;

[Serializable]
public class GrogDefaults
{
	public string Name { get; set; }

	public string Gender { get; set; }

	public string Type { get; set; }

	public GrogDefaults()
	{
		Name = "Brak";
		Gender = "male";
		Type = "grog";
	}

	public void Save()
	{
		FileManager.WriteObject("grog_v1.dft", Game.Instance.Defaults);
	}
}
