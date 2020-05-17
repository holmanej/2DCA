using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2DCA
{
    class _2DCA
    {
        public int[,] Field;
        private Point[] Neighborhood = { new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(0, 1), new Point(-1, 1), new Point(-1, 0), new Point(-1, -1), new Point(0, -1), new Point(1, -1) };
        private int[] Birth;
        private int[] Survival;
        private Size Area;
        public long CalcTime;

        public _2DCA(string rule, int density, bool random, Size area)
        {
            string[] parts = rule.Split('/');
            Survival = Array.ConvertAll(parts[0].ToArray(), item => (int)item - '0');
            Birth = Array.ConvertAll(parts[1].ToArray(), item => (int)item - '0');
            //foreach (int b in Birth)
            //{
            //    Debug.WriteLine(b);
            //}
            //foreach (int b in Survival)
            //{
            //    Debug.WriteLine(b);
            //}

            Area = area;
            Field = new int[Area.Width, Area.Height];

            if (random)
            {
                Random rand = new Random();
                for (int i = 0; i < Area.Width - 1; i++)
                {
                    for (int j = 0; j < Area.Height; j++)
                    {
                        Field[i, j] = rand.Next(0, 100) < density ? 1 : 0;
                    }
                }
            }
            else
            {
                Field[Area.Width / 2, Area.Height / 2] = 1;
            }
        }

        public void NextCycle()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            int[,] newField = new int[Area.Width, Area.Height];
            for (int i = 1; i < Area.Height - 2; i++)
            {
                for (int j = 1; j < Area.Width - 2; j++)
                {
                    int neighbors = 0;
                    for (int p = 0; p < Neighborhood.Count(); p++)
                    {
                        if (Field[j + Neighborhood[p].X, i + Neighborhood[p].Y] == 1)
                        {
                            neighbors++;
                        }
                    }
                    if (Birth.Any(c => c == neighbors))
                    {
                        newField[j, i] = 1;
                    }
                    else if (Survival.All(c => c != neighbors))
                    {
                        newField[j, i] = 0;
                    }
                }
            }
            Field = newField;
            stopwatch.Stop();
            CalcTime = stopwatch.ElapsedMilliseconds;
            Debug.WriteLine(stopwatch.Elapsed);
        }
    }
}
