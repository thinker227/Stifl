using Pidgin;
using Stifl;

if (args is not [var path])
{
    Console.WriteLine("Specify a file.");
    return 1;
}

var source = File.ReadAllText(path);

var unit = Parse.Unit.ParseOrThrow(source);
var (scopes, symbols) = unit.ResolveScopes();
var types = TypeResolve.ResolveTypes(unit, scopes, symbols);

foreach (var decl in unit.Decls)
{
    switch (decl)
    {
    case Ast.Decl.Binding binding:
        var type = types[binding];
        Console.WriteLine($"{binding.Name}: {type}");
        break;
    }
}

return 0;
