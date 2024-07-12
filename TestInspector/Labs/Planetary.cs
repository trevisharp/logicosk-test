using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;

using Pamella;

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
    List<string> originalArgs = [];
    int speed = 60 * 60 * 100;
    public override void LoadParams(List<string> args)
    {
        originalArgs = args;
        foreach (var arg in args)
        {
            var data = arg.Split(" ");
            if (data.Length == 0)
                return;
            
            if (data is [ "add", "earth-moon" ])
            {
                planets.Add(earth(0, 0, 0, 0));
                planets.Add(moon(384.4e6f, 0, 0, 1.03e3f));
                continue;
            }
            
            if (data is [ "custom", "dist" ])
            {
                dist = null;
                customDist = true;
            }
        }
    }

    bool customDist = false;
    Func<dynamic, dynamic, dynamic, dynamic, dynamic> dist = (xp, yp, xq, yq) =>
    {
        var deltaX = xp - xq;
        var deltaY = yp - yq;
        var mod = deltaX * deltaX + deltaY * deltaY;
        return MathF.Sqrt(mod);
    };

    public override void Interact(float dt)
    {
        if (dist is null)
            return;
        
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
                circle(
                    cx + planet.X / 1000 / 1000,
                    cy + planet.Y / 1000 / 1000,
                    planet.Radius / 200 / 1000
                ),
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
                X = x + MathF.Cos(angle) * radius,
                Y = y + MathF.Sin(angle) * radius
            });
        }

        return [ ..pts ];
    }

    public override void LoadBehaviour(Type code)
    {
        var method = code.GetMethod("dist");
        if (customDist && method is not null)
            dist = (xp, yp, xq, yq) =>
                method.Invoke(null, [xp, yp, xq, yq]);
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

    public override void Reset()
    {
        planets.Clear();
        LoadParams(originalArgs);
    }

    public override float Avaliate()
    {
        float points = 0;
        assert(dist(1, 0, 0, 0), 1, ref points);
        assert(dist(0, 1, 0, 0), 1, ref points);
        assert(dist(0, 0, 1, 0), 1, ref points);
        assert(dist(0, 0, 0, 1), 1, ref points);

        assert(dist(0, 0, 0, 0), 0, ref points);
        assert(dist(1, 1, 1, 1), 0, ref points);
        assert(dist(1, 1, 0, 0), MathF.Sqrt(2), ref points);
        assert(dist(0, 0, 1, 1), MathF.Sqrt(2), ref points);
        
        assert(dist(8, 0, -8, 0), 16, ref points);
        assert(dist(0, -8, 0, 8), 16, ref points);
        assert(dist(-6, 0, 0, -8), 10, ref points);
        assert(dist(0, 8, 6, 0), 10, ref points);
        
        float result = MathF.Sqrt(100 * 100 + 44 * 44);
        assert(dist(110, -6, 10, -50), result, ref points);
        assert(dist(90, 148, 190, 104), result, ref points);
        assert(dist(-50, 22, 50, -22), result, ref points);
        assert(dist(20, 36, 120, 80), result, ref points);

        return points / 16;
    }

    void assert(float expected, float result, ref float points)
        => points += gauss(expected - result);

    float gauss(float x)
        => MathF.Exp(-x * x / 3);
}