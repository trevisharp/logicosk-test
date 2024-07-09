using System.Drawing;

using Pamella;

public class DebugTest(
    Results results
    ) : View
{
    protected override void OnStart(IGraphics g)
    {
        App.Pop();
        App.Push(new ResultView(results));
    }

    protected override void OnRender(IGraphics g)
    {
        g.Clear(Color.FromArgb(40, 10, 10));
    }
}