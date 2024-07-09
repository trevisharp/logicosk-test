using System.Text;
using System.Linq;
using System.Collections.Generic;

public class ExtendedBrainfuck : PseudoLanguage
{
    public override Dictionary<string, string> Tutorial() => new(){
        { "Variávies",
            """
            No ebf todas as variáveis são ponteiros que podem ser acessados e
            declarados usando a função goto:
            @
            main:
                goto myVar
            @
            Depois de usar goto para acessar o ponteiro de uma variável qualquer
            valor digitado define seus dados:
            @
            main:
                goto myVar
                8 // agora myVar vale 8
                goto myOtherVar
                myVar // agora myOtherVar vale 8 também
            @
            """
        },
        { "Operações", 
            """
            Ao acessar uma variável, você pode incrementar seu valor
            ou decrementá-lo da seguinte forma:
            @
            main:
                goto var
                8 // var vale 8 aqui
                + // var vale 9 aqui
                + // var vale 10 aqui
                + // var vale 11 aqui
                - // var vale 10 aqui
            @
            """
        },
        { "Funções",
            """
            Todo programa ebf precisa de uma função main! Declarando funções:
            @
            main:
                // código aqui
            nomedafunção:
                // código aqui
            outrafunção:
                // código aqui
            @
            Chamando funções:
            @
            main:
                call myfunc parametros
            myfunc:
                // código aqui
            @
            """
        },
        { "Parâmetros",
            """
            Para acessar os parâmetros, em ordem, que foram enviados para função use
            o caractér especial '.' (ponto).
            @
            main:
                goto valor
                4
                call func valor

            func:
                goto variavel
                . // variavel vale 4
            @
            """
        },
        { "Retornos",
            """
            Toda função tem uma variável especial chamada result que armazena o
            retorno da função, basta usá-lo. Inclusive a variável que está sendo
            usada no inicio do código sempre será a result:
            @
            main:
                goto var
                5
                goto result
                call somadois var // result vale 5 + 2 = 7
            somadois:
                goto result // desnecessário, visto que começamos usando o result
                . // passa a entrada da função para result
                +
                + // soma dois
                exit // retorna o result
            """
        },
        { "Fluxo",
            """
            Você pode utilizar de "[" e "]" para criar whiles:
            @
            main:
                10 // result vale 10
                [ // enquanto result != 0, repita
                    -
                ]

                ![
                    + // enquanto result == 0 repita
                ]
            @
            Você pode fazer um 'if' utilizando deste artefato:
            @
            add:
            .
            goto other
            .
            ![
                exit // se other é zero, retorna
            ]
            [
                -
                goto result
                +
                goto other
            ]
            exit
            @
            """
        }
    };

    protected override string convert(string source, StringBuilder sb)
    {
        var code = new StringBuilder();
        code.AppendLine("using System.Collections.Generic;");
        code.AppendLine("TestePratico.main(1, 2, 3);");
        code.AppendLine("public static class TestePratico {");

        var lines = source.Split("\n");
        string currentFunc = null;
        Dictionary<string, int> pointerDict = null;
        int count = 0;
        foreach (var line in lines.Select(x => x.Trim()))
        {
            count++;
            if (string.IsNullOrEmpty(line))
                continue;
            
            if (line.EndsWith(":")) {
                if (currentFunc is not null)
                    code.AppendLine("}");

                pointerDict = new Dictionary<string, int> {
                    { "result", 0 }
                };
                currentFunc = line.Replace(":", "");
                code.AppendLine($"public static object {currentFunc}(params object[] inputs) {{");
                code.AppendLine("List<int> memory = [ 0 ];");
                code.AppendLine("int pointer = 0;");
                code.AppendLine("int inputIndex = 0;");
                continue;
            }

            if (line == "exit") {
                code.AppendLine("return memory[0];");
                continue;
            }

            if (line.StartsWith("goto")) {
                var variable = line.Split(" ").LastOrDefault();
                if (!pointerDict.ContainsKey(variable))
                {
                    pointerDict.Add(variable, pointerDict.Keys.Count);
                    code.AppendLine("memory.Add(0);");
                }
                int pointer = pointerDict[variable];
                code.AppendLine($"pointer = {pointer};");
                continue;
            }

            if (line.StartsWith(".")) {
                code.AppendLine($"memory[pointer] = (int)inputs[inputIndex];");
                code.AppendLine($"inputIndex++;");
                continue;
            }

            if (line == "[")
            {
                code.AppendLine("while(memory[pointer] != 0) {");
                continue;
            }

            if (line == "![")
            {
                code.AppendLine("while(memory[pointer] == 0) {");
                continue;
            }

            if (line == "]")
            {
                code.AppendLine("}");
                continue;
            }

            if (line == "-")
            {
                code.AppendLine($"memory[pointer]--;");
                continue;
            }

            if (line == "+")
            {
                code.AppendLine($"memory[pointer]++;");
                continue;
            }

            if (line.StartsWith("call"))
            {
                var data = line.Split(" ");
                code.Append($"memory[pointer] = (int){data[1]}(");
                for (int i = 2; i < data.Length; i++)
                {
                    var variable = data[i];
                    if (!pointerDict.ContainsKey(variable))
                    {
                        sb.AppendLine($"unknow variable {variable} at line {count}.");
                        continue;
                    }
                    code.Append($"memory[{pointerDict[variable]}]");
                    if (i + 1 < data.Length)
                        code.Append(", ");
                }
                code.AppendLine($");");
                continue;
            }

            if (int.TryParse(line, out int num))
            {
                code.AppendLine($"memory[pointer] = {num};");
                continue;
            }

            if (!pointerDict.ContainsKey(line))
            {
                sb.AppendLine($"unknow variable {line} at line {count}.");
                continue;
            }
            code.AppendLine($"memory[pointer] = memory[{pointerDict[line]}];");
        }

        if (currentFunc is not null)
            code.AppendLine("}");
        code.AppendLine("}");
        return code.ToString();
    }
}