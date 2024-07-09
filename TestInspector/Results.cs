using System.Collections.Generic;

using Logicosk;

public class Results
{
    public Test Test { get; set; }
    public Dictionary<Question, Alternative> Answers { get; set; } = new();
    public Dictionary<PraticalTest, float> BestResults { get; set; } = new();
}