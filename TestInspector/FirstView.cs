using System;
using System.Drawing;
using System.Threading;

using Pamella;
using Logicosk;

class FirstView(string path) : View
{
    const string seed = "etsps2024401";
    protected override void OnStart(IGraphics g)
    {
        new Thread(async () => {
            try
            {
                Results.Current = new() {
                    Test = await TestManager.Open(path, seed)
                };
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message + "\n" + ex.StackTrace);
            }
        }).Start();

        Action<Input> ev = null;
        ev = key => {
            if (Results.Current.Test is null)
                return;
            
            if (key == Input.Space)
            {
                App.Clear();
                App.Push(new QuestionsView(ev));
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
            Use setas para navegar na prova e espaço para interagir.
            Use Q, W, A e S se estiver com dificuldade de ler alguma parte do texto.
            Use F para colocar a prova em modo 'aguradando...'.
            Ao aguardar você pode esperar o tempo acabar e descansar, você
            também pode começar a próxima etapa da prova antecipadamente.
            Outras funcionalidades serão indicadas na prova.
            """
        );
    }
}