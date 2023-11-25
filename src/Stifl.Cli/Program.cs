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
var bindingTypes = bindings
    .Select(b => (b, compilation.TypeOf(b).ToString()));

foreach (var (binding, str) in bindingTypes)
{
    var name = $"{binding.Name}:".PadRight(maxNameWidth + 1);
    Console.WriteLine($"{name} {str}");
}

return 0;
