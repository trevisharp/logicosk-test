using System;
using System.Drawing;
using System.Collections.Generic;

using Pamella;
using Logicosk;
using System.IO;
using System.Text;

class PraticalView(
    Test test, 
    Dictionary<Question, Alternative> awnsers,
    Action<Input> oldDown, 
    Action<Input> oldUp) : View
{
    protected override void OnStart(IGraphics g)
    {
        g.UnsubscribeKeyDownEvent(oldDown);
        g.UnsubscribeKeyUpEvent(oldUp);

        AlwaysInvalidateMode();

        if (!File.Exists("main.ebf"))
            File.Create("main.ebf").Close();

        g.SubscribeKeyDownEvent(key => {
            if (key == Input.A)
            {
                var sb = new StringBuilder();
                var compiler = new ExtendedBrainfuck();
                var f = compiler.Compile<int[], int>(File.ReadAllText("main.ebf"), sb);
                System.Windows.Forms.MessageBox.Show(sb.ToString());
                if (f is null)
                    return;
                System.Windows.Forms.MessageBox.Show(f([5]).ToString());
            }
        });
    }

    protected override void OnRender(IGraphics g)
    {
        g.Clear(Color.GreenYellow);
    }
}