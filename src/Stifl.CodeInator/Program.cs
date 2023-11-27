using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using Stifl.CodeInator;

var cmd = new RootCommand();

var inputPathArg = new Argument<FileInfo>()
{
    Name = "input-path",
    Description = "Path to the input file",
};
inputPathArg.ExistingOnly();
cmd.AddArgument(inputPathArg);

var outputPathArg = new Argument<FileInfo>()
{
    Name = "output-path",
    Description = "Path to the output file",
};
outputPathArg.LegalFilePathsOnly();
cmd.AddArgument(outputPathArg);

cmd.SetHandler(ctx =>
{
    var inputPath = ctx.ParseResult.GetValueForArgument(inputPathArg);
    var outputPath = ctx.ParseResult.GetValueForArgument(outputPathArg);
    
    ctx.ExitCode = App.Run(inputPath, outputPath);
});

var builder = new CommandLineBuilder(cmd);
builder.UseDefaults();

builder.Build().Parse(args).Invoke();
