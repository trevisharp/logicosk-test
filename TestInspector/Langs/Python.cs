using System;
using System.IO;
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


        # @@ Não alterar
        if __name__ == "__main__":
            import sys
            path = sys.argv[1]

            mainInput = []
            with open(path, "r") as file:
                for line in file.readlines():
                    mainInput.extend([float(i) for i in line.split(" ") if i.strip()])
            
            import time

            start = time.time()
            print(main(mainInput))
            end = time.time()
            print(end - start)
        # @@
        """;

    public override Func<T, object> Compile<T>(string source, StringBuilder sb)
    {
        var constantFile = File.ReadAllText(source).Split("@@")[1];
        var constantBaseFile = BaseCode.Split("@@")[1];
        if (constantBaseFile != constantFile)
            return input => "arquivo modificado.";

        return input => {
            object output = "sem output";
            float time = 0;
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
                    RedirectStandardError= true,
                    CreateNoWindow = true
                }
            };
            process.Start();

            int selected = Task.WaitAny(
                Task.Delay(5000),
                Task.Run(() => {
                    try
                    {
                        process.WaitForExit();
                        var data = process.StandardOutput
                            .ReadToEnd()
                            .Split("\n");
                        
                        output = data.Length > 0 ? data[0].Trim() : string.Empty;

                        if (data.Length > 1 && float.TryParse(data[1], out float f))
                            time = f;
                        
                        output += process.StandardError
                            .ReadToEnd();
                    }
                    catch (Exception ex)
                    {
                        output = ex.Message;
                    }
                })
            );

            Directory.Delete(temp.FullName, true);
            if (selected == 0)
                return "O algoritmo demorou indefinidamente para terminar.";
            
            // TODO: Trocar por limite de tempo dinâmico
            if (time > 0.01f)
                return "O algoritmo ainda está lento!";
            
            return output;
        };
    }

    public override Assembly CompileAssembly(string source, StringBuilder sb)
        => throw new Exception("Python can't be compiled to assembly type");
}