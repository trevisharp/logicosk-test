using System;
using System.Text;
using System.Diagnostics;

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
            print(main(args))
        """;

    public override Func<T, object> Compile<T>(string source, StringBuilder sb)
    {
        return input => {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "python -u main.py 0 1 2",
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