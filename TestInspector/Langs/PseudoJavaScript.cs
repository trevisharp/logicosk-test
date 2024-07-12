using System.Text;
using System.Linq;
using System.Collections.Generic;

public class PseudoJavaScript : PseudoLanguage
{
    public override Dictionary<string, string> Tutorial()
        => new() {
            { "Linguagem Não-Esotérica", "Como linguagem real, não existe um tutorial para essa prova." }
        };

    protected override string convert(string source, StringBuilder sb)
    {
        var code = new StringBuilder();
        
        code.AppendLine("using System;");
        code.AppendLine("using System.Collections.Generic;");
        code.AppendLine("TestePratico.main();");
        code.AppendLine("public static class TestePratico {");
        code.AppendLine(
            """
            public static void main(params string[] args)
            {
                
            }
            """
        );

        var lines = source.Split("\n")
            .Select(s => s.Trim());
        foreach (var line in lines)
        {
            if (line.StartsWith("func"))
            {
                var data = 
                    line.Split(' ', '(', ')', ',', '{')
                    .Where(s => !string.IsNullOrEmpty(s))
                    .Skip(1)
                    .ToArray();
                var name = data[0];
                var parameters = string.Join(", ", data[1..].Select(s => "dynamic " + s));
                code.AppendLine($"public static dynamic {name}({parameters}) {{");
                continue;
            }

            if (line == "}")
            {
                code.AppendLine("}");
                continue;
            }

            code.Append(
                line
                    .Replace("sqrt", "Sqrt")
                    .Replace("Math", "MathF")
                    .Replace("let", "var")
            );
            code.AppendLine(";");
        }

        code.AppendLine("}");

        return code.ToString();
    }
}