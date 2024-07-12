using System;
using System.IO;
using System.Linq;
using System.Drawing;

using Pamella;
using Logicosk;

public class DebugTest(
    Results results,
    Action<Input> oldEvent
    ) : View
{
    Test test = results.Test;
    Lab lab;

    protected override void OnStart(IGraphics g)
    {
        var bugFix = test.BugfixTests.FirstOrDefault();
        var lang = Language.New(bugFix.Language);
        lab = Lab.New(bugFix.Lab, lang);
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
                    code = File.ReadAllText(file);
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
    }
}