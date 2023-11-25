using Stifl;

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

return 0;
