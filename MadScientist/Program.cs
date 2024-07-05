using Logicosk;
using System.IO;

if (args[0] == "clear")
{
    Directory.Delete("extra", true);
    File.Delete("result.test");
    File.Delete("current.key");
    return;
}

if (args[0] == "new")
{
    var test = await TestBuilder.CreateTest(args[1]);
    await TestManager.Save("result.test", test);
    return;
}
