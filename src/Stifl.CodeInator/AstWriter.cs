using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Stifl.CodeInator.Helpers;

namespace Stifl.CodeInator;

public sealed partial class AstWriter
{
    private readonly Root root;
    private readonly IndentedTextWriter writer = new();

    private AstWriter(Root root) => this.root = root;

    public static string Write(Root root)
    {
        var writer = new AstWriter(root);
        writer.WriteFile();
        return writer.writer.ToString();
    }

    private void WriteFile()
    {
        writer.WriteLine("""
        #nullable enable
        #pragma warning disable CS0108

        using System.Diagnostics.CodeAnalysis;

        namespace Stifl;

        """, isMultiline: true);

        WriteNode(root, null, []);

        writer.WriteLine();

        writer.WriteLine($"""
        partial record { root.name } : INode<{ root.name }>
        """);

        using (writer.WriteBlock())
        {
            writer.WriteLine("""
            private static IEnumerable<T> EmptyIfNull<T>(T? value) =>
                value is not null
                    ? [value]
                    : [];

            public IEnumerable<Ast> Children() => this switch
            {
            """, isMultiline: true);

            writer.IncreaseIndent();
            WriteChildrenGetter(root, [], []);
            writer.WriteLine("_ => throw new UnreachableException()");
            writer.DecreaseIndent();

            writer.WriteLine("};");
        }

        writer.WriteLine();

        writer.WriteLine($"""
        /// <summary>
        /// Visits AST nodes.
        /// </summary>
        /// <typeparam name="T">The type which the visitor returns.</typeparam>
        public abstract class {root.name}Visitor<T>
            where T : notnull
        """, isMultiline: true);

        using (writer.WriteBlock())
        {
            writer.WriteLine($$"""
            /// <summary>
            /// The default value of a visit.
            /// </summary>
            protected abstract T Default { get; }

            /// <summary>
            /// Called before a node is visited.
            /// </summary>
            /// <param name="node">The node being visited.</param>
            protected virtual void BeforeVisit({{root.name}} node) {}

            /// <summary>
            /// Called after a node has been visited.
            /// </summary>
            /// <param name="node">The node being visited.</param>
            protected virtual void AfterVisit({{root.name}} node) {}

            /// <summary>
            /// Filters nodes which should be visited.
            /// </summary>
            /// <param name="node">The node to filter.</param>
            /// <returns>Whether the node and its children should be visited.</returns>
            protected virtual bool Filter({{root.name}} node) => true;

            /// <summary>
            /// Visits many nodes and returns a sequence of return values.
            /// </summary>
            /// <typeparam name="TNode">The type of the nodes to visit.</typeparam>
            /// <param name="nodes">The nodes to visit.</param>
            [return: NotNullIfNotNull(nameof(nodes))]
            public IReadOnlyList<T>? VisitMany<TNode>(IEnumerable<TNode>? nodes) where TNode : {{root.name}} =>
                nodes?.Select(VisitNode).ToList()!;

            /// <summary>
            /// Visits a node.
            /// </summary>
            /// <param name="node">The node to visit.</param>
            /// <returns>The return value of the visit.</returns>
            [return: NotNullIfNotNull(nameof(node))]
            public T? VisitNode({{root.name}}? node)
            {
                if (node is null) return default;

                if (!Filter(node)) return Default;

                BeforeVisit(node);
                var x = Visit{{root.name}}(node);
                AfterVisit(node);

                return x;
            }

            """, isMultiline: true);

            WriteVisitorMethods(root, WriteVisitorMethod);
        }

        writer.WriteLine();

        writer.WriteLine($"""
        //// <summary>
        /// Visits AST nodes.
        /// </summary>
        public abstract class {root.name}Visitor
        """, isMultiline: true);

        using (writer.WriteBlock())
        {
            writer.WriteLine($$"""
            /// <summary>
            /// Called before a node is visited.
            /// </summary>
            /// <param name="node">The node being visited.</param>
            protected virtual void BeforeVisit({{root.name}} node) {}

            /// <summary>
            /// Called after a node has been visited.
            /// </summary>
            /// <param name="node">The node being visited.</param>
            protected virtual void AfterVisit({{root.name}} node) {}

            /// <summary>
            /// Filters nodes which should be visited.
            /// </summary>
            /// <param name="node">The node to filter.</param>
            /// <returns>Whether the node and its children should be visited.</returns>
            protected virtual bool Filter({{root.name}} node) => true;

            /// <summary>
            /// Visits many nodes and returns a sequence of return values.
            /// </summary>
            /// <typeparam name="TNode">The type of the nodes to visit.</typeparam>
            /// <param name="nodes">The nodes to visit.</param>
            public void VisitMany<TNode>(IEnumerable<TNode>? nodes) where TNode : {{root.name}}
            {
                if (nodes is null) return;

                foreach (var x in nodes) VisitNode(x);
            }

            /// <summary>
            /// Visits a node.
            /// </summary>
            /// <param name="node">The node to visit.</param>
            public void VisitNode({{root.name}}? node)
            {
                if (node is null) return;

                if (!Filter(node)) return;

                BeforeVisit(node);
                Visit{{root.name}}(node);
                AfterVisit(node);
            }
            
            """, isMultiline: true);

            WriteVisitorMethods(root, WriteVoidVisitorMethod);
        }

        writer.WriteLine();
    }

