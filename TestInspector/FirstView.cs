using System;
using System.Drawing;
using System.Threading;

using Pamella;
using Logicosk;

class FirstView(string path) : View
{
    const string seed = "etsps2024401";
    Test test = null;
    protected override void OnStart(IGraphics g)
    {
        new Thread(async () => {
            try
            {
                test = await TestManager.Open(path, seed);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }).Start();

        Action<Input> ev = null;
        ev = key => {
            if (test is null)
                return;
            
            if (key == Input.Space)
            {
                App.Clear();
                App.Push(new QuestionsView(test, ev));
            }
        };
        g.SubscribeKeyDownEvent(ev);
    }

    protected override void OnRender(IGraphics g)
    {
        g.DrawText(
            new Rectangle(5, 5, g.Width - 10, g.Height - 10),
            new Font("Arial", 140), 
            StringAlignment.Center, StringAlignment.Center,
            "Aperte espaço para começar..."
        );
    }
}