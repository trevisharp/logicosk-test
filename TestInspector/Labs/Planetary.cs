using System;
using System.Drawing;
using System.Collections.Generic;

using Pamella;

public class Planetary : Lab
{
    public class Planet
    {
        public Brush Brush { get; set; }
        public float Mass { get; set; } // Kg
        public float X { get; set; } // 1'000 Km
        public float Y { get; set; } // 1'000 Km
        public float VelX { get; set; } // 1'000 Km / h
        public float VelY { get; set; } // 1'000 Km / h
        public float Radius { get; set; } // 1'000 Km
    }

    List<Planet> planets = [];
    public override void LoadParams(List<string> args)
    {
        foreach (var arg in args)
        {
            var data = arg.Split(" ");
            if (data.Length == 0)
                return;
            
            if (data[0] == "add")
                add(data[1..]);
        }

        void add(string[] args)
        {
            if (args.Length == 0)
                return;
            
            if (args[1] == "earth-moon")
            {
                planets.Add(earth(0, 0, 0, 0));
                planets.Add(moon(384.4f, 0, 0, 3.6944f)); // 1.03 km/s * 3600s/h / 1000km
            }
        }
    }

    public override void Draw(IGraphics g)
    {
        int cx = g.Width / 2;
        int cy = g.Height / 2;
        g.Clear(Color.Black);

        foreach (var planet in planets)
        {
            g.FillPolygon(
                circle(cx + planet.X, cy + planet.Y, planet.Radius),
                planet.Brush
            );
        }
    }

    PointF[] circle(float x, float y, float radius)
    {
        var pts = new List<PointF>();

        for (int i = 0; i < 90; i++)
        {
            float angle = MathF.PI * 4 * i / 180;
            pts.Add(new PointF {
                X = x + MathF.Cos(angle),
                Y = y + MathF.Sin(angle)
            });
        }

        return [ ..pts ];
    }

    public override void LoadBehaviour(Type code)
    {

    }

    Planet earth(float x0, float y0, float vx0, float vy0)
        => new Planet {
            Brush = Brushes.Blue,
            Mass = 5.9736e24f,
            X = x0,
            Y = y0,
            VelX = vx0,
            VelY = vy0,
            Radius = 6.3781f
        };

    Planet moon(float x0, float y0, float vx0, float vy0)
        => new Planet {
            Brush = Brushes.White,
            Mass = 7.349e22f,
            X = x0,
            Y = y0,
            VelX = vx0,
            VelY = vy0,
            Radius = 1.7374f
        };
}