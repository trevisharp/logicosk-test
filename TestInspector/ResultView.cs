using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

using Pamella;

public class ResultView(Action<Input> oldKeyUp, Action<Input> oldKeyDown) : View
{
    int desloc = 24;
    Grade grade = Results.Current.GenerateGrade();
    List<float> gradeAnimation = [ 0, 0, 0, 0, 0, 0, 0 ];
    List<float> maxGradeAnimation = [ 0, 0, 0, 0, 0, 0, 0 ];
    List<string> teoricalAspects = [
        "                           A1: Pure Logic and Math:",
        "     A2: Low-Level Programming and Data Structures:",
        "A3: Modern Object Oriented and Funcional Languages:",
        "                          A4: Frontend Development:",
        "                          A5: Network and Security:",
        "              A6: Software Engineering and Quality:",
        "        A7: AI, Machine Learning and Deep Learning:"
    ];
    float praticalGrade = 0;
    float maxPraticalGrade;
    float bugFixGrade = 0;
    float maxbugFixGrade;
    float finalGrade = 0;
    string praticalBar = "";
    string bugFixBar = "";
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
        g.UnsubscribeKeyUpEvent(oldKeyUp);
        g.UnsubscribeKeyDownEvent(oldKeyDown);
        
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
        maxbugFixGrade = grade.BugfixAspect;
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
                StringAlignment.Center, colors[i], ((int)gradeAnimation[i]).ToString()
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
            StringAlignment.Center, Brushes.DarkGreen, ((int)praticalGrade).ToString()
        );
        int praticalEnd = praticalStart + 3 * desloc / 2 + 5;
        g.FillRectangle(20, praticalEnd + 1, g.Width - 40, 3, Brushes.Black);

        int bugFixStart = praticalEnd + 9;
        g.DrawText(
            new RectangleF(5, bugFixStart, 4 * part - 10, 3 * desloc / 2),
            new Font("Monospace", 30f), StringAlignment.Far,
            StringAlignment.Center, Brushes.DarkGreen, "Resultado do Bugfix:"
        );
        g.DrawText(
            new RectangleF(4 * part + 5, bugFixStart, 5 * part - 10, 3 * desloc / 2),
            new Font("Monospace", 20f), StringAlignment.Near,
            StringAlignment.Center, Brushes.DarkGreen, bugFixBar
        );
        g.DrawText(
            new RectangleF(9 * part + 5, bugFixStart, part - 10, 3 * desloc / 2),
            new Font("Monospace", 20f), StringAlignment.Center,
            StringAlignment.Center, Brushes.DarkGreen, ((int)bugFixGrade).ToString()
        );
        int bugFixEnd = bugFixStart + 3 * desloc / 2 + 5;
        g.FillRectangle(20, bugFixEnd + 1, g.Width - 40, 3, Brushes.Black);

        int finalStart = bugFixEnd + 9;
        g.DrawText(
            new RectangleF(5, finalStart, g.Width - 10, 40),
            new Font("Monospace", 30f), StringAlignment.Center,
            StringAlignment.Center, Brushes.DarkGreen, "Resultado Final:"
        );
        int gradeSize = (g.Height - 50 - finalStart) / 2;
        g.DrawText(
            new RectangleF(5, finalStart, g.Width - 10, gradeSize),
            new Font("Monospace", 120f), StringAlignment.Center,
            StringAlignment.Center, Brushes.DarkGreen, ((int)finalGrade).ToString()
        );
        int classStart = finalStart += gradeSize;
        g.DrawText(
            new RectangleF(5, classStart, g.Width - 10, gradeSize),
            new Font("Monospace", 120f), StringAlignment.Center,
            StringAlignment.Center, Brushes.DarkGreen, finalGrade switch
            {
                <50 => "F",   
                >=050 and <150 => "E",   
                >=150 and <300 => "D",   
                >=300 and <500 => "C",   
                >=500 and <700 => "B",   
                >=700 and <850 => "A",   
                >=850 and <950 => "S",   
                _ => "L",   
            }
        );
        
    }

    void onTick()
    {
        const float div = 50;
        float[] sums = [ 
            grade.PureLogicAndMathAspect / div,
            grade.LowLevelProgrammingAndDataStructureAspect / div,
            grade.ModernObjectOrientedAndFuncionalLanguagesAspect / div,
            grade.FrontendDevelopmentAspect / div,
            grade.NetwordAndSecurityAspect / div,
            grade.SoftwareEngineeringAndQualityAspect / div,
            grade.AIMachineLearningAndDeepLearningAspect / div
        ];

        for (int i = 0; i < 7; i++)
        {
            gradeAnimation[i] += sums[i];
            if (gradeAnimation[i] > maxGradeAnimation[i])
                gradeAnimation[i] = maxGradeAnimation[i];

            float barCount = gradeAnimation[i] / 12.5f;
            bars[i] = string.Concat(Enumerable.Repeat("█ ", (int)(barCount / 2))) 
                + (barCount % 2 == 1 ? "▌" : "");
        }

        // Pratical Bar
        float psum = grade.PraticalAspect / div;
        praticalGrade += psum;
        if (praticalGrade > maxPraticalGrade)
            praticalGrade = maxPraticalGrade;

        float pbarCount = praticalGrade / 12.5f;
        praticalBar = string.Concat(Enumerable.Repeat("█ ", (int)(pbarCount / 2))) 
            + (pbarCount % 2 == 1 ? "▌" : "");

        // Bugfix Bar
        float bsum = grade.BugfixAspect / div;
        bugFixGrade += bsum;
        if (bugFixGrade > maxbugFixGrade)
            bugFixGrade = maxbugFixGrade;

        float bbarCount = bugFixGrade / 12.5f;
        bugFixBar = string.Concat(Enumerable.Repeat("█ ", (int)(bbarCount / 2))) 
            + (bbarCount % 2 == 1 ? "▌" : "");
        
        // Final Grade
        float rating = grade.Rating;
        float fsum = rating / div;
        finalGrade += fsum;
        if (finalGrade > rating)
            finalGrade = rating;
    }
}