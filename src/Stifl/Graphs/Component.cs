namespace Stifl.Graphs;

// https://en.wikipedia.org/wiki/Strongly_connected_component

/// <summary>
/// A strongly connected component in a graph.
/// </summary>
/// <typeparam name="T">The type of the associated values in the nodes in the graph.</typeparam>
/// <param name="nodes">The nodes in the component.</param>
internal sealed class Component<T>(IReadOnlySet<Node<T>> nodes)
{
    /// <summary>
    /// The nodes in the component.
    /// </summary>
    public IReadOnlySet<Node<T>> Nodes { get; } = nodes;
}

/// <summary>
/// Utilities for creating strongly connected components.
/// </summary>
internal static class Component
{
    private struct Data
    {
        public int? index;
        public int lowlink;
        public bool onStack;
    }

    /// <summary>
    /// Creates a set of strongly connected components from a set of nodes.
    /// </summary>
    /// <typeparam name="T">The type of the associated values in the nodes.</typeparam>
    /// <param name="nodes">The nodes to create the components from.</param>
    public static IReadOnlyCollection<Component<T>> Create<T>(IEnumerable<Node<T>> nodes)
    {
        // https://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm#The_algorithm_in_pseudocode

        // TODO: Clean up this method.

        var components = new List<Component<T>>();

        var data = new Dictionary<Node<T>, Data>();

        Data Data(Node<T> node) =>
            data!.GetOrAdd(node, () => new());
        
        var index = 0;
        var s = new Stack<Node<T>>();

        foreach (var v in nodes)
        {
            if (Data(v).index is null)
                StrongConnect(v);
        }

        void StrongConnect(Node<T> v)
        {
            var vd = Data(v);

            vd.index = index;
            vd.lowlink = index;
            vd.onStack = true;
            index++;
            data[v] = vd;

            s.Push(v);

            foreach (var w in v.Edges)
            {
                var v1 = v;

                var v1d = Data(v1);
                var wd = Data(w);

                if (wd.index is null)
                {
                    StrongConnect(w);

                    wd = Data(w);
                    v1d.lowlink = Math.Min(v1d.lowlink, wd.lowlink);
                }
                else if (wd.onStack)
                {
                    v1d.lowlink = Math.Min(v1d.lowlink, wd.index.Value);
                }
                
                data[v1] = v1d;
            }

            vd = Data(v);
            if (vd.lowlink == vd.index)
            {
                var componentNodes = new HashSet<Node<T>>();

                var w = null as Node<T>;
                do
                {
                    w = s.Pop();

                    var wd = Data(w);
                    wd.onStack = false;
                    data[w] = wd;

                    componentNodes.Add(w);
                } while(w != v);

                components.Add(new(componentNodes));
            }
        }

        return components;
    }

    /// <summary>
    /// Generates a directed acyclic graph of components.
    /// </summary>
    /// <typeparam name="T">The type of the associated values in the nodes of the components.</typeparam>
    /// <param name="components">The components to generate the graph from.</param>
    public static IReadOnlyCollection<Node<Component<T>>> GenerateAcyclicGraph<T>(
        IReadOnlyCollection<Component<T>> components)
    {
        // https://en.wikipedia.org/wiki/Edge_contraction

        // This algorithm will contract the nodes of each
        // strongly connected component into a single node per component.

        var nodes = components
            .SelectMany(c => c.Nodes
                .Select(n => (c, n)))
            .ToDictionary(
                x => x.n,
                x => x.c);

        return Graph.Create(components, c => c.Nodes
            .SelectMany(n => n.Edges)
            .Where(n => !c.Nodes.Contains(n))
            .Select(n => nodes[n])
            .Distinct()
            .ToList());
    }
}
