using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Pamella;
using Logicosk;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing.Design;

class PraticalView(
    Test test, 
    Dictionary<Question, Alternative> answers,
    Action<Input> oldDown, 
    Action<Input> oldUp) : View
{
    Dictionary<PraticalTest, float> bestResult = new();
    int current = 0;
    int page = 0;

    protected override void OnStart(IGraphics g)
    {
        g.UnsubscribeKeyDownEvent(oldDown);
        g.UnsubscribeKeyUpEvent(oldUp);

        AlwaysInvalidateMode();

        foreach (var pratical in test.PraticalTests)
        {
            var file = $"main.{pratical.Language}";
            File.Create(file).Close();
            bestResult.Add(pratical, 0f);
        }

        g.SubscribeKeyDownEvent(async key => {
            
            switch (key)
            {
                case Input.Up:
                    page--;
                    break;

                    
                case Input.Down:
                    page++;
                    break;

                    
                case Input.Left:
                    current--;
                    break;

                    
                case Input.Right:
                    current++;
                    break;
                
                
                case Input.Space:
                    var pratical = test.PraticalTests[current % test.PraticalTests.Count];
                    var compiler = PseudoLanguage.New(pratical.Language);
                    
                    var file = $"main.{pratical.Language}";
                    var code = File.ReadAllText(file);
                    
                    var sb = new StringBuilder();
                    var func = compiler.Compile<object[], object>(code, sb);
                    if (sb.Length > 0)
                    {
                        System.Windows.Forms.MessageBox.Show(sb.ToString());
                        return;
                    }
                    if (func is null)
                        return;
                    
                    float corrects = 0f;
                    var resutls = new StringBuilder();
                    foreach (var test in pratical?.Tests ?? [])
                    {
                        int result = Task.WaitAny(
                            Task.Run(async () => {
                                await Task.Delay(1000);
                            }),
                            Task.Run(() => {
                                try
                                {
                                    var output = func(
                                        test.Inputs
                                            .Select(x => x is JsonElement el && 
                                                el.TryGetInt32(out int value) ? value : x)
                                            .Select(x => x is JsonElement el && 
                                                el.TryGetSingle(out float value) ? value : x)
                                            .Select(x => x is JsonElement el ? el.GetString() : x)
                                            .ToArray()
                                        );
                                    
                                    if (output.ToString() != test.Output.ToString())
                                        return;       
                                    corrects++;
                                }
                                catch (Exception ex)
                                {
                                    System.Windows.Forms.MessageBox.Show(ex.Message);
                                }
                            })
                        );
                    }

                    float pontuation = corrects / pratical.Tests.Count;
                    if (bestResult[pratical] < pontuation)
                        bestResult[pratical] = pontuation;
                    break;
            }
        });
    }

    protected override void OnRender(IGraphics g)
    {
        g.Clear(Color.GreenYellow);
    }
}