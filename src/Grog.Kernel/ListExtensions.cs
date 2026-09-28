using System.Collections.Generic;

namespace Grog.Kernel;

public static class ListExtensions
{
	public static void Shuffle<T>(this List<T> list)
	{
		if (list.Count >= 2)
		{
			for (int i = 0; i < list.Count; i++)
			{
				int num = Game.Instance.Random(list.Count);
				int num2 = Game.Instance.Random(list.Count);
				int index = num;
				int index2 = num2;
				T val = list[num2];
				T val2 = list[num];
				T val3 = (list[index] = val);
				val3 = (list[index2] = val2);
			}
		}
	}
}
