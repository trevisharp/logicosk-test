using System;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

public abstract class Language
{
    public static Language New(string lang)
        => lang switch
        {
            "py" => new Python(),
            "ebf" => new ExtendedBrainfuck(),
            _ => throw new NotImplementedException($"Not Implemented Language '{lang}'")
        };

    public abstract Dictionary<string, string> Tutorial();
    
    public abstract Func<T, object> Compile<T>(string source, StringBuilder sb);

    public abstract Assembly CompileAssembly(string source, StringBuilder sb);

    public abstract string BaseCode { get; }
}