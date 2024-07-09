using System.Collections.Generic;
using System.Drawing;
using Logicosk;
using Pamella;

public class DebugTest(
    Test test, 
    Dictionary<Question, Alternative> answers,
    Dictionary<PraticalTest, float> bestResults
    ) : View
{
    protected override void OnStart(IGraphics g)
    {
        
    }

    protected override void OnRender(IGraphics g)
    {
        g.Clear(Color.FromArgb(40, 10, 10));
    }
}