using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public class ExtendedBrainfuck : PseudoLanguage
{
    protected override string convert(string source, StringBuilder sb)
    {
        var code = new StringBuilder();
        code.AppendLine("using System.Collections.Generic;");
        code.AppendLine("defaultType.main(1, 2, 3);");
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

                pointerDict = new Dictionary<string, int> {
                    { "result", 0 }
                };
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
                    pointerDict.Add(variable, pointerDict.Keys.Count);
                    code.AppendLine("memory.Add(0);");
                }
                int pointer = pointerDict[variable];
                code.AppendLine($"pointer = {pointer};");
                continue;
            }

            if (line.StartsWith(".")) {
                code.AppendLine($"memory[pointer] = inputs[inputIndex];");
                code.AppendLine($"inputIndex++;");
                continue;
            }

            if (line == "[")
            {
                code.AppendLine("while(memory[pointer] != 0) {");
                continue;
            }

            if (line == "![")
            {
                code.AppendLine("while(memory[pointer] == 0) {");
                continue;
            }

            if (line == "]")
            {
                code.AppendLine("}");
                continue;
            }

            if (line == "-")
            {
                code.AppendLine($"memory[pointer]--;");
                continue;
            }

            if (line == "+")
            {
                code.AppendLine($"memory[pointer]++;");
                continue;
            }

            if (line.StartsWith("call"))
            {
                var data = line.Split(" ");
                code.Append($"memory[pointer] = {data[1]}(");
                for (int i = 2; i < data.Length; i++)
                {
                    var variable = data[i];
                    if (!pointerDict.ContainsKey(variable))
                    {
                        sb.AppendLine($"unknow variable {variable} at line {count}.");
                        continue;
                    }
                    code.Append($"memory[{pointerDict[variable]}]");
                    if (i + 1 < data.Length)
                        code.Append(", ");
                }
                code.AppendLine($");");
                continue;
            }

            if (int.TryParse(line, out int num))
            {
                code.AppendLine($"memory[pointer] = {num};");
                continue;
            }

            if (!pointerDict.ContainsKey(line))
            {
                sb.AppendLine($"unknow variable {line} at line {count}.");
                continue;
            }
            code.AppendLine($"memory[pointer] = memory[{pointerDict[line]}];");
        }

        if (currentFunc is not null)
            code.AppendLine("}");
        code.AppendLine("}");
        return code.ToString();
    }
}