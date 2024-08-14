using Godot;
using System;
using System.Runtime.CompilerServices;

namespace Red
{
    public interface ISingleton<T> where T : Node
    {
        public static T Instance { get; private set; }

        public void __Ready()
        {
            if (Instance != null) throw new Exception($"There should never be more than one {typeof(T).Name} in the scene graph!");
            if (!(this is T)) throw new Exception($"ISingleton<{typeof(T).Name}> must be implemented by {typeof(T).Name}");
            Instance = (T)this;
        }

        public void __ExitTree()
        {
            Instance = null;
        }
    }

    public static partial class Singleton
    {
        public static T InstanceOf<T>() where T : Node { return ISingleton<T>.Instance; }
    }
}