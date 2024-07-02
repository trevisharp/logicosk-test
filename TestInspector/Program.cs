using System.Drawing;

using Pamella;

App.Open(new MainView());

class MainView : View
{
    protected override void OnRender(IGraphics g)
    {
        g.Clear(Color.White);
    }
}