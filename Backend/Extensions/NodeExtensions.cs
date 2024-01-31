using Godot;
using System.Collections.Generic;
using System.Linq;

namespace Godot
{
    public static class NodeExtensions
    {
        public static T? GetChildByType<T>(this Node node)
            where T : class
        {
            foreach (var child in node.GetChildren())
            {
                var type = child.GetType();
                if (child is T) return child as T;
            }

            return null;
        }

        public static IEnumerable<T> GetChildrenByType<T>(this Node node)
            where T : class
        {
            return node.GetChildren()
                .Where(f => f is T)
                .Cast<T>();
        }
    }
}
