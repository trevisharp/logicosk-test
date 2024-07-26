using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

using Pamella;
using System.Windows.Forms;
using System.Text;

public class Calculator : Lab
{
    List<string> originalArgs = [];
    public override void LoadParams(List<string> args)
    {
        originalArgs = args;
        foreach (var arg in args)
        {
            var data = arg.Split(" ");
            if (data.Length == 0)
                return;
            
            if (data is [ "custom", "get" ])
            {
                customGet = true;
                get = null;
            }
        }
    }

    bool customGet = false;
    Func<dynamic, dynamic, dynamic, dynamic> get;
    public Calculator()
    {
        get = (chars, start, end) =>
        {
            int parCount = 0;
            int parStart = 0;
            int result = 0;
            int num = 0;
            bool sum = true;
            for (int i = start; i < end; i++)
            {
                char character = chars[i];
                if (character == '(')
                {
                    if (parCount == 0)
                        parStart = i;
                    parCount++;
                    continue;
                }

                if (character == ')')
                {
                    parCount--;
                    if (parCount == 0)
                    {
                        var value = get(chars, parStart + 1, i);
                        result += (sum ? 1 : -1) * value;
                    }
                    continue;
                }

                if (parCount > 0)
                    continue;

                if (character == '+')
                {
                    result += (sum ? 1 : -1) * num;
                    num = 0;

                    sum = true;
                    continue;
                }

                if (character == '-')
                {
                    result += (sum ? 1 : -1) * num;
                    num = 0;

                    sum = false;
                    continue;
                }

                if (character < '0' || character > '9')
                    continue;

                num = 10 * num + character - '0';
            }

            result += (sum ? 1 : -1) * num;
            return result;
        };
    }

    public override void Interact(float dt) { }

    string[] eq = ["1+1", "1-1", "1+(2+3)", "1-(1-(3+4))"];
    string[] results = ["?", "?", "?", "?"];
    public override void Draw(IGraphics g)
    {
        g.Clear(Color.Black);

        StringBuilder sb = new();
        var pairs = eq.Zip(results);
        foreach ((var eq, var res) in pairs)
            sb.AppendLine($"{eq} = {res}");
        
        g.DrawText(
            new RectangleF(0, 0, g.Width, g.Height),
            new Font("Arial", 120f),
            StringAlignment.Center,
            StringAlignment.Center,
            Brushes.White, sb.ToString()
        );
    }

    public override void LoadBehaviour(Type code)
    {
        var method = code.GetMethod("get");
        if (customGet && method is not null)
            get = (chars, start, end) =>
            {
                try
                {
                    return method.Invoke(null, [chars, start, end]);
                }
                catch { return int.MinValue; }
            };
        
        results = eq.Select(e => (string)get(e, 0, e.Length).ToString()).ToArray();
    }

    public override void Reset()
    {
        LoadParams(originalArgs);
    }

    public override float Avaliate()
    {
        float points = 0;
        assert(get("1+1".ToList(), 0, 3), 2, ref points);
        assert(get("2+30".ToList(), 0, 4), 32, ref points);
        assert(get("1-1".ToList(), 0, 3), 0, ref points);
        assert(get("-1+2".ToList(), 0, 4), 1, ref points);
        assert(get("1+(2+3)".ToList(), 0, 7), 6, ref points);
        assert(get("2+(7-5)".ToList(), 0, 7), 4, ref points);
        assert(get("12-(3+4)".ToList(), 0, 8), 5, ref points);
        assert(get("1-(1-(1+1))".ToList(), 0, 11), 2, ref points);

        return points / 8;
    }

    void assert(float expected, float result, ref float points)
        => points += gauss(expected - result);

    float gauss(float x)
        => MathF.Exp(-x * x / 3);
}