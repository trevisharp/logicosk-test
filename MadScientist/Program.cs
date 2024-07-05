using System;
using Logicosk;

using System.Drawing;

var test = await TestBuilder.CreateTest("../../repo");
await TestManager.Save("result.test", test);

Test test1 = await TestManager.Open("result.test");
System.Console.WriteLine(test1.Questions[0]);