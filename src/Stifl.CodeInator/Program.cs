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

var outputFileArg = new Argument<FileInfo>()
{
    Name = "output-file",
    Description = "Path to the output file",
};
outputFileArg.LegalFilePathsOnly();
cmd.AddArgument(outputFileArg);

cmd.SetHandler(ctx =>
{
    var inputFile = ctx.ParseResult.GetValueForArgument(inputFileArg);
    var outputFile = ctx.ParseResult.GetValueForArgument(outputFileArg);
    
    ctx.ExitCode = App.Run(inputFile, outputFile);
});

var builder = new CommandLineBuilder(cmd);
builder.UseDefaults();

builder.Build().Parse(args).Invoke();
