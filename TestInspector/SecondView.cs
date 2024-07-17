using System;
using System.Drawing;

using Pamella;

class SecondView(
    Action<Input> oldDown, 
    Action<Input> oldUp) : View
{
    protected override void OnStart(IGraphics g)
    {
        g.UnsubscribeKeyDownEvent(oldDown);
        g.UnsubscribeKeyUpEvent(oldUp);

        Action<Input> ev = null;
        ev = key => {
            if (Results.Current.Test is null)
                return;
            
            if (key == Input.Space)
            {
                App.Clear();
                App.Push(new PraticalView(ev));
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

        g.DrawText(
            new Rectangle(5, g.Height / 2 + 50, g.Width - 10, g.Height / 2 - 80),
            new Font("Arial", 20), 
            StringAlignment.Center, StringAlignment.Center,
            """
            Nesta fase, na sua pasta onde está rodando este programa, arquivos
            de código em diversas linguagens serão criados. Resolva os problemas
            e rode os testes unitários usando o espaço.
            """
        );
    }
}