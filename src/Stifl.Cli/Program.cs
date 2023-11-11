using Pidgin;
using Stifl;
using Stifl.Parsing;

if (args is not [var path])
{
    Console.WriteLine("Specify a file.");
    return 1;
}

var source = File.ReadAllText(path);

var unit = Parse.Unit.Full().ParseOrThrow(source);
var (scopes, symbols) = unit.ResolveScopes();
var types = TypeResolve.ResolveTypes(unit, scopes, symbols);

var bindings = unit.Decls.OfType<Ast.Decl.Binding>();
var maxNameWidth = bindings.Select(x => x.Name.Length).Max();
var bindingTypes = bindings
    .Select(b => (b, types[b].ToString()));

foreach (var (binding, str) in bindingTypes)
{
    var name = $"{binding.Name}:".PadRight(maxNameWidth + 1);
    Console.WriteLine($"{name} {str}");
}

return 0;
