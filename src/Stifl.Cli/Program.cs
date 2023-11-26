using Stifl;
using Stifl.Interpret;

if (args is not [var path])
{
    Console.WriteLine("Specify a file.");
    return 1;
}

var source = File.ReadAllText(path);

var compilation = Compilation.Create(source);

var bindings = compilation.Root.Decls.OfType<Ast.Decl.Binding>();
var maxNameWidth = bindings.Select(x => x.Name.Length).Max();

foreach (var binding in bindings)
{
    var name = $"{binding.Name}:".PadRight(maxNameWidth + 1);
    var type = compilation.TypeOf(binding);
    Console.WriteLine($"{name} {type}");
}

var main = bindings.FirstOrDefault(b => b.Name == "main");
if (main is not null && compilation.TypeOf(main) is { ForallTypes.Count: 0 })
{
    var interpreter = new Interpreter(compilation);
    var interpretedValue = interpreter.Evaluate(main);
    var retValue = interpretedValue.Eval();

    Console.WriteLine($"\n{retValue}");
}

return 0;
