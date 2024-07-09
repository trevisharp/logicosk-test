using System.Collections.Generic;

using Logicosk;

public class Results
{
    public Dictionary<Question, Alternative> Answers { get; set; } = new();
    public Dictionary<PraticalTest, float> BestResults { get; set; } = new();
}