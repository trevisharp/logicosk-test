using System;
using Logicosk;

using System.Drawing;

var questions = await TestBuilder.GetQuestions("../../S1", 1);
var test = new Test("../../S1", questions);
await TestManager.Save("result.test", test);
Test test1 = await TestManager.Open("result.test");
System.Console.WriteLine(test1.Questions[0]);