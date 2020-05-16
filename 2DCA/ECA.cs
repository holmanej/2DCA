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
    class ECA
    {
        public int[,] Field;
        private Point[] Neighborhood = { new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(0, 1), new Point(-1, 1), new Point(-1, 0), new Point(-1, -1), new Point(0, -1), new Point(1, -1) };

        public ECA(string rule, int density, bool random, Size area)
        {
            string[] parts = rule.Split('/');
            int[] birth = Array.ConvertAll(parts[0].Substring(1).ToArray(), item => (int)item - '0');
            int[] survival = Array.ConvertAll(parts[1].Substring(1).ToArray(), item => (int)item - '0');
            foreach (int b in birth)
            {
                Debug.WriteLine(b);
            }
            foreach (int b in survival)
            {
                Debug.WriteLine(b);
            }

            Field = new int[area.Width, area.Height];

            if (random)
            {
                Random rand = new Random();
                for (int i = 0; i < area.Width - 1; i++)
                {
                    for (int j = 0; j < area.Height; j++)
                    {
                        Field[i, j] = rand.Next(0, 100) < density ? 1 : 0;
                    }
                }
            }
            else
            {
                Field[area.Width / 2, area.Height / 2] = 1;
            }

            int[] neighbors = new int[9];
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int border = 1; border < (area.Height / 2) - 1; border++)
            {
                for (int i = (area.Height / 2) - border; i < (area.Height / 2) + border; i++)
                {
                    Parallel.For((area.Width / 2) - border, (area.Width / 2) + border, (int j) =>
                    {
                        for (int p = 0; p < Neighborhood.Count(); p++)
                        {
                            if (Field[j + Neighborhood[p].X, i + Neighborhood[p].Y] == 1)
                            {
                                neighbors[p] = 1;
                            }
                            else
                            {
                                neighbors[p] = 0;
                            }
                        }
                        if (birth.Any(c => neighbors[c] == 1))
                        {
                            Field[j, i] = 1;
                        }
                        if (survival.All(c => neighbors[c] == 0))
                        {
                            Field[j, i] = 0;
                        }
                    });
                }
            }
            stopwatch.Stop();
            Debug.WriteLine(stopwatch.Elapsed);
        }
    }
}
