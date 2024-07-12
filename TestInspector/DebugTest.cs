using System;
using System.IO;
using System.Linq;
using System.Drawing;

using Pamella;
using Logicosk;
using System.Text;
using System.Collections.Generic;

public class DebugTest(
    Results results,
    Action<Input> oldEvent
    ) : View
{
    Test test = results.Test;
    List<Lab> labs = [];
    List<Language> langs = [];
    List<string> files = [];
    float[] grades;
    int level;
    bool helpView = false;

    protected override void OnStart(IGraphics g)
    {
        level = 0;
        results.LevelAvaliations = grades = new float[results.Test.BugfixTests.Count];

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
        g.SubscribeKeyDownEvent(key =>
        {
            switch (key)
            {
                case Input.Space:
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
            }

            if (key != Input.Escape)
                return;

            App.Pop();
            App.Push(new ResultView(results));
        });
    }

    DateTime last = DateTime.Now;
    protected override void OnRender(IGraphics g)
    {
        var now = DateTime.Now;
        var time = now - last;
        last = now;
        var dt = (float)time.TotalSeconds;

        g.Clear(Color.FromArgb(40, 10, 10));
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
    }
}