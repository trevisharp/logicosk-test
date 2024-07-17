using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public abstract class PseudoLanguage : Language
{
    protected abstract string convert(string source, StringBuilder sb);

    public override string BaseCode => "";

    public override Func<T, object> Compile<T>(string source, StringBuilder sb)
    {
        var assembly = CompileAssembly(source, sb);
        if (assembly is null)
            return null;
        
        var defaultType = assembly.GetType("TestePratico");
        var mainCode = defaultType.GetMethod("main");
        return x =>
        {
            object output = null;
            Task.WaitAny(
                Task.Delay(500),
                Task.Run(() => output = mainCode.Invoke(null, [x]))
            );
            
            return output ?? "O código demorou muito para terminar de rodar.";
        };
    }

    public override Assembly CompileAssembly(string source, StringBuilder sb)
    {
        var code = File.ReadAllText(source);
        code = convert(code, sb);
        if (sb.Length > 0)
        {
            sb.Insert(0, "Erros de Sintaxes encontrados:\n");
            return null;
        }

        var assembly = Compiler.Compile(code, sb);
        if (sb.Length > 0)
        {
            sb.Insert(0, "Erros Semânticos encontrados:\n");
            return null;
        }
        return assembly;
    }
}