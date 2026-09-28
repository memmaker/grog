/* port: .NET Core's BinaryFormatter refuses delegates ("Serializing delegates is
   not supported on this platform"); .NET Framework wrote them through
   DelegateSerializationHolder. Grog's saves hold delegates (static lambdas on the
   compiler's [Serializable] <>c classes, methods of serializable game objects), so
   this does the same: a surrogate writes delegate type, method (declaring type,
   name, parameter types) and target; DelegateHolder rebuilds it on load. */
using System;
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
