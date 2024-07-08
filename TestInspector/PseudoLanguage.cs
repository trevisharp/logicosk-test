using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

public abstract class PseudoLanguage
{
    public static PseudoLanguage New(string lang)
        => lang switch
        {
            "ebf" => new ExtendedBrainfuck(),
            _ => throw new NotImplementedException("Not Implemented Language")
        };

    public abstract Dictionary<string, string> Tutorial();

    protected abstract string convert(string source, StringBuilder sb);

    public Func<T, R> Compile<T, R>(string source, StringBuilder sb)
    {
        var code = convert(source, sb);
        if (sb.Length > 0)
            return null;

        var assembly = compile(code, sb);
        if (sb.Length > 0)
            return null;
        if (assembly is null)
            return null;
        
        var defaultType = assembly.GetType("defaultType");
        var mainCode = defaultType.GetMethod("main");
        return x => (R)mainCode.Invoke(null, [x]);
    }

    Assembly compile(string code, StringBuilder messages)
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

    IEnumerable<MetadataReference> getReferences()
    {
        var assembly = Assembly.GetEntryAssembly();
        var assemblies = assembly
            .GetReferencedAssemblies()
            .Select(r => Assembly.Load(r))
            .Append(assembly)
            .Append(Assembly.Load("System.Private.CoreLib"));
        
        return assemblies
            .Select(r => MetadataReference.CreateFromFile(r.Location));
    }
}