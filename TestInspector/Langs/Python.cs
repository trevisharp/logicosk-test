using System;
using System.Text;
using System.Diagnostics;
using Pamella;
using System.Linq;
using System.Collections;

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
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            process.Start();
            process.WaitForExit();
            object output = process.StandardOutput
                .ReadToEnd()
                .Replace("\n", "")
                .Trim();
            
            return output;
        };
    }
}   