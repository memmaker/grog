/* port: .NET Core's BinaryFormatter refuses delegates ("Serializing delegates is
   not supported on this platform"); .NET Framework wrote them through
   DelegateSerializationHolder. Grog's saves hold delegates (static lambdas on the
   compiler's [Serializable] <>c classes, methods of serializable game objects), so
   this does the same: a surrogate writes delegate type, method (declaring type,
   name, parameter types) and target; DelegateHolder rebuilds it on load. */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Grog.Kernel.FileAccess;

public sealed class DelegateSurrogateSelector : ISurrogateSelector
{
	private ISurrogateSelector _next;
	private static readonly DelegateSurrogate Surrogate = new DelegateSurrogate();

	public void ChainSelector(ISurrogateSelector selector) { _next = selector; }
	public ISurrogateSelector GetNextSelector() => _next;

	public ISerializationSurrogate GetSurrogate(Type type, StreamingContext context, out ISurrogateSelector selector)
	{
		if (typeof(Delegate).IsAssignableFrom(type))
		{
			selector = this;
			return Surrogate;
		}
		if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(HashSet<>) || type.GetGenericTypeDefinition() == typeof(Dictionary<,>)))
		{
			selector = this;
			return CollectionSurrogate.Instance;
		}
		selector = null;
		return _next?.GetSurrogate(type, context, out selector);
	}
}

internal sealed class DelegateSurrogate : ISerializationSurrogate
{
	public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
	{
		Delegate[] list = ((Delegate)obj).GetInvocationList();
		info.SetType(typeof(DelegateHolder));
		info.AddValue("type", obj.GetType().AssemblyQualifiedName);
		info.AddValue("count", list.Length);
		for (int i = 0; i < list.Length; i++)
		{
			MethodInfo m = list[i].Method;
			info.AddValue("decl" + i, m.DeclaringType.AssemblyQualifiedName);
			info.AddValue("name" + i, m.Name);
			info.AddValue("sig" + i, Signature(m));
			info.AddValue("target" + i, list[i].Target, typeof(object));
		}
	}

	public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) => throw new NotSupportedException();

	internal static string Signature(MethodInfo m) => string.Join(",", m.GetParameters().Select(p => p.ParameterType.FullName));
}

[Serializable]
internal sealed class DelegateHolder : ISerializable, IObjectReference
{
	private readonly SerializationInfo _info;

	private DelegateHolder(SerializationInfo info, StreamingContext context) { _info = info; }

	public void GetObjectData(SerializationInfo info, StreamingContext context) => throw new NotSupportedException();

	public object GetRealObject(StreamingContext context)
	{
		Type type = Type.GetType(_info.GetString("type"), throwOnError: true);
		int count = _info.GetInt32("count");
		Delegate result = null;
		for (int i = 0; i < count; i++)
		{
			Type decl = Type.GetType(_info.GetString("decl" + i), throwOnError: true);
			string name = _info.GetString("name" + i), sig = _info.GetString("sig" + i);
			MethodInfo m = decl.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
				.First(x => x.Name == name && DelegateSurrogate.Signature(x) == sig);
			object target = _info.GetValue("target" + i, typeof(object));
			result = Delegate.Combine(result, m.IsStatic ? Delegate.CreateDelegate(type, m) : Delegate.CreateDelegate(type, target, m));
		}
		return result;
	}
}

/* port: the browser-wasm runtime pack has no ISerializable constructors for
   HashSet<T> / Dictionary<K,V> (Serialization_ConstructorNotFound on load):
   write them as plain arrays, rebuild them after the graph is complete. */
internal sealed class CollectionSurrogate : ISerializationSurrogate
{
	public static readonly CollectionSurrogate Instance = new CollectionSurrogate();

	public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
	{
		Type t = obj.GetType();
		info.SetType(typeof(CollectionHolder));
		info.AddValue("type", t.AssemblyQualifiedName);
		var all = new List<object>();
		foreach (object o in (IEnumerable<object>)Enumerate(obj)) all.Add(o);
		if (IsDict(t))
		{
			// KeyValuePair<K,V> via reflection (the wasm runtime pack could not resolve System.Collections.IDictionary)
			object[] k = new object[all.Count], v = new object[all.Count];
			for (int i = 0; i < all.Count; i++) { k[i] = all[i].GetType().GetProperty("Key").GetValue(all[i]); v[i] = all[i].GetType().GetProperty("Value").GetValue(all[i]); }
			info.AddValue("keys", k);
			info.AddValue("values", v);
		}
		else
		{
			info.AddValue("items", all.ToArray());
		}
	}

	public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector) => throw new NotSupportedException();

	internal static bool IsDict(Type t) => t.GetGenericTypeDefinition() == typeof(Dictionary<,>);

	private static IEnumerable<object> Enumerate(object o)
	{
		var e = o.GetType().GetMethod("GetEnumerator", Type.EmptyTypes).Invoke(o, null);
		var next = e.GetType().GetMethod("MoveNext");
		var cur = e.GetType().GetProperty("Current");
		while ((bool)next.Invoke(e, null)) yield return cur.GetValue(e);
	}
}

[Serializable]
internal sealed class CollectionHolder : ISerializable, IObjectReference, IDeserializationCallback
{
	private readonly SerializationInfo _info;
	private object _real;
	private object[] _k, _v, _items;

	private CollectionHolder(SerializationInfo info, StreamingContext context)
	{
		_info = info;
		Type t = Type.GetType(info.GetString("type"), throwOnError: true);
		_real = Activator.CreateInstance(t);
		if (CollectionSurrogate.IsDict(t))
		{
			_k = (object[])info.GetValue("keys", typeof(object[]));
			_v = (object[])info.GetValue("values", typeof(object[]));
		}
		else
		{
			_items = (object[])info.GetValue("items", typeof(object[]));
		}
	}

	public void GetObjectData(SerializationInfo info, StreamingContext context) => throw new NotSupportedException();

	public object GetRealObject(StreamingContext context) => _real;

	// keys/items may still be half built during GetRealObject: fill in when the whole graph is done
	public void OnDeserialization(object sender)
	{
		if (_k != null)
		{
			var add = _real.GetType().GetMethod("Add");
			for (int i = 0; i < _k.Length; i++) add.Invoke(_real, new[] { _k[i], _v[i] });
		}
		else if (_items != null)
		{
			var add = _real.GetType().GetMethod("Add");
			foreach (object o in _items) add.Invoke(_real, new[] { o });
		}
	}
}
