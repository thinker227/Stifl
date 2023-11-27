using System.Xml;
using System.Xml.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Options;
using OneOf;
using Scriban;
using Scriban.Runtime;

namespace Stifl.CodeInator;

public static class App
{
    private const string ResourceLocation = "Stifl.CodeInator.AstTemplate.sbncs";
    
    public static int Run(FileInfo inputPath, FileInfo outputPath)
    {
        Console.WriteLine($"Input file: {inputPath.FullName}");
        var modelOrException = GetModel(inputPath);

        if (modelOrException.TryPickT1(out var exception, out var model))
        {
            Console.WriteLine(exception);
            return 1;
        }
        
        var templateStr = ManifestResourceHelper.ReadResource(ResourceLocation);
        var template = Template.Parse(templateStr);

        var text = Render(template, model);

        File.WriteAllText(outputPath.FullName, text);
        Console.WriteLine($"Written to {outputPath.FullName}");
        
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

    private static string Render(Template template, Root model)
    {
        var context = new TemplateContext();
        
        var scriptObject = new ScriptObject();
        scriptObject.Import(typeof(Functions));
        scriptObject.Import("root", () => model);
        context.PushGlobal(scriptObject);
        
        var text = template.Render(context);
        
        var root = CSharpSyntaxTree.ParseText(text).GetRoot();
        return root
            .NormalizeWhitespace(indentation: "    ", eol: "\n")
            .ToString();
    }
}
