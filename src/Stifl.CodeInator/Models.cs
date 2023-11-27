using System.Xml.Serialization;
using JetBrains.Annotations;

namespace Stifl.CodeInator;

public class Node
{
    [XmlAttribute("Name")]
    public required string name;
    
    [XmlElement("Value", typeof(Value))]
    [XmlElement("List", typeof(List))]
    public List<Member> members = [];

    [UsedImplicitly]
    public bool IsVariant => this is Variant;
}

public class Variant : Node
{
    [XmlElement("Node", typeof(Node))]
    [XmlElement("Variant", typeof(Variant))]
    public List<Node> nodes = [];
}

[XmlRoot]
public sealed class Root : Variant;

public abstract class Member
{
    [XmlAttribute("Name")]
    public required string name;

    [XmlAttribute("Type")]
    public required string type;

    [XmlAttribute("Optional")]
    public string optional = "false";

    [XmlAttribute("Primitive")]
    public string primitive = "false";

    [UsedImplicitly]
    public bool IsOptional => optional == "true";

    [UsedImplicitly]
    public bool IsPrimitive => primitive == "true";
    
    [UsedImplicitly]
    public bool IsList => this is List;
}

public sealed class Value : Member;

public sealed class List : Member;
