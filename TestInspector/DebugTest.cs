using System;
using System.Drawing;

using Pamella;

public class DebugTest(
    Results results,
    Action<Input> oldEvent
    ) : View
{
    protected override void OnStart(IGraphics g)
    {
        g.UnsubscribeKeyDownEvent(oldEvent);
        g.SubscribeKeyDownEvent(key =>
        {
            if (key != Input.Escape)
                return;
            
            App.Pop();
            App.Push(new ResultView(results));
        });
    }

    protected override void OnRender(IGraphics g)
    {
        g.Clear(Color.FromArgb(40, 10, 10));
    }
}