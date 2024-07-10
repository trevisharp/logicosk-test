using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Pamella;

public class ResultView(Results results) : View
{
    int desloc = 24;
    Grade grade = results.GenerateGrade();
    List<int> gradeAnimation = [ 0, 0, 0, 0, 0, 0, 0 ];
    List<int> maxGradeAnimation = [ 0, 0, 0, 0, 0, 0, 0 ];
    List<string> teoricalAspects = [
        "                           A1: Pure Logic and Math:",
        "     A2: Low-Level Programming and Data Structures:",
        "A3: Modern Object Oriented and Funcional Languages:",
        "                          A4: Frontend Development:",
        "                          A5: Network and Security:",
        "              A6: Software Engineering and Quality:",
        "        A7: AI, Machine Learning and Deep Learning:"
    ];
    int praticalGrade = 0;
    int maxPraticalGrade;
    string praticalBar = "";
    List<Brush> colors = [ 
        Brushes.Black, Brushes.Black, Brushes.Black,
        Brushes.Black, Brushes.Black, Brushes.Black,
        Brushes.Black
    ];
    List<string> bars = [
        "", "", "", "", "", "", ""
    ];
    DateTime animationStart;
    int lastTick = -1;
    protected override void OnStart(IGraphics g)
    {
        animationStart = DateTime.Now;
        maxGradeAnimation = [
            grade.PureLogicAndMathAspect,
            grade.LowLevelProgrammingAndDataStructureAspect,
            grade.ModernObjectOrientedAndFuncionalLanguagesAspect,
            grade.FrontendDevelopmentAspect,
            grade.NetwordAndSecurityAspect,
            grade.SoftwareEngineeringAndQualityAspect,
            grade.AIMachineLearningAndDeepLearningAspect
        ];
        maxPraticalGrade = grade.PraticalAspect;
        colors = [
            Brushes.DarkBlue,
            Brushes.BlueViolet,
            Brushes.Purple,
            Brushes.DarkOrange,
            Brushes.Brown,
            Brushes.SteelBlue,
            Brushes.Black
        ];
    }

    protected override void OnFrame(IGraphics g)
    {
        var time = DateTime.Now - animationStart;
        int tick = (int)(10 * time.TotalSeconds);

        if (tick == lastTick)
            return;
        
        while (lastTick < tick)
        {
            lastTick++;
            onTick();
        }

        Invalidate();
    }

    protected override void OnRender(IGraphics g)
    {
        int part = g.Width / 10;
        g.Clear(Color.Wheat);
        g.DrawText(
            new RectangleF(5, 5, g.Width - 10, 45),
            new Font("Monospace", 30f), StringAlignment.Center,
            StringAlignment.Center, Brushes.Black, "Resultados:"
        );
        g.FillRectangle(20, 51, g.Width - 40, 3, Brushes.Black);

        for (int i = 0; i < 7; i++)
        {
            g.DrawText(
                new RectangleF(5, 55 + i * desloc, 4 * part - 10, desloc),
                new Font("Monospace", 20f), StringAlignment.Far,
                StringAlignment.Center, colors[i], teoricalAspects[i]
            );
            g.DrawText(
                new RectangleF(4 * part + 5, 55 + i * desloc, 5 * part - 10, desloc),
                new Font("Monospace", 20f), StringAlignment.Near,
                StringAlignment.Center, colors[i], bars[i]
            );
            g.DrawText(
                new RectangleF(9 * part + 5, 55 + i * desloc, part - 10, desloc),
                new Font("Monospace", 20f), StringAlignment.Center,
                StringAlignment.Center, colors[i], gradeAnimation[i].ToString()
            );
        }
        int questionEnd = 55 + 7 * desloc + desloc;
        g.FillRectangle(20, questionEnd + 1, g.Width - 40, 3, Brushes.Black);
        
        int praticalStart = questionEnd + 9;
        g.DrawText(
            new RectangleF(5, praticalStart, 4 * part - 10, 3 * desloc / 2),
            new Font("Monospace", 30f), StringAlignment.Far,
            StringAlignment.Center, Brushes.DarkGreen, "Resultado Prático:"
        );
        g.DrawText(
            new RectangleF(4 * part + 5, praticalStart, 5 * part - 10, 3 * desloc / 2),
            new Font("Monospace", 20f), StringAlignment.Near,
            StringAlignment.Center, Brushes.DarkGreen, praticalBar
        );
        g.DrawText(
            new RectangleF(9 * part + 5, praticalStart, part - 10, 3 * desloc / 2),
            new Font("Monospace", 20f), StringAlignment.Center,
            StringAlignment.Center, Brushes.DarkGreen, praticalGrade.ToString()
        );
        int praticalEnd = praticalStart + 3 * desloc / 2 + 5;
        g.FillRectangle(20, praticalEnd + 1, g.Width - 40, 3, Brushes.Black);
        
        
    }

    void onTick()
    {
        int[] sums = [ 
            grade.PureLogicAndMathAspect / 20,
            grade.LowLevelProgrammingAndDataStructureAspect / 20,
            grade.ModernObjectOrientedAndFuncionalLanguagesAspect / 20,
            grade.FrontendDevelopmentAspect / 20,
            grade.NetwordAndSecurityAspect / 20,
            grade.SoftwareEngineeringAndQualityAspect / 20,
            grade.AIMachineLearningAndDeepLearningAspect / 20
        ];

        for (int i = 0; i < 7; i++)
        {
            gradeAnimation[i] += sums[i];
            if (gradeAnimation[i] > maxGradeAnimation[i])
                gradeAnimation[i] = maxGradeAnimation[i];

            int barCount = (int)(gradeAnimation[i] / 12.5);
            bars[i] = string.Concat(Enumerable.Repeat("█ ", barCount / 2)) 
                + (barCount % 2 == 1 ? "▌" : "");
        }

        int psum = grade.PraticalAspect / 20;
        praticalGrade += psum;
        if (praticalGrade > maxPraticalGrade)
            praticalGrade = maxPraticalGrade;

        int pbarCount = (int)(praticalGrade / 12.5);
        praticalBar = string.Concat(Enumerable.Repeat("█ ", pbarCount / 2)) 
            + (pbarCount % 2 == 1 ? "▌" : "");
    }
}