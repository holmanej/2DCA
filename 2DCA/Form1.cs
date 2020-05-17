using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _2DCA
{
    public partial class Form1 : Form
    {

        private _2DCA eca;
        private Bitmap Pattern = new Bitmap(1, 1);
        ListBox File_ListBox;
        private int DrawWidth = 720;
        private int DrawHeight = 720;
        private int PixelSize = 4;
        private Bitmap Initial;
        private Rectangle RenderArea;
        private Rectangle DrawArea;
        Timer cycleTick = new Timer();

        public Form1()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            Paint += Form1_Paint;

            Panel controlPanel = new Panel()
            {
                Location = Location,
                Size = new Size(200, 1000)
            };
            Controls.Add(controlPanel);
            Label rule_Label = new Label()
            {
                Location = new Point(20, 20),
                Size = new Size(150, 20),
                Text = "Rule (B/S)"
            };
            controlPanel.Controls.Add(rule_Label);
            TextBox rule_TextBox = new TextBox()
            {
                Location = new Point(20, 50),
                Size = new Size(100, 20),
                Text = "3/23"
            };
            controlPanel.Controls.Add(rule_TextBox);

            Label random_Label = new Label()
            {
                Location = new Point(20, 100),
                Size = new Size(150, 20),
                Text = "Density Percentage [0-100]"
            };
            controlPanel.Controls.Add(random_Label);
            CheckBox random_CheckBox = new CheckBox()
            {
                Location = new Point(20, 130),
                Size = new Size(20, 20)
            };
            controlPanel.Controls.Add(random_CheckBox);
            TextBox random_TextBox = new TextBox()
            {
                Location = new Point(50, 130),
                Text = "50"
            };
            controlPanel.Controls.Add(random_TextBox);

            Button generate_Button = new Button()
            {
                Location = new Point(20, 160),
                Size = new Size(100, 30),
                Text = "Generate!"
            };
            generate_Button.Click += Generate_Button_Click;
            controlPanel.Controls.Add(generate_Button);

            File_ListBox = new ListBox()
            {
                Location = new Point(20, 200),
                Size = new Size(100, 400)
            };
            File_ListBox.DoubleClick += File_ListBox_DoubleClick;
            controlPanel.Controls.Add(File_ListBox);

            Button SavePattern_Button = new Button()
            {
                Location = new Point(20, 700),
                Size = new Size(100, 30),
                Text = "Save Result"
            };
            SavePattern_Button.Click += SavePattern_Button_Click;
            controlPanel.Controls.Add(SavePattern_Button);

            File_ListBox.Items.AddRange(Directory.GetFiles(".", "*.bmp").Select(f => f.Substring(2)).ToArray());

            RenderArea = new Rectangle(0, 0, (DrawWidth / PixelSize), (DrawHeight / PixelSize));
            DrawArea = new Rectangle(0, 0, DrawWidth, DrawHeight);
            Initial = new Bitmap(DrawWidth / PixelSize, DrawHeight / PixelSize, PixelFormat.Format24bppRgb);
        }

        private void SavePattern_Button_Click(object sender, EventArgs e)
        {
            Bitmap p = new Bitmap(180, 180);
            for (int i = 0; i < 180; i++)
            {
                for (int j = 0; j < 180; j++)
                {
                    if (eca.Field[j, i] == 1)
                    {
                        p.SetPixel(j, i, Color.Black);
                    }
                    else
                    {
                        p.SetPixel(j, i, Color.White);
                    }
                }
            }
            p.Save("result.bmp", ImageFormat.Bmp);
        }

        private void File_ListBox_DoubleClick(object sender, EventArgs e)
        {
            cycleTick.Stop();
            ListBox LB = (ListBox)sender;
            
            Bitmap img = new Bitmap(LB.SelectedItem.ToString());
            Bitmap result = new Bitmap("template.bmp");
            Graphics g = Graphics.FromImage(result);
            g.DrawImage(img, 0, 0, img.Width, img.Height);
            Graphics gfx = CreateGraphics();
            Rectangle drawRect = new Rectangle((Width - 200 - 720) / 2, 5, 720, 720);
            Initial = result;
            gfx.DrawImage(result, drawRect);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics gfx = CreateGraphics();
            Size area = new Size(Width - 200, Height);
            SolidBrush grayBrush = new SolidBrush(Color.Gray);
            gfx.FillRectangle(grayBrush, 200, 0, area.Width, area.Height);
            gfx.DrawImage(Pattern, (Width - 200 - 720) / 2, 5);
        }

        private void Generate_Button_Click(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            var CC = b.Parent.Controls;
            //foreach (Control c in b.Parent.Controls)
            //{
            //    Debug.WriteLine(c.Text);
            //}
            CheckBox cb = (CheckBox)CC[3];
            string rule = CC[1].Text;
            int density = int.Parse(CC[4].Text);
            if (density < 0)
            {
                density = 0;
                CC[4].Text = "0";
            }
            if (density > 100)
            {
                density = 100;
                CC[4].Text = "100";
            }

            Pattern = new Bitmap(DrawArea.Width, DrawArea.Height, PixelFormat.Format32bppRgb);
            Graphics gfx = CreateGraphics();
            SolidBrush grayBrush = new SolidBrush(Color.Gray);
            gfx.FillRectangle(grayBrush, b.Parent.Width, 0, Width - b.Parent.Width, Height);

            eca = new _2DCA(rule, density, cb.Checked, Initial);

            cycleTick.Interval = 500;
            cycleTick.Tick += CycleTick_Tick;
            cycleTick.Start();
            //eca.NextCycle();
            DrawCycle();
        }

        private void CycleTick_Tick(object sender, EventArgs e)
        {
            eca.NextCycle();
            DrawCycle();
            if (eca.CalcTime > 33)
            {
                cycleTick.Interval = (int)(eca.CalcTime * 1.1);
            }
            else
            {
                cycleTick.Interval = 33;
            }
        }

        private void DrawCycle()
        {
            BitmapData bmpData = Pattern.LockBits(DrawArea, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * Pattern.Height;
            byte[] patternValues = new byte[bytes];
            Marshal.Copy(ptr, patternValues, 0, bytes);

            for (int i = 0; i < DrawArea.Height - 1; i++)
            {
                Parallel.For(0, DrawArea.Width - 1, (j) =>
                {
                    int coordinate = (i * 4 * PixelSize * RenderArea.Width) + (j * 4);
                    if (eca.Field[j / PixelSize, i / PixelSize] == 1)
                    {
                        patternValues[coordinate] = 0;
                        patternValues[coordinate + 1] = 0;
                        patternValues[coordinate + 2] = 0;
                        patternValues[coordinate + 3] = 255;

                    }
                    else
                    {
                        patternValues[coordinate] = 255;
                        patternValues[coordinate + 1] = 255;
                        patternValues[coordinate + 2] = 255;
                        patternValues[coordinate + 3] = 255;
                    }
                });
            }

            Marshal.Copy(patternValues, 0, ptr, bytes);
            Pattern.UnlockBits(bmpData);
            Graphics gfx = CreateGraphics();
            gfx.DrawImage(Pattern, (Width - 200 - 720) / 2, 5);
        }
    }
}
