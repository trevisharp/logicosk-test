using System.Collections.Generic;
using System.Linq;
using Logicosk;

public class Results
{
    public Test Test { get; set; }
    public Dictionary<Question, Alternative> Answers { get; set; } = new();
    public Dictionary<PraticalTest, float> BestResults { get; set; } = new();

    public Grade GenerateGrade()
    {
        float[] aspects = new float[14];
        foreach (var pair in Answers)
        {
            foreach (var aspect in pair.Key.Aspects)
            {
                int index = 2 * (aspect - 1);
                aspects[index] += pair.Value.Correctness;
                aspects[index + 1] += 1f;
            }
        }

        for (int i = 1; i < 14; i += 2)
            if (aspects[i] == 0)
                aspects[i] = 1;

        return new() {
            PureLogicAndMathAspect = (int)(1000 * aspects[0] / aspects[1]),
            LowLevelProgrammingAndDataStructureAspect = (int)(1000 * aspects[2] / aspects[3]),
            ModernObjectOrientedAndFuncionalLanguagesAspect = (int)(1000 * aspects[4] / aspects[5]),
            FrontendDevelopmentAspect = (int)(1000 * aspects[6] / aspects[7]),
            NetwordAndSecurityAspect = (int)(1000 * aspects[8] / aspects[9]),
            SoftwareEngineeringAndQualityAspect = (int)(1000 * aspects[10] / aspects[11]),
            AIMachineLearningAndDeepLearningAspect = (int)(1000 * aspects[12] / aspects[13]),
            PraticalAspect = (int)(1000 * BestResults.Values.Average()),
            BugfixAspect = 0
        };
    }
}

public class Grade
{
    public int PureLogicAndMathAspect { get; init; }
    public int LowLevelProgrammingAndDataStructureAspect { get; init; }
    public int ModernObjectOrientedAndFuncionalLanguagesAspect { get; init; }
    public int FrontendDevelopmentAspect { get; init; }
    public int NetwordAndSecurityAspect { get; init;}
    public int SoftwareEngineeringAndQualityAspect { get; init; }
    public int AIMachineLearningAndDeepLearningAspect { get; init; }
    public int PraticalAspect { get; init; }
    public int BugfixAspect { get; init; }

    public float Rating =>
        70 * PureLogicAndMathAspect / 1000f + 
        70 * LowLevelProgrammingAndDataStructureAspect / 1000f + 
        70 * ModernObjectOrientedAndFuncionalLanguagesAspect / 1000f + 
        70 * FrontendDevelopmentAspect / 1000f + 
        70 * NetwordAndSecurityAspect / 1000f + 
        70 * SoftwareEngineeringAndQualityAspect / 1000f + 
        70 * AIMachineLearningAndDeepLearningAspect / 1000f + 
        300 * PraticalAspect / 1000f +
        210 * BugfixAspect / 1000f;
}