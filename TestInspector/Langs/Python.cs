using System;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;

public class Python : RealLanguage
{
    public override string BaseCode =>
        """
        def main(args):
            return "não implementado"


        # Não alterar
        if __name__ == "__main__":
            import sys
            path = sys.argv[1]

            mainInput = []
            with open(path, "r") as file:
                for line in file.readlines():
                    mainInput.extend([float(i) for i in line.split(" ") if i.strip()])
            
            print(main(mainInput))
        """;

    public override Func<T, object> Compile<T>(string source, StringBuilder sb)
    {
        return input => {
            object output = "sem output";
            string data = 
                input.GetType().IsArray ?
                string.Join(' ', input as object[]) :
                input.ToString();
            
            var temp = Directory.CreateTempSubdirectory();
            var path = Path.Combine(temp.FullName, "challenge.txt");
            File.WriteAllText(path, data.Replace(',', '.'));

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"python -u main.py {path}",
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

            Directory.Delete(temp.FullName, true);
            if (selected == 0)
                return "O algoritmo demorou demais para completar.";
            
            
            return output;
        };
    }

    public override Assembly CompileAssembly(string source, StringBuilder sb)
        => throw new Exception("Python can't be compiled to assembly type");
}