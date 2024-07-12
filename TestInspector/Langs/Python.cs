using System;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;

public class Python : RealLanguage
{
    public override string BaseCode =>
        """
        def main(args):
            return "não implementado"

        # Não alterar
        if __name__ == "__main__":
            import sys
            args = sys.argv[1:]
            mainInput = []
            for arg in args:
                if arg.isnumeric():
                    mainInput.append(int(arg))
                else:
                    mainInput.append(arg)
            print(main(mainInput))
        """;

    public override Func<T, object> Compile<T>(string source, StringBuilder sb)
    {
        return input => {
            object output = null;
            string parameter = 
                input.GetType().IsArray ?
                string.Join(' ', input as object[]) :
                input.ToString();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"python -u main.py {parameter}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                }
            };
            process.Start();

            int selected = Task.WaitAny(
                Task.Delay(5000),
                Task.Run(() => {
                    process.WaitForExit();
                    output = process.StandardOutput
                        .ReadToEnd()
                        .Replace("\n", "")
                        .Trim();
                })
            );
            if (selected == 0)
                return "O algoritmo demorou demais para completar.";
            
            return output;
        };
    }

    public override Assembly CompileAssembly(string source, StringBuilder sb)
        => throw new Exception("Python can't be compiled to assembly type");
}