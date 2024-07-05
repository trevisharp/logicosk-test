using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Logicosk;

public static class TestBuilder
{
    const int questionCount = 30;
    const int duration = 60;
    public static async Task<Test> CreateTest(string baseFolder)
    {
        var questions = await GetQuestions(
            Path.Combine(baseFolder, "S1"), questionCount
        );

        var test = new Test(baseFolder, duration, questions);
        return test;
    }

    public static async Task<List<Question>> GetQuestions(string questionFolder, int size)
    {
        var questionFiles = 
            from file in Directory.GetFiles(questionFolder)
            where Path.GetExtension(file) == ".json"
            select file;
        
        var options = new JsonSerializerOptions() {
            PropertyNameCaseInsensitive = true
        };

        var selectedQuestions = questionFiles
            .OrderBy(q => Random.Shared.Next())
            .Take(size)
            .Select(async file => await File.ReadAllTextAsync(file))
            .Select(async file => JsonSerializer.Deserialize<Question>(await file, options))
            .ToArray();
        
        List<Question> questions = [];
        foreach (var item in selectedQuestions)
            questions.Add(await item);
        
        return questions;
    }
}