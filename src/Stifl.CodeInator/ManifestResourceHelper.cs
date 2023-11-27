using System.Reflection;

namespace Stifl.CodeInator;

public static class ManifestResourceHelper
{
    public static string ReadResource(string resourceLocation)
    {
        var assembly = Assembly.GetCallingAssembly();

        using var resourceStream = assembly.GetManifestResourceStream(resourceLocation);
        if (resourceStream is null)
        {
            throw new IOException($"Could not read manifest resource '{resourceLocation}' in assembly '{assembly.FullName}'.");
        }

        var reader = new StreamReader(resourceStream);
        var content = reader.ReadToEnd();

        return content;
    }
}
