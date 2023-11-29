using System.Xml;
using System.Xml.Serialization;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using OneOf;
using Scriban;
using Scriban.Runtime;

namespace Stifl.CodeInator;

public static class App
{
    private const string ResourceLocation = "Stifl.CodeInator.AstTemplate.sbncs";
    
    public static async Task<int> Run(FileInfo inputFile, FileInfo projectFile, string fileName)
    {
        Console.WriteLine($"Reading input XML file {inputFile.FullName}");
        var modelOrException = GetModel(inputFile);

        if (modelOrException.TryPickT1(out var exception, out var model))
        {
            Console.WriteLine(exception);
            return 1;
        }
        
        var templateStr = ManifestResourceHelper.ReadResource(ResourceLocation);
        var template = Template.Parse(templateStr);

        Console.WriteLine("Rendering Scriban template");
        var text = Render(template, model);

        Console.WriteLine("Registering MSBuild locator");
        MSBuildLocator.RegisterDefaults();

        Console.WriteLine($"Opening project {projectFile.FullName}");
        var workspace = MSBuildWorkspace.Create();
        var project = await workspace.OpenProjectAsync(projectFile.FullName);
        var document = await CreateFormattedDocument(project, text, fileName);

        var success = workspace.TryApplyChanges(document.Project.Solution);

        if (!success)
        {
            Console.WriteLine($"Failed to apply changes to project");
            return 1;
        }

        Console.WriteLine($"Wrote to document {fileName}");
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
        
        return template.Render(context);
    }

    private static async Task<Document> CreateFormattedDocument(Project project, string text, string documentName)
    {
        Console.WriteLine("Parsing and formatting text");
        var tree = CSharpSyntaxTree.ParseText(text);
        var formattedRoot = tree.GetRoot().NormalizeWhitespace();

        Document document;
        if (project.Documents.FirstOrDefault(doc => doc.Name == documentName) is Document existing)
        {
            Console.WriteLine($"Found existing document {documentName}");
            document = existing.WithSyntaxRoot(formattedRoot);
        }
        else
        {
            Console.WriteLine($"Creating new document {documentName}");
            Console.WriteLine("*** Remember to clean up csproj! ***");
            document = project.AddDocument(documentName, formattedRoot);
        }

        var formatted = await Formatter.FormatAsync(document);

        return formatted;
    }
}
