using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

public class Java : Language
{
    public override string BaseCode => 
        """
        public class TestePratico
        {
            RETURNTYPE function(PARAMETERS)
            {
                // Implementation here
            }
        }
        """;

    public override Func<T, object> Compile<T>(string source, StringBuilder sb)
    {
        var assembly = CompileAssembly(source, sb);
        if (assembly is null)
            return null;
        
        var defaultType = assembly.GetType("TestePratico");
        var mainCode = defaultType.GetMethod("main");
        return x => mainCode.Invoke(null, [x]);
    }

    public override Assembly CompileAssembly(string source, StringBuilder sb)
    {
        var code = File.ReadAllText(source);
        code = convert(code, sb);
        if (sb.Length > 0)
        {
            sb.Insert(0, "Erros de Sintaxes encontrados:\n");
            return null;
        }

        var assembly = Compiler.Compile(code, sb);
        if (sb.Length > 0)
        {
            sb.Insert(0, "Erros Semânticos encontrados:\n");
            return null;
        }
        return assembly;
    }

    private string convert(string code, StringBuilder sb)
    {
        code = code
            .Replace("String", "string")
            .Replace("Integer", "int")
            .Replace("Integer", "int")
            .Replace("substring", "SubString")
            .Replace("length", "Length")
            ;
        
        code +=
            """
            public static void JavaExtensions
            {
                public static char charAt(this string str, int index)
                    => str[index];
            }
            """;
        
        var lines = code.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (line.Contains(':') && line.Contains("for"))
                lines[i] = line
                    .Replace(":", "in")
                    .Replace("for", "foreach");
        }

        code = string.Join('\n', lines);

        return code;
    }

    public override Dictionary<string, string> Tutorial()
        => new() {
            { "Linguagem Não-Esotérica", "Como linguagem real, não existe um tutorial para essa prova." }
        };
}