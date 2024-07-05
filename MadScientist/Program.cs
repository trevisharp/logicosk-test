using System;
using Logicosk;

using System.Drawing;

Test test = new Test([
    new Question([], "", "q7.png", [])
]);
await TestManager.Save("result.test", test);

Test test1 = await TestManager.Open("result.test");
System.Console.WriteLine(test1.Questions.Count);