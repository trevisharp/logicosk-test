using System;
using System.Drawing;
using System.Linq;
using Logicosk;
using Pamella;

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
        lab = Lab.New(bugFix.Lab);
        lab.LoadParams(bugFix.LabParams);

        AlwaysInvalidateMode();

        if (oldEvent is not null)
            g.UnsubscribeKeyDownEvent(oldEvent);
        g.SubscribeKeyDownEvent(key =>
        {
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