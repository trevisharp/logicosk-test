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
    const int praticalCount = 3;
    const int bugFixCount = 6;
    public static async Task<Test> CreateTest(string baseFolder)
    {
        var questions = await GetQuestions(
            Path.Combine(baseFolder, "S1"), questionCount
        );

        var pratical = await GetPraticalTests(
            Path.Combine(baseFolder, "S2"), praticalCount
        );

        var bugfix = await GetBugfixTests(
            Path.Combine(baseFolder, "S3"), bugFixCount
        );

        var test = new Test(baseFolder, duration, questions, pratical, bugfix);
        return test;
    }

    public static async Task<List<PraticalTest>> GetPraticalTests(string questionFolder, int size)
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
            .Select(async file => JsonSerializer.Deserialize<PraticalTest>(await file, options))
            .ToArray();
        
        List<PraticalTest> questions = [];
        foreach (var item in selectedQuestions)
            questions.Add(await item);
        
        return questions;
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

    public static async Task<List<BugfixTest>> GetBugfixTests(string questionFolder, int size)
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
            .Select(async file => JsonSerializer.Deserialize<BugfixTest>(await file, options))
            .ToArray();
        
        List<BugfixTest> bugfixes = [];
        foreach (var item in selectedQuestions)
            bugfixes.Add(await item);
        
        return bugfixes;
    }
}