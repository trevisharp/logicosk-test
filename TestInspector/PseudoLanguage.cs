using System;
using System.IO;
using System.Text;

public abstract class PseudoLanguage : Language
{
    protected abstract string convert(string source, StringBuilder sb);

    public override string BaseCode => "";

    public override Func<T, object> Compile<T>(string source, StringBuilder sb)
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
        if (assembly is null)
            return null;
        
        var defaultType = assembly.GetType("TestePratico");
        var mainCode = defaultType.GetMethod("main");
        return x => mainCode.Invoke(null, [x]);
    }
}