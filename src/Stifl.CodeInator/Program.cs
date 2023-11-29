using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Stifl.CodeInator;

var cmd = new RootCommand();

var inputFileArg = new Argument<FileInfo>()
{
    Name = "input-file",
    Description = "Path to the input file",
};
inputFileArg.ExistingOnly();
cmd.AddArgument(inputFileArg);

var projectFileArg = new Argument<FileInfo>()
{
    Name = "project-file",
    Description = "Path to the project file",
};
projectFileArg.LegalFilePathsOnly();
cmd.AddArgument(projectFileArg);

var fileNameArg = new Argument<string>()
{
    Name = "file-name",
    Description = "The file name of the generated file"
};
fileNameArg.LegalFileNamesOnly();
cmd.AddArgument(fileNameArg);

cmd.SetHandler(async ctx =>
{
    var inputFile = ctx.ParseResult.GetValueForArgument(inputFileArg);
    var projectPath = ctx.ParseResult.GetValueForArgument(projectFileArg);
    var fileName = ctx.ParseResult.GetValueForArgument(fileNameArg);
    
    ctx.ExitCode = await App.Run(inputFile, projectPath, fileName);
});

var builder = new CommandLineBuilder(cmd);
builder.UseDefaults();

builder.Build().Parse(args).Invoke();
