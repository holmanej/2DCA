using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
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
        private Rectangle renderArea = new Rectangle(0, 0, 320, 240);
        Timer cycleTick = new Timer();

        public Form1()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            Paint += Form1_Paint;

            Panel controlPanel = new Panel()
            {
                Location = Location,
                Size = new Size(200, Height)
            };
            Controls.Add(controlPanel);
            Label rule_Label = new Label()
            {
                Location = new Point(20, 20),
                Size = new Size(150, 20),
                Text = "Rule (S/B)"
            };
            controlPanel.Controls.Add(rule_Label);
            TextBox rule_TextBox = new TextBox()
            {
                Location = new Point(20, 50),
                Size = new Size(100, 20),
                Text = "1/123"
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

            TextBox log_TextBox = new TextBox()
            {
                Location = new Point(20, 200),
                Size = new Size(100, 100),
                Multiline = true,
                ReadOnly = true
            };
            controlPanel.Controls.Add(log_TextBox);
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics gfx = CreateGraphics();
            Size area = new Size(Width - 200, Height);
            SolidBrush grayBrush = new SolidBrush(Color.Gray);
            gfx.FillRectangle(grayBrush, 200, 0, area.Width, area.Height);
            gfx.DrawImage(CropPattern(), 205, 5);
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

            TextBox log = (TextBox)CC[6];
            log.Clear();

            Pattern = new Bitmap(renderArea.Width, renderArea.Height, PixelFormat.Format32bppRgb);
            Graphics gfx = CreateGraphics();
            SolidBrush grayBrush = new SolidBrush(Color.Gray);
            gfx.FillRectangle(grayBrush, b.Parent.Width, 0, Width - b.Parent.Width, Height);

            eca = new _2DCA(rule, density, cb.Checked, renderArea.Size);

            cycleTick.Interval = 500;
            cycleTick.Tick += CycleTick_Tick;
            cycleTick.Start();
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
            BitmapData bmpData = Pattern.LockBits(renderArea, ImageLockMode.ReadWrite, PixelFormat.Format32bppRgb);
            IntPtr ptr = bmpData.Scan0;
            int bytes = Math.Abs(bmpData.Stride) * Pattern.Height;
            byte[] patternValues = new byte[bytes];
            Marshal.Copy(ptr, patternValues, 0, bytes);

            for (int i = 0; i < renderArea.Height - 1; i++)
            {
                Parallel.For(0, renderArea.Width - 1, (j) =>
                {
                    int coordinate = (i * 4 * renderArea.Width) + (j * 4);
                    if (eca.Field[j, i] == 1)
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
            gfx.DrawImage(CropPattern(), 205, 5);
        }

        private Bitmap CropPattern()
        {
            int w = Width - 200;
            int h = Height;
            Bitmap b = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(b);
            g.DrawImage(Pattern, new Rectangle(0, 0, w, h), new Rectangle((Pattern.Width - w) / 2, 0, w, h), GraphicsUnit.Pixel);
            return b;
        }
    }
}
