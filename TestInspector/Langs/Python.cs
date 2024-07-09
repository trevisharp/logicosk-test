using System;
using System.Collections.Generic;
using System.Text;

public class Python : RealLanguage
{
    public override string BaseCode =>
        """
        
        """;

    public override Func<T, R> Compile<T, R>(string source, StringBuilder sb)
    {
        return null;
    }
}