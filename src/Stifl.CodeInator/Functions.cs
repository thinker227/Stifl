using JetBrains.Annotations;
using Scriban.Runtime;

namespace Stifl.CodeInator;

public static class Functions
{
    [UsedImplicitly]
    public static string Join(string separator, ScriptArray values) =>
        string.Join(separator, values);

    [UsedImplicitly]
    public static string JoinReverse(string separator, ScriptArray values) =>
        string.Join(separator, values.Reverse());
}
