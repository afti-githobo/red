using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Red.Sys
{
    [GlobalClass]
    public partial class MenuSystem : Control, ISingleton<MenuSystem>
    {
        private Stack<IManagedMenu> menuStack = new Stack<IManagedMenu>();
        private Dictionary<Type, IManagedMenu> menuDict = new Dictionary<Type, IManagedMenu>();

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            ((ISingleton<MenuSystem>)this).__Ready();
            var children = GetChildren();

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] is IManagedMenu)
                {
                    var type = children[i].GetType();
                    Trace.Assert(!menuDict.ContainsKey(type), "Should only be one menu of a given type");
                    menuDict[type] = (IManagedMenu)children[i];
                }
            }
        }

        public override void _ExitTree()
        {
            ((ISingleton<MenuSystem>)this).__ExitTree();
        }

        public bool HasMenu<T>() where T : IManagedMenu => menuDict.ContainsKey(typeof(T));

        public async Task OpenMenu<T>(params string[] args)
        {
            GD.Print("Opening menu: " + typeof(T));
            Trace.Assert(menuDict.ContainsKey(typeof(T)));
            var menu = menuDict[typeof(T)];
            IManagedMenu menuBelow;
            menuStack.TryPeek(out menuBelow);
            menuStack.Push(menu);
            var surrenderFocus = menuBelow?.SurrenderFocusTo(menu) ?? Task.CompletedTask;
            await Task.WhenAll(surrenderFocus, menu.Open(args));
            GD.Print("Done!");
        }

        public async Task CloseMenu<T>()
        {
            GD.Print(menuStack.Peek().GetType());
            Trace.Assert(menuStack.Peek().GetType() == typeof(T), $"Cannot close menu of type {typeof(T).Name} if not on top of MenuStack");
            GD.Print("Closing menu: " + typeof(T));
            var menu = menuStack.Pop();
            IManagedMenu menuBelow;
            menuStack.TryPeek(out menuBelow);
            var reclaimFocus = menuBelow?.ReclaimFocusFrom(menu) ?? Task.CompletedTask;
            await Task.WhenAll(reclaimFocus, menu.Close());
            GD.Print("Done!");
        }
    }

}