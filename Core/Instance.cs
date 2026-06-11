using Il2CppInterop.Runtime;
using System;
using System.Linq;
using System.Reflection;

namespace BarkFart.Core;

public class Instance : MonoWrap
{
    internal static Instance instance;

    internal void Awake()
    {
        instance = this;
        Logger.Log(@$"{Constants.Name} {Constants.GUID} {Constants.Version}");

        var types = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && t.GetCustomAttribute<AddOnAwake>() != null);
        foreach (var type in types)
            gameObject.AddComponent(Il2CppType.From(type));
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class AddOnAwake : Attribute
    {
    }

    public interface IPlugin
    {
        string Name { get; }
        string Version { get; }

        void Initialize();
    }
}