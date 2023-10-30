namespace Stifl.Graphs;

/// <summary>
/// A node in a graph.
/// </summary>
/// <typeparam name="T">The type of the value associated with the node.</typeparam>
/// <param name="value">The value associated with the node.</param>
/// <param name="edges">The connected edges of the node.</param>
internal sealed class Node<T>(T value, IReadOnlyCollection<Node<T>> edges)
{
    /// <summary>
    /// The value associated with the node.
    /// </summary>
    public T Value { get; } = value;

    /// <summary>
    /// The connected edges of the node.
    /// </summary>
    public IReadOnlyCollection<Node<T>> Edges { get; } = edges;
}

/// <summary>
/// Utilities for creating graphs.
/// </summary>
internal static class Graph
{
    /// <summary>
    /// Creates a graph of nodes from a collection of values.
    /// </summary>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="values">The values to create the graph from.</param>
    /// <param name="getEdges">A function to get the edges of a value.</param>
    /// <returns>A collection of nodes in the graph.</returns>
    public static IReadOnlyCollection<Node<T>> Create<T>(
        IEnumerable<T> values,
        Func<T, IReadOnlyCollection<T>> getEdges)
        where T : notnull
    {
        var nodes = new Dictionary<T, Node<T>>();

        foreach (var value in values) Visit(value);

        Node<T> Visit(T value)
        {
            if (nodes!.TryGetValue(value, out var node)) return node;
            
            var connected = new List<Node<T>>();
            node = new Node<T>(value, connected);
            nodes[value] = node;

            var edges = getEdges(value);
            foreach (var edge in edges) connected.Add(Visit(edge));

            return node;
        }

        return nodes.Values;
    }

    /// <summary>
    /// Generates a path through an acyclic graph.
    /// </summary>
    /// <remarks>
    /// Does not perform any checks whether the graph is actually acyclic.
    /// </remarks>
    /// <typeparam name="T">The type of the values associated with the nodes.</typeparam>
    /// <param name="nodes">The nodes to walk.</param>
    /// <returns>A path of nodes visited in reverse.</returns>
    public static IEnumerable<Node<T>> GenerateAcyclicPath<T>(IEnumerable<Node<T>> nodes)
    {
        // https://en.wikipedia.org/wiki/Directed_acyclic_graph

        // This algorithm will walk through the nodes backwards.
        // As long as the graph is acyclic, this will find a path through every node.

        var visited = new HashSet<Node<T>>();

        return nodes.SelectMany(Visit);
        
        IEnumerable<Node<T>> Visit(Node<T> node)
        {
            if (visited.Contains(node)) return [];
            visited.Add(node);

            return node.Edges.SelectMany(Visit).Prepend(node);
        }
    }
}
