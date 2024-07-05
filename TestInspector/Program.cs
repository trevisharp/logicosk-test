using System;
using System.Drawing;
using System.Threading;
using System.Collections.Generic;

using Pamella;
using Logicosk;

App.Open(new QuestionsView("result.test"));

class QuestionsView(string path) : View
{
    Test test = null;
    Dictionary<Question, Alternative> awnsers = new Dictionary<Question, Alternative>();
    Dictionary<string, Image> imgs = new Dictionary<string, Image>();
    int current = 0;
    int selected = 0;
    bool showImage = false;
    int jump = 80;
    int spacing = 60;
    DateTime escTime = DateTime.MaxValue;
    DateTime testFinal;
    protected override void OnStart(IGraphics g)
    {
        new Thread(async () => {
            test = await TestManager.Open(path);
            testFinal = DateTime.Now.Add(
                TimeSpan.FromMinutes(test.MinutesDuration)
            );
        }).Start();

        AlwaysInvalidateMode();
        g.SubscribeKeyDownEvent(key => {
            if (key == Input.Escape && escTime == DateTime.MaxValue)
                escTime = DateTime.Now;

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
                    current--;
                    if (current < 0)
                        current = this.test.Questions.Count - 1;
                    break;
                
                
                case Input.Right:
                    current++;
                    if (current >= this.test.Questions.Count)
                        current = 0;
                    break;
                

                case Input.I:
                    showImage = !showImage;
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


                case Input.Enter:
                    var question = test.Questions[current];
                    var awnser = question.Alternatives[selected];

                    if (awnsers.ContainsKey(question))
                        awnsers[question] = awnser;
                    else awnsers.Add(question, awnser);
                    break;
            }

        });
        g.SubscribeKeyUpEvent(key => {
            if (key == Input.Escape)
                escTime = DateTime.MaxValue;
        });
    }

    protected override void OnFrame(IGraphics g)
    {
        var time = DateTime.Now - escTime;
        if (time.TotalSeconds > 2f)
            App.Close();
    }

    protected override void OnRender(IGraphics g)
    {
        g.Clear(Color.SkyBlue);
        if (test is null)
            return;
        
        var question = this.test.Questions[current];
        var font = new Font("Arial", 40);

        g.DrawText(
            new Rectangle(5, 5, g.Width - 10, g.Height - 10),
            font, StringAlignment.Near, StringAlignment.Near,
            question.Text
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
            bool isAnswer = awnsers.ContainsKey(question) && awnsers[question] == alternative;
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
            App.Open(new PraticalView(test, awnsers));
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

class PraticalView(Test test, Dictionary<Question, Alternative> awnsers) : View
{
    
}