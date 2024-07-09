using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Pamella;
using Logicosk;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;

class PraticalView(
    Test test, 
    Dictionary<Question, Alternative> answers,
    Action<Input> oldDown, 
    Action<Input> oldUp) : View
{
    Dictionary<PraticalTest, float> bestResult = new();
    Dictionary<PraticalTest, Dictionary<string, string>> docs = new();
    Dictionary<PraticalTest, string> lastResult = new();
    int current = 0;
    int page = 0;
    DateTime testFinal;
    int spacing = 22;
    protected override void OnStart(IGraphics g)
    {
        g.UnsubscribeKeyDownEvent(oldDown);
        g.UnsubscribeKeyUpEvent(oldUp);

        AlwaysInvalidateMode();

        foreach (var pratical in test.PraticalTests)
        {
            var file = $"main.{pratical.Language}";
            var lang = Language.New(pratical.Language);
            File.WriteAllText(file, lang.BaseCode);
            bestResult.Add(pratical, 0f);
            docs.Add(pratical, lang.Tutorial());
            lastResult.Add(pratical, "Nenhuma execução registrada...");
        }

        testFinal = DateTime.Now.AddMinutes(test.MinutesDuration);

        g.SubscribeKeyDownEvent(async key => {
            
            switch (key)
            {
                case Input.Up:
                    page--;
                    break;

                    
                case Input.Down:
                    page++;
                    break;

                    
                case Input.Left:
                    page = 0;
                    current--;
                    break;

                    
                case Input.Right:
                    page = 0;
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
                    var compiler = Language.New(pratical.Language);
                    
                    var file = $"main.{pratical.Language}";
                    
                    var runInfo = new StringBuilder();
                    var func = compiler.Compile<object[]>(file, runInfo);
                    if (runInfo.Length > 0)
                        lastResult[pratical] = runInfo.ToString();
                    if (func is null)
                        return;
                    
                    int corrects = 0;
                    await Task.Run(() =>
                        Parallel.ForEach(pratical?.Tests ?? [], test =>
                        {
                            StringBuilder testInfo = new StringBuilder();
                            if (test.Hidden)
                                testInfo.AppendLine("Executando teste fechado:");
                            else
                            {
                                testInfo.AppendLine("Executando teste aberto:");
                                string inputs = string.Join(',', test.Inputs);
                                testInfo.AppendLine($"Entrada: {inputs}.");
                                testInfo.AppendLine($"Saida esperada: {test.Output}.");
                            }

                            try
                            {
                                object output = default;
                                lock (func)
                                {
                                    output = func(
                                        test.Inputs
                                            .Select(x => x is JsonElement el && 
                                                el.TryGetInt32(out int value) ? value : x)
                                            .Select(x => x is JsonElement el && 
                                                el.TryGetSingle(out float value) ? value : x)
                                            .Select(x => x is JsonElement el ? el.GetString() : x)
                                            .ToArray()
                                        );
                                }
                                if (!test.Hidden)
                                    testInfo.AppendLine($"Saida: {output}.");
                                
                                if (output?.ToString() != test.Output.ToString())
                                {
                                    testInfo.AppendLine($"Resultado diferente do esperado. Falha no teste.");
                                    return;
                                }
                                    
                                testInfo.AppendLine($"Pontuação concedida.");
                                Interlocked.Increment(ref corrects);
                            }
                            catch (Exception ex)
                            {
                                testInfo.AppendLine("O seguinte erro de execução foi encontrado:");
                                testInfo.AppendLine(ex.Message);
                            }
                            finally
                            {
                                lock (runInfo)
                                {
                                    runInfo.AppendLine(testInfo.ToString());
                                }
                            }
                        })
                    );
                    lastResult[pratical] = runInfo.ToString();

                    float pontuation = corrects / (float)pratical.Tests.Count;
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
        
        while (current < 0)
            current += test.PraticalTests.Count;
        int index = current % test.PraticalTests.Count;
        var pratical = test.PraticalTests[index];
        int totalPages = 2 + docs[pratical].Keys.Count;
        while (page < 0)
            page += totalPages;
        int pageIndex = page % totalPages;
        if (pageIndex == 0)
        {
            var font = new Font("Arial", 40);
            g.DrawText(
                new Rectangle(5, 5, g.Width - 10, g.Height - 10),
                font, StringAlignment.Near, StringAlignment.Near,
                Brushes.LightCoral,
                $"""
                Questão Prática {index + 1} / {test.PraticalTests.Count}: {pratical.Text}
                
                Exemplos: {pratical.Example}
                Desempenho Crítico: {pratical.Performance}
                Linguagem: {pratical.Language}

                Use setar para cima e baixo para navegar entre as instruções e direita e
                esquerda para alterar a questão. Na última página você poderá executar
                o seu código.
                """
            );
        }
        else if (pageIndex < totalPages - 1)
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
        else
        {
            var font = new Font("Arial", 20);
            g.DrawText(
                new Rectangle(5, 5, g.Width / 2 - 5, g.Height - 10),
                font, StringAlignment.Near, StringAlignment.Near,
                Brushes.LightCoral,
                $"""
                Melhor Nota Obtida: {100 * bestResult[pratical]}%
                
                Edite o arquivo main.{pratical.Language} e pressione espaço para executar o código.
                """
            );
            font = new Font("Arial", 12);
            g.DrawText(
                new Rectangle(g.Width / 2 + 5, 5, g.Width / 2 - 10, g.Height - 10),
                font, StringAlignment.Near, StringAlignment.Near,
                Brushes.LightCoral, lastResult[pratical]
            );
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