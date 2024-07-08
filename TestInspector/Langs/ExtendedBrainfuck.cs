using System.Collections.Generic;
using System.Linq;
using System.Text;

public class ExtendedBrainfuck : PseudoLanguage
{
    protected override string convert(string source, StringBuilder sb)
    {
        StringBuilder code = new StringBuilder();
        code.AppendLine("using System.Collections.Generic;");
        code.AppendLine("public static class defaultType {");

        var lines = source.Split("\n");
        string currentFunc = null;
        Dictionary<string, int> pointerDict = null;
        int count = 0;
        foreach (var line in lines.Select(x => x.Trim()))
        {
            count++;
            if (string.IsNullOrEmpty(line))
                continue;
            
            if (line.EndsWith(":")) {
                if (currentFunc is not null)
                    code.AppendLine("}");

                pointerDict = new Dictionary<string, int>();
                currentFunc = line.Replace(":", "");
                code.AppendLine($"public static int {currentFunc}(params int[] inputs) {{");
                code.AppendLine("List<int> memory = [ 0 ];");
                code.AppendLine("int pointer = 0;");
                code.AppendLine("int inputIndex = 0;");
                continue;
            }

            if (line == "exit") {
                code.AppendLine("return memory[0];");
                continue;
            }

            if (line.StartsWith("goto")) {
                var variable = line.Split(" ").LastOrDefault();
                if (!pointerDict.ContainsKey(variable))
                {
                    sb.AppendLine($"unknow variable {variable} at line {count}.");
                    continue;
                }
                int pointer = pointerDict[variable];
                code.AppendLine($"pointer = {pointer};");
            }

            if (line.StartsWith(".")) {
                var variable = line.Split(" ").LastOrDefault();
                if (!pointerDict.ContainsKey(variable))
                {
                    sb.AppendLine($"unknow variable {variable} at line {count}.");
                    continue;
                }
                
                int pointer = pointerDict[variable];
                code.AppendLine($"memory[pointer] = memory{pointer};");
            }
        }

        if (currentFunc is not null)
            code.AppendLine("}");
        code.AppendLine("}");
        return code.ToString();
    }
}