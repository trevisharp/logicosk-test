using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public static class Compiler
{
    public static Assembly Compile(string code, StringBuilder messages)
    {
        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.ConsoleApplication
        );

        var compilation = CSharpCompilation.Create(
            "HotReloadAppend",
            syntaxTrees: [CSharpSyntaxTree.ParseText(code)],
            references: getReferences(),
            options: compilationOptions
        );

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            foreach (var diagnostic in result.Diagnostics)
                messages.AppendLine(diagnostic.GetMessage());
            return null;
        }
        
        ms.Seek(0, SeekOrigin.Begin);
        var assembly = Assembly.Load(ms.ToArray());
        return assembly;
    }

    static IEnumerable<MetadataReference> getReferences()
    {
        var assembly = Assembly.GetEntryAssembly();
        var assemblies = assembly
            .GetReferencedAssemblies()
            .Select(Assembly.Load)
            .Append(assembly)
            .Append(Assembly.Load("System.Private.CoreLib"));
        
        return assemblies
            .Select(r => MetadataReference.CreateFromFile(r.Location));
    }
}