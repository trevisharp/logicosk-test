using System.Collections.Generic;

public abstract class RealLanguage : Language
{
    public override Dictionary<string, string> Tutorial()
        => new() {
            { "Linguagem Não-Esotérica", "Como linguagem real, não existe um tutorial para essa prova." }
        };
}