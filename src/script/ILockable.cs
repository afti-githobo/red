using Godot;
using System;
using System.Collections.Generic;

namespace Red
{
    public interface ILockable
    {
        public struct LockStruct
        {
            public readonly Node node;
            public int depth;

            public LockStruct(Node node, int depth)
            {
                this.node = node;
                this.depth = depth;
            }
        }

        public LinkedList<LockStruct> Locks { get; }

        public void LockAs(Node node)
        {
            GD.Print($"{GetType()} lock being placed by {node.Name}");
            if (Locks.Count > 0)
            {
                var cur = Locks.First;
                while (cur.ValueRef.node != node && cur != Locks.Last)
                    cur = cur.Next;
                if (cur.ValueRef.node == node) cur.ValueRef.depth++;
                else Locks.AddLast(new LinkedListNode<LockStruct>(new LockStruct(node, 1)));
            }
            else Locks.AddLast(new LinkedListNode<LockStruct>(new LockStruct(node, 1)));
        }

        public void UnlockAs(Node node)
        {
            GD.Print($"{GetType()} lock being removed by {node.Name}");
            if (Locks.Count > 0)
            {
                var cur = Locks.First;
                while (cur.ValueRef.node != node && cur != Locks.Last)
                    cur = cur.Next;
                if (cur.ValueRef.node != node) throw new Exception($"Should not be attempting to unlock as {node} b/c no lock exists");
                else cur.ValueRef.depth--;
                if (cur.ValueRef.depth == 0) Locks.Remove(cur.ValueRef);
            }
            else throw new Exception($"Should not be attempting to unlock as {node} b/c no lock exists");
        }
    }
}