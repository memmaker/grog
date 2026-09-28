using System;

namespace Grog.Kernel;

public class GrogException : Exception
{
	public GrogException(string message)
		: base(message)
	{
	}
}
