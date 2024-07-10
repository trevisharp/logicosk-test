using System;
using System.Drawing;
using System.Collections.Generic;

using Pamella;
using System.Linq;

public class Planetary : Lab
{
    public class Planet
    {
        public Brush Brush { get; set; }
        public float Mass { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float VelX { get; set; }
        public float VelY { get; set; }
        public float Radius { get; set; }
    }

    List<Planet> planets = [];
    int speed = 3600;
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
            
            if (args[0] == "earth-moon")
            {
                planets.Add(earth(0, 0, 0, 0));
                planets.Add(moon(384_400_000f, 0, 0, 1030));
            }
        }
    }

    Func<dynamic, dynamic, dynamic, dynamic, dynamic> dist = (xp, yp, xq, yq) =>
    {
        var deltaX = xp - xq;
        var deltaY = yp - yq;
        var mod = deltaX * deltaX;
        return MathF.Sqrt(mod);
    };

    public override void Interact(float dt)
    {
        dt *= speed;
        var pairs = 
            from p in planets
            from q in planets
            where p != q
            select (p, q);

        foreach (var (p, q) in pairs)
        {
            float distance = dist(p.X, p.Y, q.X, q.Y);
            const float G = 6.6743e-11f; // m3 kg-1 s-2;
            float force = G * p.Mass * q.Mass / (distance * distance);
            
            float dx = p.X - q.X;
            float dy = p.Y - q.Y; 
            dx /= distance + 1f;
            dy /= distance + 1f;

            q.VelX += dx * force / q.Mass * dt;
            q.VelY += dy * force / q.Mass * dt;
        }

        foreach (var planet in planets)
        {
            planet.X += planet.VelX * dt;
            planet.Y += planet.VelY * dt;
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
        float graphRadius = radius / 100 / 1000;

        for (int i = 0; i < 90; i++)
        {
            float angle = MathF.PI * 4 * i / 180;
            float realX = x + MathF.Cos(angle) * graphRadius;
            float realY = y + MathF.Sin(angle) * graphRadius;
            pts.Add(new PointF {
                X = realX / 1000 / 1000,
                Y = realY / 1000 / 1000
            });
        }

        return [ ..pts ];
    }

    public override void LoadBehaviour(Type code)
    {

    }

    Planet earth(float x0, float y0, float vx0, float vy0)
        => new() {
            Brush = Brushes.Blue,
            Mass = 5.9736e24f,
            X = x0,
            Y = y0,
            VelX = vx0,
            VelY = vy0,
            Radius = 6.3781e6f
        };

    Planet moon(float x0, float y0, float vx0, float vy0)
        => new() {
            Brush = Brushes.White,
            Mass = 7.349e22f,
            X = x0,
            Y = y0,
            VelX = vx0,
            VelY = vy0,
            Radius = 1.7374e6f
        };
}