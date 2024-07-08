using Logicosk;
using System.IO;

const string seed = "etsps2024401";

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
    await TestManager.Save("result.test", seed, test);
    return;
}
