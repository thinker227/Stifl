using JetBrains.Annotations;
using Scriban.Runtime;

namespace Stifl.CodeInator;

public static class Functions
{
    [UsedImplicitly]
    public static string TypeName(Node node, ScriptArray parents) =>
        string.Join('.', parents.OfType<Node>().Append(node).Select(x => x.name));

    [UsedImplicitly]
    public static string MethodName(Node node, ScriptArray parents)
    {
        var nodes = parents
            .OfType<Node>()
            .Append(node) // Make sure the node is included.
            .ToList();

        // Only skip root type name if there are more nodes than the root node.
        var names = (nodes.Count == 1
                ? nodes
                : nodes.Skip(1))
            .Reverse()
            .Select(x => x.name);

        return $"Visit{string.Concat(names)}";
    }
}
