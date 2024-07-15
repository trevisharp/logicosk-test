using System;
using System.Linq;
using System.Text.Json;
using System.Collections.Generic;

public static class InputAnalyzer
{
    public static string[] Analyze(List<object> objs)
    {
        List<string> outputs = [];

        foreach (var obj in objs)
        {
            if (obj is JsonElement el)
            {
                try
                {
                    if (el.TryGetInt32(out int integer))
                    {
                        outputs.Add(integer.ToString());
                        continue;
                    }
                }
                catch {}

                try
                {
                    if (el.TryGetSingle(out float single))
                    {
                        outputs.Add(single.ToString());
                        continue;
                    }
                }
                catch {}

                var code = el.GetString();
                outputs.AddRange(CompileExpression(code));
            }
        }

        return [.. outputs];
    }

    public static IEnumerable<string> CompileExpression(string code)
    {
        var parts = code.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .ToArray();
        
        List<string> data = [];

        Dictionary<string, float> variables = [];
        for (int i = 0; i < parts.Length; i++)
        {
            var item = parts[i];
            switch (item)
            {
                case "given":
                    i++; // given
                    var name = parts[i];
                    i++; // name
                    i++; // =
                    var value = get(parts[i]);
                    variables.Add(name, value);
                    break;

                case "for":
                    i++; // for
                    var forname = parts[i];
                    i++; // variable
                    i++; // to
                    var limit = get(parts[i]);
                    i++; // limit
                    i++; // add
                    
                    variables.Add(forname, 0);
                    for (int k = 0; k < limit; k++)
                    {
                        variables[forname] = k;
                        data.Add(get(parts[i]).ToString());
                    }
                    break;
            }
        }
        
        return data;

        float get(string str)
        {
            if (str is ['r', '(', .., ')'])
            {
                var parts = str
                    .Replace("r(", "")
                    .Replace(")", "")
                    .Split(',')
                    .Select(s => s.Trim())
                    .ToArray();
                if (parts.Length != 2)
                    throw new Exception($"Invalid random expression in {str}.");
                
                var min = get(parts[0]);
                var max = get(parts[1]);
                return Random.Shared.Next((int)min, (int)max);
            }

            if (str.Contains('+'))
            {
                return str
                    .Split('+')
                    .Select(s => s.Trim())
                    .Select(s => get(s))
                    .Aggregate(0f, (i, j) => i + j);
            }

            if (str.Contains('-'))
            {
                var coll = str
                    .Split('-')
                    .Select(s => s.Trim())
                    .Select(s => get(s));
                
                return coll.Skip(1)
                    .Aggregate(coll.First(), (i, j) => i - j);
            }

            if (str.Contains('*'))
            {
                return str
                    .Split('*')
                    .Select(s => s.Trim())
                    .Select(s => get(s))
                    .Aggregate(1f, (i, j) => i * j);
            }

            if (float.TryParse(str
                .Replace("k", "000")
                .Replace("m", "000000"),
                out float result))
                return result;
            
            if (variables.ContainsKey(str))
                return variables[str];
            
            throw new Exception($"Invalid variable {str}");
        }
    }
}