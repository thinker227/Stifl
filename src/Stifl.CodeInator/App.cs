using System.Xml;
using System.Xml.Serialization;
using OneOf;

namespace Stifl.CodeInator;

public static class App
{    
    public static int Run(FileInfo inputFile, FileInfo outputFile)
    {
        Console.WriteLine($"Reading input XML file {inputFile.FullName}");
        var modelOrException = GetModel(inputFile);

        if (modelOrException.TryPickT1(out var exception, out var model))
        {
            Console.WriteLine(exception);
            return 1;
        }

        Console.WriteLine("Rendering text");
        var text = Render(model);

        Console.WriteLine($"Writing to {outputFile.FullName}");
        File.WriteAllText(text, outputFile.FullName);

        return 0;
    }

    private static OneOf<Root, XmlException> GetModel(FileInfo xmlFile)
    {
        try
        {
            using var stream = xmlFile.OpenText();
            var reader = XmlReader.Create(stream);
            var serializer = new XmlSerializer(typeof(Root));
            return (Root)serializer.Deserialize(reader)!;
        }
        catch (InvalidOperationException e) when (e.InnerException is XmlException xmlException)
        {
            return xmlException;
        }
    }

    private static string Render(Root model) => AstWriter.Write(model);
}
