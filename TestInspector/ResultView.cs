using Pamella;

public class ResultView(Results results) : View
{
    Grade grade = results.GenerateGrade();
    protected override void OnStart(IGraphics g)
    {
        
    }

    protected override void OnRender(IGraphics g)
    {
        
    }
}