    private void WriteNode(Node node, Node? parent, ImmutableArray<Member> parentMembers)
    {
        var members = parentMembers.AddRange(node.members);

        if (node.docs is not null)
        {
            writer.WriteLine($"""
            /// <summary>
            /// {NormalizeWhitespace(node.docs)}
            /// </summary>
            """, isMultiline: true);
        }

        foreach (var docMember in members.Where(x => x.docs is not null))
        {
            writer.WriteLine($"""
            /// <param name="{docMember.name}">{NormalizeWhitespace(docMember.docs!)}</param>
            """);
        }

        var modifier = node.IsVariant ? "abstract" : "sealed";
        writer.Write($"""
        public {modifier} partial record {node.name}
        """);

        if (members.Length > 0)
        {
            writer.Write("(");
            foreach (var (member, hasNext) in members.HasNext())
            {
                WriteMember(member);
                if (hasNext) writer.Write(", ");
            }
            writer.Write(")");
        }

        if (parent is not null)
        {
            writer.Write(" : ");
            writer.Write(parent.name);

            if (parentMembers.Length > 0)
            {
                writer.Write("(");
                foreach (var (member, hasNext) in parentMembers.HasNext())
                {
                    writer.Write(member.name);
                    if (hasNext) writer.Write(", ");
                }
                writer.Write(")");
            }
        }

        if (node is Variant variant)
        {
            writer.WriteLine();

            using (writer.WriteBlock())
            {
                foreach (var (child, hasNext) in variant.nodes.HasNext())
                {
                    WriteNode(child, node, members);
                    if (hasNext) writer.WriteLine();
                }
            }
        }
        else
        {
            writer.WriteLine(";");
        }
    }

    private void WriteMember(Member member)
    {
        writer.Write(member.IsList
            ? $"ImmutableArray<{member.type}>"
            : member.type);

        if (member.IsOptional) writer.Write("?");

        writer.Write(" ");

        writer.Write(member.name);
    }

    private void WriteChildrenGetter(Node node, ImmutableArray<Node> parents, ImmutableArray<Member> parentMembers)
    {
        var members = parentMembers.AddRange(node.members);

        if (node is not Variant variant)
        {
            writer.Write(TypeName(node, parents));
            writer.Write(" x => [");

            foreach (var (member, hasNext) in members.HasNext())
            {
                if (member.IsPrimitive) continue;

                switch (member.IsList, member.IsOptional)
                {
                // Non-nullable list
                case (true, false):
                    writer.Write("..x.");
                    writer.Write(member.name);
                    break;

                // Nullable list
                case (true, true):
                    writer.Write("..(x.");
                    writer.Write(member.name);
                    writer.Write(" ?? [])");
                    break;

                // Non-nullable value
                case (false, false):
                    writer.Write("x.");
                    writer.Write(member.name);
                    break;

                // Nullable value
                case (false, true):
                    writer.Write("..EmptyIfNull(x.");
                    writer.Write(member.name);
                    writer.Write(")");
                    break;
                }

                if (hasNext) writer.Write(", ");
            }

            writer.WriteLine("],");
        }
        else
        {
            foreach (var child in variant.nodes)
                WriteChildrenGetter(child, parents.Add(node), parentMembers.AddRange(node.members));
        }
    }

