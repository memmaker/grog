using System;
using System.Collections.Generic;

namespace Grog.Dressings.MapElements;

public class InteractionContext
{
	private readonly List<Action> _actions = new List<Action>();

	public void AddInteraction(Action action)
	{
		_actions.Add(action);
	}

	public void Interact()
	{
		foreach (Action action in _actions)
		{
			action();
		}
	}
}
