using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

using Pamella;

public class Validator : Lab
{
    bool customConvert = false;
    Func<dynamic, dynamic> convert = input =>
    {
        string alphaNumeric = input;
        string numeric = string.Concat(alphaNumeric.Where(c => c >= '0' && c <= '9'));
        return long.Parse(numeric);
    };

    bool customValidate = false;
    Func<dynamic, dynamic> validate = input =>
    {
        long cpf = input;
        if (cpf.ToString().Length != 11)
            return false;
        
        long mod = 10_000_000_000;
        long wei = 10;
        long sum = 0;
        for (var i = 0; i < 9; i++)  {
            sum += wei * cpf / mod % 10;
            mod /= 10;
            wei--;
        }
        long rest = sum % 11;
        long digit = rest < 2 ? 0 : 11 - rest;

        if (cpf / mod % 10 != digit)
            return false;

        mod = 10_000_000_000;
        wei = 11;
        sum = 0;
        for (var i = 0; i < 10; i++)  {
            sum += wei * cpf / mod % 10;
            mod /= 10;
            wei--;
        }
        rest = sum % 11;
        digit = rest < 2 ? 0 : 11 - rest;

        return cpf / mod % 10 == digit;
    };
    
    public override float Avaliate()
    {
        throw new NotImplementedException();
    }

    public override void Draw(IGraphics g)
    {
        throw new NotImplementedException();
    }

    public override void Interact(float dt)
    {
        throw new NotImplementedException();
    }

    public override void LoadBehaviour(Type code)
    {
        throw new NotImplementedException();
    }

    public override void LoadParams(List<string> args)
    {
        throw new NotImplementedException();
    }

    public override void Reset()
    {
        throw new NotImplementedException();
    }
}