    private void WriteVisitorMethods(Node root, Action<Node, ImmutableArray<Node>> writeVisitorMethod)
    {
        foreach (var ((node, parents), hasNext) in Hierarchy(root).HasNext())
        {
            writeVisitorMethod(node, parents);

            if (hasNext)
            {
                writer.WriteLine();
                writer.WriteLine();
            }
        }
    }

    private void WriteVisitorMethod(Node node, ImmutableArray<Node> parents)
    {
        var typeName = TypeName(node, parents);
        var methodName = MethodName(node, parents);
        var newParents = parents.Add(node);

        writer.WriteLine($"""
        /// <summary>
        /// Visits a node of type <see cref="{typeName}"/>.
        /// </summary>
        /// <param name="node">The node which is being visited.</param>
        /// <returns>The return value of the visit.</returns>
        public virtual T {methodName}({typeName} node)
        """, isMultiline: true);

        var asVariant = node as Variant;

        using (writer.WriteBlock())
        {
            WriteMemberVisits(node.members);
            if (node.members.Count > 0) writer.WriteLine();

            if (asVariant is not null)
            {
                writer.WriteLine("switch (node)");
                
                using (writer.WriteBlock())
                {
                    foreach (var child in asVariant.nodes)
                    {
                        writer.Write("case ");
                        writer.Write(TypeName(child, newParents));
                        writer.Write(" x: return ");
                        writer.Write(MethodName(child, newParents));
                        writer.WriteLine("(x);");
                    }

                    writer.WriteLine("default: throw new UnreachableException();");
                }
            }
            else writer.WriteLine("return Default;");
        }
    }

    private void WriteVoidVisitorMethod(Node node, ImmutableArray<Node> parents)
    {
        var typeName = TypeName(node, parents);
        var methodName = MethodName(node, parents);
        var newParents = parents.Add(node);

        writer.WriteLine($"""
        /// <summary>
        /// Visits a node of type <see cref="{typeName}"/>.
        /// </summary>
        /// <param name="node">The node which is being visited.</param>
        public virtual void {methodName}({typeName} node)
        """, isMultiline: true);

        using (writer.WriteBlock())
        {
            WriteMemberVisits(node.members);

            if (node is not Variant variant) return;

            if (node.members.Count > 0)
                writer.WriteLine();

            writer.WriteLine("switch (node)");

            using (writer.WriteBlock())
            {
                foreach (var child in variant.nodes)
                {
                    writer.Write("case ");
                    writer.Write(TypeName(child, newParents));
                    writer.WriteLine(" x:");

                    writer.IncreaseIndent();
                    writer.Write(MethodName(child, newParents));
                    writer.WriteLine("(x);");
                    writer.WriteLine("break;");
                    writer.DecreaseIndent();

                    writer.WriteLine();
                }

                writer.WriteLine("default: throw new UnreachableException();");
            }
        }
    }

    private void WriteMemberVisits(IEnumerable<Member> members)
    {
        foreach (var member in members)
        {
            if (member.IsPrimitive) continue;

            writer.Write(member.IsList ? "VisitMany" : "VisitNode");
            writer.Write("(node.");
            writer.Write(member.name);
            writer.WriteLine(");");
        }
    }

    private static IEnumerable<(Node node, ImmutableArray<Node> parents)> Hierarchy(Node root)
    {
        return Visit(root, []);

        static IEnumerable<(Node, ImmutableArray<Node>)> Visit(Node node, ImmutableArray<Node> parents)
        {
            if (node is not Variant variant) return [(node, parents)];

            var newParents = parents.Add(node);

            return variant.nodes
                .SelectMany(n => Visit(n, newParents))
                .Prepend((node, parents));
        }
    }

    private static string TypeName(Node node, IEnumerable<Node> parents) =>
        string.Join('.', parents.Append(node).Select(x => x.name));

    private static string MethodName(Node node, IEnumerable<Node> parents)
    {
        var nodes = parents
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

    private static string NormalizeWhitespace(string docs) =>
        NormalizeWhitespaceRegex().Replace(docs.Trim(), " ");

    [GeneratedRegex("\\s+")]
    private static partial Regex NormalizeWhitespaceRegex();
}
