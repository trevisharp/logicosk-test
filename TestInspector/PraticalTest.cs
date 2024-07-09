using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Pamella;
using Logicosk;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing.Design;
using System.Dynamic;

class PraticalView(
    Test test, 
    Dictionary<Question, Alternative> answers,
    Action<Input> oldDown, 
    Action<Input> oldUp) : View
{
    Dictionary<PraticalTest, float> bestResult = new();
    Dictionary<PraticalTest, Dictionary<string, string>> docs = new();
    int current = 0;
    int page = 0;
    DateTime testFinal;
    int spacing = 20;
    protected override void OnStart(IGraphics g)
    {
        g.UnsubscribeKeyDownEvent(oldDown);
        g.UnsubscribeKeyUpEvent(oldUp);

        AlwaysInvalidateMode();

        foreach (var pratical in test.PraticalTests)
        {
            var file = $"main.{pratical.Language}";
            File.Create(file).Close();
            bestResult.Add(pratical, 0f);
            docs.Add(pratical, PseudoLanguage.New(pratical.Language).Tutorial());
        }

        testFinal = DateTime.Now.AddMinutes(test.MinutesDuration);

        g.SubscribeKeyDownEvent(key => {
            
            switch (key)
            {
                case Input.Up:
                    page--;
                    break;

                    
                case Input.Down:
                    page++;
                    break;

                    
                case Input.Left:
                    current--;
                    break;

                    
                case Input.Right:
                    current++;
                    break;
                

                case Input.W:
                    spacing--;
                    if (spacing < 2)
                        spacing = 2;
                    break;
                

                case Input.S:
                    spacing++;
                    if (spacing > 40)
                        spacing = 40;
                    break;
                
                case Input.Space:
                    var pratical = test.PraticalTests[current % test.PraticalTests.Count];
                    var compiler = PseudoLanguage.New(pratical.Language);
                    
                    var file = $"main.{pratical.Language}";
                    var code = File.ReadAllText(file);
                    
                    var sb = new StringBuilder();
                    var func = compiler.Compile<object[], object>(code, sb);
                    if (sb.Length > 0)
                    {
                        System.Windows.Forms.MessageBox.Show(sb.ToString());
                        return;
                    }
                    if (func is null)
                        return;
                    
                    float corrects = 0f;
                    var resutls = new StringBuilder();
                    foreach (var test in pratical?.Tests ?? [])
                    {
                        int result = Task.WaitAny(
                            Task.Run(async () => {
                                await Task.Delay(1000);
                            }),
                            Task.Run(() => {
                                try
                                {
                                    var output = func(
                                        test.Inputs
                                            .Select(x => x is JsonElement el && 
                                                el.TryGetInt32(out int value) ? value : x)
                                            .Select(x => x is JsonElement el && 
                                                el.TryGetSingle(out float value) ? value : x)
                                            .Select(x => x is JsonElement el ? el.GetString() : x)
                                            .ToArray()
                                        );
                                    
                                    if (output.ToString() != test.Output.ToString())
                                        return;       
                                    corrects++;
                                }
                                catch (Exception ex)
                                {
                                    System.Windows.Forms.MessageBox.Show(ex.Message);
                                }
                            })
                        );
                    }

                    float pontuation = corrects / pratical.Tests.Count;
                    if (bestResult[pratical] < pontuation)
                        bestResult[pratical] = pontuation;
                    break;
            }
        });
    }

    protected override void OnRender(IGraphics g)
    {
        g.Clear(Color.DarkGreen);
        if (test is null)
            return;
        
        int index = current % test.PraticalTests.Count;
        var pratical = test.PraticalTests[index];
        int pageIndex = page % (1 + docs[pratical].Keys.Count);
        if (pageIndex == 0)
        {
            var font = new Font("Arial", 40);
            g.DrawText(
                new Rectangle(5, 5, g.Width - 10, g.Height - 10),
                font, StringAlignment.Near, StringAlignment.Near,
                Brushes.LightCoral,
                $"""
                Questão Prática {index + 1} / {test.PraticalTests.Count}: {pratical.Text}
                Melhor Nota Obtida: {100 * bestResult[pratical]}%
                
                Exemplos: {pratical.Example}
                Desempenho Crítico: {pratical.Performance}
                Linguagem: {pratical.Language}

                Use setar para cima e baixo para navegar entre as instruções e direita e
                esquerda para alterar a questão. Use espaço para testar o código que foi
                criado na área execução.
                """
            );
        }
        else
        {
            var font = new Font("Arial", 30);
            var tutorial = docs[pratical];

            g.DrawText(
                new Rectangle(5, 5, g.Width / 3 - 10, g.Height - 10),
                font, StringAlignment.Near, StringAlignment.Near,
                Brushes.LightCoral, "Especificação da Lingugem:\n\n" +
                    string.Join('\n', tutorial.Keys.Select((s, i) => 
                        i == pageIndex - 1 ? s.ToUpper() : s.ToLower()
                    )
                )
            );

            font = new Font("Arial", 20);
            var current = tutorial[tutorial.Keys.ToArray()[pageIndex - 1]].Split('@');
            bool code = false;
            int docLines = 0;
            foreach (var part in current)
            {
                int lines = part.Count(c => c == '\n') + 1;
                int start = 5 + spacing * docLines;
                int end = 5 + spacing * (docLines + lines);
                var rect = new Rectangle(
                    g.Width / 3 - 5, start, 
                    2 * g.Width / 3 - 10, end
                );
                g.DrawText(rect, font, 
                    StringAlignment.Near, StringAlignment.Near,
                    code ? Brushes.LightYellow : Brushes.LightCoral, part
                );
                docLines += lines;
                code = !code;
            }
        }

        timeCheck();

        void timeCheck()
        {
            var timeFont = new Font("Arial", 16);
            var remainingTime = testFinal - DateTime.Now;
            g.DrawText(
                new RectangleF(g.Width - 120, g.Height - 30, 120, 30),
                timeFont, remainingTime.TotalMinutes switch
                {
                    <= 15 and > 5 => Brushes.Yellow,
                    <= 5 and > 1 => Brushes.Orange,
                    <= 1 => Brushes.Red,
                    _ => Brushes.Black
                },
                $"{remainingTime.Hours:00}:{remainingTime.Minutes:00}:{remainingTime.Seconds:00}"
            );

            if (DateTime.Now > testFinal)
            {
                App.Pop();
            }
        }

    }
}