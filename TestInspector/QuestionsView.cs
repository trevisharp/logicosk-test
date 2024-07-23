using System;
using System.Drawing;
using System.Collections.Generic;

using Pamella;
using Logicosk;

class QuestionsView(Action<Input> oldEvent) : View
{
    Test test = Results.Current.Test;
    Dictionary<string, Image> imgs = new Dictionary<string, Image>();
    int current = 0;
    int selected = 0;
    bool showImage = false;
    bool waitingEnd = false;
    int jump = 80;
    int spacing = 60;
    DateTime fTime = DateTime.MaxValue;
    DateTime testFinal;
    Action<Input> oldKeyEventDown = null;
    Action<Input> oldKeyEventUp = null;
    protected override void OnStart(IGraphics g)
    {
        g.UnsubscribeKeyDownEvent(oldEvent);
        testFinal = DateTime.Now.Add(
            TimeSpan.FromMinutes(test.MinutesDuration)
        );

        foreach (var q in test.Questions)
            Results.Current.Answers.Add(q, null);

        AlwaysInvalidateMode();
        g.SubscribeKeyDownEvent(oldKeyEventDown = key => {

            if (test is null)
                return;

            switch (key)
            {
                case Input.Down:
                    selected++;
                    if (selected >= test.Questions[current].Alternatives.Count)
                        selected = 0;
                    break;
                

                case Input.Up:
                    selected--;
                    if (selected < 0)
                        selected = test.Questions[current].Alternatives.Count - 1;
                    break;
                

                case Input.Left:
                    showImage = false;
                    current--;
                    if (current < 0)
                        current = test.Questions.Count - 1;
                    break;
                
                
                case Input.Right:
                    showImage = false;
                    current++;
                    if (current >= test.Questions.Count)
                        current = 0;
                    break;
                

                case Input.I:
                    showImage = !showImage;
                    break;

                case Input.F:
                    if (fTime == DateTime.MaxValue) {
                        fTime = DateTime.Now;
                    }

                    if (!waitingEnd)
                        waitingEnd = true;
                    break;

                case Input.W:
                    jump--;
                    break;


                case Input.S:
                    jump++;
                    break;
                
                
                case Input.A:
                    spacing--;
                    break;
                

                case Input.Q:
                    spacing++;
                    break;


                case Input.Space:
                    if (waitingEnd)
                        return;

                    var question = test.Questions[current];
                    var awnser = question.Alternatives[selected];

                    if (!Results.Current.Answers.TryAdd(question, awnser))
                        Results.Current.Answers[question] = awnser;
                    break;
            }

        });
        g.SubscribeKeyUpEvent(oldKeyEventUp = key => {
            if (key == Input.F)
            {
                var time = DateTime.Now - fTime;
                if (time.TotalSeconds > 2f)
                {
                    App.Clear();
                    App.Push(new SecondView(oldKeyEventDown, oldKeyEventUp));
                    return;
                }

                waitingEnd = !waitingEnd;
                fTime = DateTime.MaxValue;
            }
        });
    }

    protected override void OnRender(IGraphics g)
    {
        g.Clear(Color.SkyBlue);
        var time = DateTime.Now - fTime;
        if (test is null)
            return;
        
        if (waitingEnd)
        {
            g.DrawText(
                new Rectangle(5, 5, g.Width - 10, g.Height - 10),
                new Font("Arial", 140), 
                StringAlignment.Center, StringAlignment.Center,
                "Finalizando..."
            );
            g.DrawText(
                new Rectangle(5, g.Height - 200, g.Width - 10, 200),
                new Font("Arial", 20), 
                StringAlignment.Center, StringAlignment.Center,
                time.TotalSeconds > 2f ? "Largue o botão F para avançar." :
                """
                Continue segurando o botão F para finalizar a prova com antecedência.
                Largue o botão F para voltar a realizar a prova.
                """
            );
            timeCheck();
            return;
        }
        
        var font = new Font("Arial", 40);
        var question = test.Questions[current];

        g.DrawText(
            new Rectangle(5, 5, g.Width - 10, g.Height - 10),
            font, StringAlignment.Near, StringAlignment.Near,
            $"{current + 1}) {question.Text}"
        );
        int y = 5 + jump * question.Text.Length / spacing;

        if (question.Image is not null)
        {
            g.DrawText(
                new Rectangle(5, y, g.Width - 10, g.Height - 10),
                new Font("Arial", 20), StringAlignment.Near, StringAlignment.Near,
                "Pressione i para ver a imagem"
            );
            y += 40;
        }
        
        if (showImage && question.Image is not null)
        {
            g.DrawImage(
                new RectangleF(5, 5, g.Width - 10, g.Height - 10),
                getImage(question.Image)
            );
            return;
        }

        int index = 0;
        foreach (var alternative in question.Alternatives)
        {
            var text = alternative.Text;
            if (selected == index)
                g.FillRectangle(
                    5, y, g.Width - 10, jump * (text.Length / spacing + 1), Brushes.Black);
            bool isAnswer = Results.Current.Answers.ContainsKey(question) && Results.Current.Answers[question] == alternative;
            g.DrawText(
                new Rectangle(5, y, g.Width - 10, g.Height - y - 5),
                font, StringAlignment.Near, StringAlignment.Near,
                (selected == index, isAnswer) switch
                {
                    (true, true) => Brushes.Orange,
                    (true, false) => Brushes.SkyBlue,
                    (false, true) => Brushes.DarkOrange,
                    (false, false) => Brushes.Black
                }, text
            );
            y += jump * (text.Length / spacing + 1);
            index++;
        }

        timeCheck();

        void timeCheck()
        {
            var timeFont = new Font("Arial", 16);
            var remainingTime = testFinal - DateTime.Now;
            g.DrawText(
                new RectangleF(g.Width - 120, g.Height - 30, 120, 30),
                timeFont, remainingTime.TotalMinutes switch
                {
                    <= 15 and > 5 => Brushes.Yellow,
                    <= 5 and > 1 => Brushes.Orange,
                    <= 1 => Brushes.Red,
                    _ => Brushes.Black
                },
                $"{remainingTime.Hours:00}:{remainingTime.Minutes:00}:{remainingTime.Seconds:00}"
            );

            if (DateTime.Now > testFinal)
            {
                App.Pop();
                App.Open(new SecondView(oldKeyEventDown, oldKeyEventUp));
            }
        }
    }

    Image getImage(string key)
    {
        if (imgs.TryGetValue(key, out Image value))
            return value;
        
        var img = Image.FromFile(key);
        imgs[key] = img;
        return img;
    }
}