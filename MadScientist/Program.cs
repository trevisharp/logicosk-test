using System;
using Logicosk;

Test test = new Test([]);
await TestManager.Save("result.test", test);

Test test1 = await TestManager.Open("result.test");
System.Console.WriteLine(test1.Questions.Count);