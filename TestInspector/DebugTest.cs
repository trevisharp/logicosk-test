using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Pamella;
using Logicosk;

public class DebugTest(
    Action<Input> oldEvent
    ) : View
{
    Test test = Results.Current.Test;
    List<Lab> labs = [];
    List<Language> langs = [];
    List<string> files = [];
    float[] grades;
    int level;
    bool helpView = false;
    bool waitingEnd = false;
    DateTime last = DateTime.Now;
    DateTime waitingTime;
    DateTime spaceTime = DateTime.MaxValue;
    Action<Input> oldKeyEvent;
    private DateTime testFinal;

    protected override void OnStart(IGraphics g)
    {
        level = 0;
        Results.Current.LevelAvaliations = grades = new float[Results.Current.Test.BugfixTests.Count];
        testFinal = DateTime.Now.Add(
            TimeSpan.FromMinutes(test.MinutesDuration)
        );

        int index = 0;
        foreach (var bugFix in test.BugfixTests)
        {
            var lang = Language.New(bugFix.Language);
            langs.Add(lang);

            var lab = Lab.New(bugFix.Lab);
            lab.LoadParams(bugFix.LabParams);
            labs.Add(lab);

            var file = $"main.{lab}.{index++}.{bugFix.Language}";
            files.Add(file);
            var code = string.Join('\n', bugFix.BaseCode);
            File.WriteAllText(file, code);
        }

        AlwaysInvalidateMode();

        if (oldEvent is not null)
            g.UnsubscribeKeyDownEvent(oldEvent);
        g.SubscribeKeyDownEvent(oldKeyEvent = key =>
        {
            switch (key)
            {
                case Input.Space:
                    if (spaceTime == DateTime.MaxValue && waitingEnd)
                        spaceTime = DateTime.Now;
                    
                    if (waitingEnd)
                        return;

                    var sb = new StringBuilder();
                    var assembly = langs[level].CompileAssembly(files[level], sb);
                    if (assembly is null)
                    {
                        System.Windows.Forms.MessageBox.Show(sb.ToString());
                        return;
                    }
                    var type = assembly.GetType("TestePratico");
                    labs[level].Reset();
                    labs[level].LoadBehaviour(type);
                    var grade = labs[level].Avaliate();
                    if (grade > grades[level])
                        grades[level] = grade;
                    break;
                

                case Input.H:
                    helpView = !helpView;
                    break;
                

                case Input.F:
                    if (waitingEnd)
                    {
                        var totalWaitingTime = DateTime.Now - waitingTime;
                        if (totalWaitingTime.TotalMinutes > 3)
                            break;
                    }
                    waitingTime = DateTime.Now;
                    waitingEnd = !waitingEnd;
                    break;
                

                case Input.Left:
                    level--;
                    if (level < 0)
                        level = test.BugfixTests.Count - 1;
                    break;
                
                
                case Input.Right:
                    level++;
                    if (level >= test.BugfixTests.Count)
                        level = 0;
                    break;
            }
        });
    }

    protected override void OnFrame(IGraphics g)
    {
        var time = DateTime.Now - spaceTime;
        if (time.TotalSeconds > 2f && waitingEnd)
        {
            App.Pop();
            App.Push(new ResultView(oldKeyEvent));
        }
    }

    protected override void OnRender(IGraphics g)
    {
        g.Clear(Color.FromArgb(40, 10, 10));
        if (Results.Current is null)
            return;
        
        if (waitingEnd)
        {
            g.DrawText(
                new Rectangle(5, 5, g.Width - 10, g.Height - 10),
                new Font("Arial", 140), 
                StringAlignment.Center, StringAlignment.Center,
                Brushes.White, "Aguardando..."
            );
            g.DrawText(
                new Rectangle(5, g.Height - 200, g.Width - 10, 200),
                new Font("Arial", 20), 
                StringAlignment.Center, StringAlignment.Center,
                Brushes.White,
                """
                Segure o espaço para finalizar a prova com antecedência.
                Aperte F para voltar a realizar a prova.
                Ficar mais de 3 minutos na tela de "aguardando..." impossibilitará você
                de voltar a fazer a prova.
                """
            );
            timeCheck();
            return;
        }

        var now = DateTime.Now;
        var time = now - last;
        last = now;
        var dt = (float)time.TotalSeconds;

        if (helpView)
        {
            g.DrawText(
                new RectangleF(0, 0, g.Width, g.Height), 
                new Font("Arial", 16),
                Brushes.White, 
                $"""
                {test.BugfixTests[level].Description}

                Use setas para navegar entre os diferentes níveis e espaço para atualizar
                o código do nível atual com base no arquivo base da prova.
                """
            );
            return;
        }

        labs[level].Interact(dt);
        labs[level].Draw(g);

        g.DrawText(
            new RectangleF(0, 0, 200, 40), 
            new Font("Arial", 16),
            Brushes.White, 
            $"Melhor Nota: {100 * grades[level]}"
        );

        g.DrawText(
            new RectangleF(0, g.Height - 40, 200, 40), 
            new Font("Arial", 16),
            Brushes.White, 
            $"Pressione h para ver a tela de ajuda..."
        );

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
                App.Push(new ResultView(oldKeyEvent));
            }
        }
    }
}