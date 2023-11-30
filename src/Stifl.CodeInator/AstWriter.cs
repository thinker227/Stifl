using Stifl.CodeInator.Helpers;

namespace Stifl.CodeInator;

public sealed class AstWriter
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
        
    }
}
