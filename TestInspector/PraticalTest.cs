using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using Pamella;
using Logicosk;

class PraticalView(Action<Input> oldEv) : View
{
    Test test = Results.Current.Test;
    Dictionary<PraticalTest, Dictionary<string, string>> docs = [];
    Dictionary<PraticalTest, string> lastResult = [];
    int current = 0;
    int page = 0;
    DateTime testFinal;
    int spacing = 22;
    int loading = -1;
    bool waitingEnd = false;
    DateTime waitingTime = DateTime.MaxValue;
    DateTime fTime = DateTime.MaxValue;
    Action<Input> oldKeydown;
    private Action<Input> oldKeyUp;

    protected override void OnStart(IGraphics g)
    {
        g.UnsubscribeKeyDownEvent(oldEv);
        AlwaysInvalidateMode();

        foreach (var pratical in test.PraticalTests)
        {
            var file = $"main.{pratical.Language}";
            var lang = Language.New(pratical.Language);
            File.WriteAllText(file, lang.BaseCode);
            Results.Current.BestResults.Add(pratical, 0f);
            docs.Add(pratical, lang.Tutorial());
            lastResult.Add(pratical, "Nenhuma execução registrada...");
        }

        testFinal = DateTime.Now.AddMinutes(test.MinutesDuration);

        g.SubscribeKeyDownEvent(oldKeydown = async key => {
            
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


                case Input.F:
                    if (fTime == DateTime.MaxValue) {
                        fTime = DateTime.Now;
                    }

                    if (!waitingEnd)
                        waitingEnd = true;
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
                    
                    if (fTime == DateTime.MaxValue && waitingEnd)
                        fTime = DateTime.Now;

                    if (loading != -1)
                        return;
                    loading = 0;
                    var pratical = test.PraticalTests[current % test.PraticalTests.Count];
                    var compiler = Language.New(pratical.Language);
                    
                    var file = $"main.{pratical.Language}";
                    
                    var runInfo = new StringBuilder();
                    var func = compiler.Compile<object[]>(file, runInfo);
                    if (runInfo.Length > 0)
                        lastResult[pratical] = runInfo.ToString();
                    if (func is null)
                    {
                        loading = -1;
                        return;
                    }
                    
                    int corrects = 0;
                    var testList = pratical?.Tests ?? [];
                    var testCount = int.Max(testList.Count, 1);
                    await Task.Run(() =>
                        Parallel.ForEach(testList, test =>
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
                                    output = func(InputAnalyzer.Analyze(test.Inputs));
                                }
                                Interlocked.Add(ref loading, 50 / testCount);
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
                                System.Windows.Forms.MessageBox.Show(ex.StackTrace);
                                testInfo.AppendLine("O seguinte erro de execução foi encontrado:");
                                testInfo.AppendLine(ex.Message);
                            }
                            finally
                            {
                                lock (runInfo)
                                {
                                    runInfo.AppendLine(testInfo.ToString());
                                }
                                Interlocked.Add(ref loading, 50 / testCount);
                            }
                        })
                    );
                    loading = 100;
                    runInfo.AppendLine();
                    runInfo.AppendLine("Testes completos...");
                    lastResult[pratical] = runInfo.ToString();

                    float pontuation = corrects / (float)pratical.Tests.Count;
                    if (Results.Current.BestResults[pratical] < pontuation)
                        Results.Current.BestResults[pratical] = pontuation;
                    loading = -1;
                    break;
            }
        });
        g.SubscribeKeyUpEvent(oldKeyUp = key => {
            if (key == Input.F)
            {
                var time = DateTime.Now - fTime;
                if (time.TotalSeconds > 2f)
                {
                    App.Clear();
                    App.Push(new ThridView(oldKeyUp, oldKeydown));
                    return;
                }

                waitingEnd = !waitingEnd;
                fTime = DateTime.MaxValue;
            }
        });
    }

    protected override void OnRender(IGraphics g)
    {
        g.Clear(Color.LightSeaGreen);
        var time = DateTime.Now - fTime;
        if (test is null)
            return;
        
        if (waitingEnd)
        {
            g.DrawText(
                new Rectangle(5, 5, g.Width - 10, g.Height - 10),
                new Font("Arial", 140), 
                StringAlignment.Center, StringAlignment.Center,
                "Finalizando..."
            );
            g.DrawText(
                new Rectangle(5, g.Height - 200, g.Width - 10, 200),
                new Font("Arial", 20), 
                StringAlignment.Center, StringAlignment.Center,
                time.TotalSeconds > 2f ? "Largue o botão F para avançar." :
                """
                Continue segurando o botão F para finalizar a prova com antecedência.
                Largue o botão F para voltar a realizar a prova.
                """
            );
            timeCheck();
            return;
        }
        
        while (current < 0)
            current += test.PraticalTests.Count;
        int index = current % test.PraticalTests.Count;
        var pratical = test.PraticalTests[index];
        int totalPages = 2 + docs[pratical].Keys.Count;
        while (page < 0)
            page += totalPages;
        int pageIndex = page % totalPages;

        g.DrawText(
            new Rectangle(5, g.Height - 55, 150, 60),
            new Font("Arial", 20),
            StringAlignment.Center, StringAlignment.Center,
            Brushes.Black,
            $"↕ ({pageIndex + 1} / {totalPages})"
        );

        if (pageIndex == 0)
        {
            var font = new Font("Arial", 40);
            g.DrawText(
                new Rectangle(5, 5, g.Width - 10, g.Height - 10),
                font, StringAlignment.Near, StringAlignment.Near,
                Brushes.Black,
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
                Brushes.Black, $"Questão Prática {index + 1} / {test.PraticalTests.Count}: Especificação da Lingugem:\n\n" +
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
                    code ? Brushes.DarkBlue : Brushes.Black, part
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
                Brushes.Black,
                $"""
                Questão Prática {index + 1} / {test.PraticalTests.Count}:
                Melhor Nota Obtida: {100 * Results.Current.BestResults[pratical]}%
                {(loading == -1 ? "" : $"Rodando: {loading}%")}
                
                Edite o arquivo main.{pratical.Language} e pressione espaço para executar o código.
                """
            );
            font = new Font("Arial", 12);
            g.DrawText(
                new Rectangle(g.Width / 2 + 5, 5, g.Width / 2 - 10, g.Height - 10),
                font, StringAlignment.Near, StringAlignment.Near,
                Brushes.Black, lastResult[pratical]
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
                App.Push(new ThridView(oldKeydown, oldKeyUp));
            }
        }
    }
}