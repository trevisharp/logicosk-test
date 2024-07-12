using System;
using System.Collections.Generic;

using Pamella;

public abstract class Lab
{
    public static Lab New(string name) =>
        name switch
        {
            "planetary" => new Planetary(),
            _ => throw new NotImplementedException($"unknown lab '{name}'.")
        };

    public abstract void LoadParams(List<string> args);
    public abstract void LoadBehaviour(Type code);
    public abstract void Draw(IGraphics g);
    public abstract void Interact(float dt);
    public abstract void Reset();
}