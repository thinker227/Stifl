namespace Stifl;

/// <summary>
/// A simple node structure which has child nodes.
/// Mainly exists to abstract shared logic for finding children, descendants, and parents.
/// </summary>
/// <remarks>For simplicity, the nodes should not be able to be cyclic in structure.</remarks>
/// <typeparam name="TSelf">The self type.</typeparam>
public interface INode<out TSelf> where TSelf : notnull, INode<TSelf>
{
    /// <summary>
    /// Gets the children of the node.
    /// </summary>
    /// <param name="node">The node to get the children of.</param>
    IEnumerable<TSelf> Children();
}

public static class NodeExtensions
{
    /// <summary>
    /// Gets the children of a node with the node prepended to the sequence.
    /// </summary>
    /// <param name="node">The node to get the children of.</param>
    public static IEnumerable<T> ChildrenAndSelf<T>(this T node)
        where T : INode<T> =>
        node.Children().Prepend(node);

    /// <summary>
    /// Gets the descendants of a node.
    /// </summary>
    /// <param name="node">The node to get the descendants of.</param>
    public static IEnumerable<T> Descendants<T>(this T node)
        where T : INode<T> =>
        node.Children().SelectMany(DescendantsAndSelf);

    /// <summary>
    /// Gets the descendants of a node with the node prepended to the sequence.
    /// </summary>
    /// <param name="node">The node to get the descendants of.</param>
    public static IEnumerable<T> DescendantsAndSelf<T>(this T node)
        where T : INode<T> =>
        node.Descendants().Prepend(node);

    /// <summary>
    /// Gets the descendants of a node.
    /// </summary>
    /// <param name="node">The node to get the descendants of.</param>
    /// <param name="filter">A function to filer nodes.
    /// If returns <see langword="false"/> for a node,
    /// the node will not be returned
    /// and none of its children will be visited.</param>
    public static IEnumerable<T> Descendants<T>(this T node, Func<T, bool> filter)
        where T : INode<T> =>
        node.Children().SelectMany(n => n.DescendantsAndSelf(filter));

    /// <summary>
    /// Gets the descendants of a node with the node prepended to the sequence.
    /// </summary>
    /// <param name="node">The node to get the descendants of.</param>
    /// <param name="filter">A function to filer nodes.
    /// If returns <see langword="false"/> for a node,
    /// the node will not be returned and none of its children will be visited.</param>
    public static IEnumerable<T> DescendantsAndSelf<T>(this T node, Func<T, bool> filter)
        where T : INode<T>
    {
        if (!filter(node)) return [];

        return node.Descendants(filter).Prepend(node);
    }

    /// <summary>
    /// Constructs a dictionary of the parents of all descendant nodes of a node. 
    /// </summary>
    /// <remarks>Note that the root node will not have a parent.</remarks>
    /// <param name="node">The node to get the parents of the descendant nodes of.</param>
    public static IReadOnlyDictionary<T, T> GetParents<T>(this T node)
        where T : INode<T>
    {
        var parents = new Dictionary<T, T>();
        Visit(node);
        return parents;

        void Visit(T node)
        {
            foreach (var child in node.Children())
            {
                parents[child] = node;
                Visit(child);
            }
        }
    }
}
