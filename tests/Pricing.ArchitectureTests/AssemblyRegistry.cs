using System.Reflection;

namespace Pricing.ArchitectureTests;

internal static class AssemblyRegistry
{
    internal static readonly IReadOnlyList<Assembly> Pricing;

    static AssemblyRegistry()
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var loaded = new List<Assembly>();
        foreach (var file in Directory.GetFiles(baseDir, "Pricing.*.dll"))
        {
            if (Path.GetFileName(file).Contains("Tests"))
                continue;
            try { loaded.Add(Assembly.LoadFrom(file)); }
            catch (BadImageFormatException) { }
        }
        Pricing = loaded;
    }
}
