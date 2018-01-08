using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MapGen
{
    class RadialShape : IIslandShape
    {
        public const double ISLAND_FACTOR = 1.07;  // 1.0 means no small islands; 2.0 leads to a lot

        private System.Random islandRandom;
        private int bumps;
        private double startAngle;
        private double dipAngle;
        private double dipWidth;
        
        public RadialShape()
        {
            islandRandom = new System.Random();
            bumps = islandRandom.Next(1, 6);
            startAngle = islandRandom.NextDouble() * (2 * Math.PI); //random  between [0, 2*PI]
            dipAngle = islandRandom.NextDouble() * (2 * Math.PI); //random between [0, 2*PI]
            dipWidth = islandRandom.NextDouble() * (0.7 - 0.2) + 0.2; //random between [0.2, 0.7]
        }

        //make sure the point is normalized (x and y are between [-1, 1]
        public bool IsInside(Vector2 normalizedPoint)
        {
            double angle = Math.Atan2(normalizedPoint.y, normalizedPoint.x);
            double length = 0.5 * (Math.Max(Math.Abs(normalizedPoint.x), Math.Abs(normalizedPoint.y)) + normalizedPoint.magnitude);

            double r1 = 0.5 + 0.40 * Math.Sin(startAngle + bumps * angle + Math.Cos((bumps + 3) * angle));
            double r2 = 0.7 - 0.20 * Math.Sin(startAngle + bumps * angle - Math.Sin((bumps + 2) * angle));
            if (Math.Abs(angle - dipAngle) < dipWidth
                || Math.Abs(angle - dipAngle + 2 * Math.PI) < dipWidth
                || Math.Abs(angle - dipAngle - 2 * Math.PI) < dipWidth)
            {
                r1 = r2 = 0.2;
            }
            return (length < r1 || (length > r1 * ISLAND_FACTOR && length < r2));
        }
    }
}
