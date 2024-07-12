using System;
using System.IO;
using System.Linq;
using System.Drawing;

using Pamella;
using Logicosk;
using System.Text;
using System.Collections.Generic;

public class DebugTest(
    Results results,
    Action<Input> oldEvent
    ) : View
{
    Test test = results.Test;
    Lab lab;
    float[] grades = [ 0f, 0f, 0f, 0f, 0f, 0f ];
    int level = 0;

    protected override void OnStart(IGraphics g)
    {
        var bugFix = test.BugfixTests.FirstOrDefault();
        var lang = Language.New(bugFix.Language);
        lab = Lab.New(bugFix.Lab);
        lab.LoadParams(bugFix.LabParams);
        var file = $"main.{bugFix.Language}";
        var code = string.Join('\n', bugFix.BaseCode);
        File.WriteAllText(file, code);

        AlwaysInvalidateMode();

        if (oldEvent is not null)
            g.UnsubscribeKeyDownEvent(oldEvent);
        g.SubscribeKeyDownEvent(key =>
        {
            switch (key)
            {
                case Input.Space:
                    var sb = new StringBuilder();
                    var assembly = lang.CompileAssembly(file, sb);
                    if (assembly is null)
                    {
                        System.Windows.Forms.MessageBox.Show(sb.ToString());
                        return;
                    }
                    var type = assembly.GetType("TestePratico");
                    lab.Reset();
                    lab.LoadBehaviour(type);
                    var grade = lab.Avaliate();
                    if (grade > grades[level])
                        grades[level] = grade;
                    break;
            }

            if (key != Input.Escape)
                return;

            App.Pop();
            App.Push(new ResultView(results));
        });
    }

    DateTime last = DateTime.Now;
    protected override void OnRender(IGraphics g)
    {
        g.Clear(Color.FromArgb(40, 10, 10));

        var now = DateTime.Now;
        var time = now - last;
        last = now;
        var dt = (float)time.TotalSeconds;

        lab.Interact(dt);
        lab.Draw(g);

        g.DrawText(
            new RectangleF(0, 0, 200, 40), 
            new Font("Arial", 16),
            Brushes.White, 
            $"Melhor Nota: {100 * grades[level]}"
        );
    }
}